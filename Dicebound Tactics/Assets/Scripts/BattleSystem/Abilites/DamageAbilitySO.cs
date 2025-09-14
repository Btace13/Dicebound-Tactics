using System.Collections;
using Unity;
using UnityEngine;
using TacticsToolkit;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/DamageAbility")]
public class DamageAbilitySO : AbilitySO
{
    public int damageAmount;
    
    [Header("Defensive Timing System")]
    [Tooltip("How long before the hit lands can the player start the defensive sequence")]
    public float defensiveWindowDuration = 1.5f;
    [Tooltip("How long does the player have to complete the button sequence")]
    public float buttonSequenceTimeLimit = 1.0f;
    [Tooltip("Button sequence the player must press (e.g., for parry/block)")]
    public string[] requiredButtonSequence = { "LeftGamepad", "RightGamepad" };
    
    private Vector3 failSafePosition = Vector3.zero;

    public override IEnumerator Execute(Entity user, Entity target)
    {
        if (!user.SpendAP(apCost))
        {
            yield break; // Not enough AP, exit early
        }

        user.InvokeCharacterStatChanged();

        int amount = user.CalculateDamageWithModifiers(damageAmount);
        bool defensiveActionSucceeded = false; // Track defensive action result

        OverworldEntityController userController = user.GetComponent<OverworldEntityController>();
        OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();

        if (requiresMovement)
        {
            if (userController == null || enemyController == null)
            {
                // No movement controllers, but still check for defensive timing on player characters
                yield return TriggerAbilityAnimationSequenceCoroutine(user, target, amount, (wasDefended) =>
                {
                    defensiveActionSucceeded = wasDefended;
                });
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // Move toward target
            Vector3 direction = (target.transform.position - user.transform.position).normalized;
            Vector3 destination = target.transform.position - direction * range;

            bool moveDone = false;
            failSafePosition = user.transform.position;
            userController.MoveToPosition(destination, true, () => moveDone = true);
            while (!moveDone) yield return null;

            // Play attack animation and apply damage
            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, amount, (wasDefended) =>
            {
                defensiveActionSucceeded = wasDefended;
            });

            // Return to origin
            bool returnDone = false;
            userController.MoveToPosition(
                userController?.AssignedEncounterSlot?.slotTransform != null
                    ? userController.AssignedEncounterSlot.slotTransform.position
                    : failSafePosition,
                true, () =>
            {
                returnDone = true;
                if (user is CharacterManager character)
                {
                    if (!character.CanUseMoreAbilitiesThisTurn())
                    {
                        EventManager.TriggerCharacterTurnEnded(character);
                    }
                    else
                    { 
                        // Continue the turn without restarting - refocus camera and refresh UI
                        Debug.Log($"[DamageAbilitySO] Character {character.name} has more AP, continuing turn...");
                        
                        // First, ensure we end any target selection state
                        EventManager.TriggerSelectingATarget(false);
                        
                        // Trigger the same events that happen during a normal turn start
                        // This ensures camera and other systems are properly set up
                        EventManager.TriggerNewActiveEntity(character);
                        
                        // Use the new ContinueCharacterTurn method to properly handle the UI state
                        CombatManager.Instance?.CombatUIManager?.ContinueCharacterTurn(character);
                        
                        Debug.Log($"[DamageAbilitySO] Character turn continued for {character.name}");
                    }
                }
            });
            while (!returnDone) yield return new WaitForSeconds(0.8f); // Reduced from 1.6f
        }
        else
        {

            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, amount, (wasDefended) =>
            {
                defensiveActionSucceeded = wasDefended;
            });
            yield return new WaitForSeconds(0.8f); // Reduced from 1.6f
            if (user is CharacterManager character)
            {

                if (!character.CanUseMoreAbilitiesThisTurn())
                {
                    EventManager.TriggerCharacterTurnEnded(character);
                }
                else
                { 
                    // Continue the turn without restarting - refocus camera and refresh UI
                    Debug.Log($"[DamageAbilitySO] Character {character.name} has more AP, continuing turn...");
                    
                    // First, ensure we end any target selection state
                    EventManager.TriggerSelectingATarget(false);
                    
                    // Trigger the same events that happen during a normal turn start
                    // This ensures camera and other systems are properly set up
                    EventManager.TriggerNewActiveEntity(character);
                    
                    // Use the new ContinueCharacterTurn method to properly handle the UI state
                    CombatManager.Instance?.CombatUIManager?.ContinueCharacterTurn(character);
                    
                    Debug.Log($"[DamageAbilitySO] Character turn continued for {character.name}");
                }
            }
        }
    }

    private IEnumerator TriggerAbilityAnimationSequenceCoroutine(Entity user, Entity target, int damageAmount, System.Action<bool> onDefensiveResult)
    {
        OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();
        OverworldEntityController userController = user.GetComponent<OverworldEntityController>();

        UnitAnimationHandler animationHandler = userController.GetComponentInChildren<UnitAnimationHandler>(true);
        float clipTime = 0;

        if (animationHandler != null && animationHandler.AnimationData != null)
        {
            if (user.TryGetComponent(out CharacterManager characterManager))
            {
                clipTime = animationHandler.AnimationData.combatAnimations[animationHandler.EquippedWeapon].abilities[this].length;
            }
        }

        // Look at target
        userController.transform.DOLookAt(target.transform.position, 0.2f);
        yield return new WaitForSeconds(0.2f);

        // Calculate defensive window timing
        float defensiveWindowStart = Mathf.Max(0, clipTime - defensiveWindowDuration);
        bool defensiveActionSucceeded = false;
        bool damageApplied = false; // Track if damage has been applied to prevent double application

        // Play attack animation and wait for completion
        bool animDone = false;

        if (animationHandler == null)
        {
            // No animation, but still check for defensive timing ONLY for player targets being attacked by enemies
            if (target is CharacterManager && user is EnemyManager && defensiveWindowDuration > 0)
            {
                var targetController = target.GetComponent<OverworldEntityController>();
                if (targetController != null)
                {
                    targetController.StartCoroutine(HandleDefensiveTimingWindow(target, 0f, (success) =>
                    {
                        defensiveActionSucceeded = success;
                        Debug.Log($"[DamageAbilitySO] No animation - Defensive timing callback received for {target.name}. Success: {success}");
                        onDefensiveResult?.Invoke(success);
                        
                        // Only apply damage if there's no projectile - projectiles handle their own damage
                        if (ProjectileManager.Instance == null || projectileData == null)
                        {
                            if (!damageApplied)
                            {
                                ApplyDamage(damageAmount, user, target, defensiveActionSucceeded);
                                damageApplied = true;
                            }
                        }
                    }));
                }
                else
                {
                    // Apply damage without defensive timing only if no projectile
                    if (ProjectileManager.Instance == null || projectileData == null)
                    {
                        ApplyDamage(damageAmount, user, target, false);
                        damageApplied = true;
                    }
                    onDefensiveResult?.Invoke(false);
                }
            }
            else
            {
                // Apply damage without defensive timing only if no projectile
                if (ProjectileManager.Instance == null || projectileData == null)
                {
                    ApplyDamage(damageAmount, user, target, false);
                    damageApplied = true;
                }
                onDefensiveResult?.Invoke(false);
            }
            animDone = true;
        }
        else
        {
            animationHandler.UseAbility(this, time =>
            {
                // Don't apply damage immediately - wait for defensive window to complete
                // The damage will be applied after the defensive timing finishes
            }, () => { animDone = true; });

            // Start defensive timing window ONLY if the target is a player character AND the user is an enemy
            if (target is CharacterManager && user is EnemyManager && defensiveWindowDuration > 0)
            {
                // Find a MonoBehaviour to run the coroutine (using the target's controller)
                var targetController = target.GetComponent<OverworldEntityController>();
                if (targetController != null)
                {
                    targetController.StartCoroutine(HandleDefensiveTimingWindow(target, defensiveWindowStart, (success) =>
                    {
                        defensiveActionSucceeded = success;
                        Debug.Log($"[DamageAbilitySO] Defensive timing callback received for {target.name}. Success: {success}");
                        onDefensiveResult?.Invoke(success);
                        
                        // Only apply damage if there's no projectile - projectiles handle their own damage
                        if (ProjectileManager.Instance == null || projectileData == null)
                        {
                            if (!damageApplied)
                            {
                                ApplyDamage(damageAmount, user, target, defensiveActionSucceeded);
                                damageApplied = true;
                            }
                        }
                    }));
                }
                else
                {
                    // No controller found, apply damage without defensive timing only if no projectile
                    if (ProjectileManager.Instance == null || projectileData == null)
                    {
                        if (!damageApplied)
                        {
                            ApplyDamage(damageAmount, user, target, false);
                            damageApplied = true;
                        }
                    }
                    onDefensiveResult?.Invoke(false);
                }
            }
            else
            {
                // Not a valid defensive scenario (enemy target or player attacker), apply damage normally only if no projectile
                if (ProjectileManager.Instance == null || projectileData == null)
                {
                    if (!damageApplied)
                    {
                        ApplyDamage(damageAmount, user, target, false);
                        damageApplied = true;
                    }
                }
                onDefensiveResult?.Invoke(false);
            }

            if (ProjectileManager.Instance != null && projectileData != null)
            {
                Transform projectileSpawnPoint = user.transform;

                if (user.vfxSpawnPoints.ContainsKey(projectileData.projectileSpawnPoint))
                {
                    projectileSpawnPoint = user.vfxSpawnPoints[projectileData.projectileSpawnPoint];
                }

                Vector3 direction = (target.transform.position - projectileSpawnPoint.position).normalized;

                if (projectileData.castVFXObject != null)
                {
                    GameObject castVFX = Instantiate(projectileData.castVFXObject, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
                    castVFX.transform.localScale = projectileData.castVFXScale; // Adjust scale as needed
                }

                yield return new WaitForSeconds(projectileData.castTime);

                // For projectile abilities, handle defensive timing BEFORE launching projectile (only for player targets)
                bool finalDefensiveResult = defensiveActionSucceeded;
                if (target is CharacterManager && user is EnemyManager && defensiveWindowDuration > 0)
                {
                    bool defensiveTimingComplete = false;
                    
                    var targetController = target.GetComponent<OverworldEntityController>();
                    if (targetController != null)
                    {
                        Debug.Log($"[DamageAbilitySO] Starting defensive timing for projectile attack on {target.name}");
                        targetController.StartCoroutine(HandleDefensiveTimingWindow(target, 0f, (success) =>
                        {
                            finalDefensiveResult = success;
                            defensiveTimingComplete = true;
                        }));
                        
                        // Wait for defensive timing to complete before launching projectile
                        float defensiveTimeout = buttonSequenceTimeLimit + 1f; // Reduced timeout
                        float defensiveTimer = 0f;
                        while (!defensiveTimingComplete && defensiveTimer < defensiveTimeout)
                        {
                            defensiveTimer += Time.deltaTime;
                            yield return null;
                        }
                        
                        if (!defensiveTimingComplete)
                        {
                            Debug.LogWarning($"[DamageAbilitySO] Defensive timing timed out for {target.name}");
                        }
                    }
                }

                // Track projectile impact so we only apply damage on collision
                bool projectileHit = false;
                
                ProjectileManager.CreateProjectile(
                    projectileSpawnPoint.position,
                    direction,
                    Vector3.one * 0.4f,
                    projectileData,
                    projectileData.projectilePath,
                    projectileData.projectileSpeed,
                    damageAmount,
                    -1,
                    (hit) =>
                    {
                        if (projectileHit) return; // safety guard

                        // Forced hits may arrive with a RaycastHit lacking collider; trust intended target
                        Entity hitEntity = null;
                        if (hit.collider != null)
                        {
                            hit.collider.gameObject.TryGetComponent(out hitEntity);
                        }

                        if (hitEntity == null && target != null)
                        {
                            // Fallback: assume target was reached (forced impact)
                            hitEntity = target;
                        }

                        if (hitEntity == target)
                        {
                            Debug.Log($"[DamageAbilitySO] Projectile impact applying {damageAmount} damage to {target.name} (defended: {finalDefensiveResult})");
                            ApplyDamage(damageAmount, user, target, finalDefensiveResult);
                            projectileHit = true;
                        }
                    },
                    projectileData.particleObject,
                    "",
                    target.transform
                );
                
                // Wait for the projectile to hit before continuing
                float projectileTimeout = 5f; // Reduced timeout from 10f to 5f
                float projectileTimer = 0f;
                while (!projectileHit && projectileTimer < projectileTimeout)
                {
                    projectileTimer += Time.deltaTime;
                    yield return null;
                }
                
                if (!projectileHit)
                {
                    Debug.LogWarning("[DamageAbilitySO] Projectile timeout without impact - no damage applied to preserve hit accuracy");
                }
            }
        }
        // Failsafe: if animation never completes, force exit after a timeout
        float timeout = Mathf.Max(clipTime, 2f);
        float timer = 0f;
        while (!animDone && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!animDone)
        {
            // Only apply damage if it hasn't been applied yet and there's no active defensive timing
            // And only if there's no projectile system handling damage
            if (!damageApplied && (ProjectileManager.Instance == null || projectileData == null)) 
            {
                ApplyDamage(damageAmount, user, target, defensiveActionSucceeded);
                onDefensiveResult?.Invoke(defensiveActionSucceeded);
                damageApplied = true;
            }
        }
    }

    public void ApplyDamage(int amount, Entity user, Entity target)
    {
        ApplyDamage(amount, user, target, false);
    }

    public void ApplyDamage(int amount, Entity user, Entity target, bool defensiveActionSucceeded)
    {
        // Modify damage based on defensive action success
        int finalDamage = amount;
        if (defensiveActionSucceeded)
        {
            finalDamage = 0; // Reduce damage to 0 for successful defense
            Debug.Log($"[DamageAbilitySO] Defensive action succeeded! Damage reduced from {amount} to {finalDamage} for {target.name}");
        }
        else
        {
            Debug.Log($"[DamageAbilitySO] Applying {finalDamage} damage to {target.name} (defensive action: {(defensiveActionSucceeded ? "succeeded" : "failed/none")})");
        }

        target.TakeDamage(finalDamage);
        user.ApplyOverloadHit(finalDamage, target);
        user.HealOnHit(finalDamage);
    }

    /// <summary>
    /// Handles the defensive timing window where players can input button sequences
    /// </summary>
    private IEnumerator HandleDefensiveTimingWindow(Entity target, float delayBeforeWindow, System.Action<bool> onComplete)
    {
        // Wait until it's time to start the defensive window
        if (delayBeforeWindow > 0)
        {
            yield return new WaitForSeconds(delayBeforeWindow);
        }

        // Use the EventManager to request defensive prompt
        bool sequenceCompleted = false;
        bool responseReceived = false;
        
        // Trigger the defensive prompt event
        Debug.Log($"[DamageAbilitySO] Requesting defensive prompt for {target.name} with sequence: [{string.Join(", ", requiredButtonSequence)}]");
        EventManager.TriggerDefensivePromptRequested(
            target,
            requiredButtonSequence, 
            buttonSequenceTimeLimit, 
            (success) => 
            {
                Debug.Log($"[DamageAbilitySO] Defensive prompt callback received for {target.name}. Success: {success}");
                sequenceCompleted = success;
                responseReceived = true;
            }
        );
        
        // Wait for the UI sequence to complete or timeout
        float maxWaitTime = buttonSequenceTimeLimit + 2f; // Add buffer time for UI animations
        float waitTimer = 0f;
        
        while (!responseReceived && waitTimer < maxWaitTime)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }
        
        // If no UI system responded, fall back to console-based system
        if (!responseReceived)
        {
            Debug.Log("[DamageAbilitySO] No UI system responded to defensive prompt - using fallback system");
            yield return HandleDefensiveTimingFallback(target, onComplete);
        }
        else
        {
            Debug.Log($"[DamageAbilitySO] Defensive timing completed for {target.name}. Success: {sequenceCompleted}");
            onComplete?.Invoke(sequenceCompleted);
        }
    }

    /// <summary>
    /// Fallback defensive timing system using console logs (for testing/backup)
    /// </summary>
    private IEnumerator HandleDefensiveTimingFallback(Entity target, System.Action<bool> onComplete)
    {
        // Notify the player that the defensive window is starting
        Debug.Log($"Defensive window started for {target.name}! Press the button sequence!");

        bool sequenceCompleted = false;
        int currentButtonIndex = 0;
        float timeRemaining = buttonSequenceTimeLimit;

        InputSystem_Actions inputActions = new InputSystem_Actions();
        inputActions.Enable();

        while (timeRemaining > 0 && !sequenceCompleted)
        {
            timeRemaining -= Time.deltaTime;

            // Check if the current required button was pressed
            if (currentButtonIndex < requiredButtonSequence.Length)
            {
                string expectedButton = requiredButtonSequence[currentButtonIndex];
                
                if (WasButtonPressed(inputActions, expectedButton))
                {
                    currentButtonIndex++;
                    Debug.Log($"Correct button {expectedButton}! ({currentButtonIndex}/{requiredButtonSequence.Length})");
                    
                    // Check if the full sequence is completed
                    if (currentButtonIndex >= requiredButtonSequence.Length)
                    {
                        sequenceCompleted = true;
                        Debug.Log("Defensive sequence completed successfully!");
                    }
                }
                else if (AnyUnexpectedButtonPressed(inputActions, expectedButton))
                {
                    // Wrong button pressed, reset the sequence
                    currentButtonIndex = 0;
                    Debug.Log("Wrong button! Sequence reset.");
                }
            }

            yield return null;
        }

        inputActions.Disable();
        inputActions.Dispose();

        // Call the completion callback
        onComplete?.Invoke(sequenceCompleted);
        
        if (sequenceCompleted)
        {
            Debug.Log("Player successfully completed defensive sequence!");
        }
        else
        {
            Debug.Log("Player failed to complete defensive sequence in time.");
        }
    }

    /// <summary>
    /// Checks if a specific button was pressed this frame
    /// </summary>
    private bool WasButtonPressed(InputSystem_Actions inputActions, string buttonName)
    {
        switch (buttonName)
        {
            case "LeftGamepad":
                return inputActions.Player.LeftGamepad.WasPressedThisFrame();
            case "RightGamepad":
                return inputActions.Player.RightGamepad.WasPressedThisFrame();
            case "TopGamepad":
                return inputActions.Player.TopGamepad.WasPressedThisFrame();
            case "BottomGamepad":
                return inputActions.Player.BottomGamepad.WasPressedThisFrame();
            default:
                return false;
        }
    }

    /// <summary>
    /// Checks if any button other than the expected one was pressed
    /// </summary>
    private bool AnyUnexpectedButtonPressed(InputSystem_Actions inputActions, string expectedButton)
    {
        bool leftPressed = inputActions.Player.LeftGamepad.WasPressedThisFrame();
        bool rightPressed = inputActions.Player.RightGamepad.WasPressedThisFrame();
        bool topPressed = inputActions.Player.TopGamepad.WasPressedThisFrame();
        bool bottomPressed = inputActions.Player.BottomGamepad.WasPressedThisFrame();

        switch (expectedButton)
        {
            case "LeftGamepad":
                return rightPressed || topPressed || bottomPressed;
            case "RightGamepad":
                return leftPressed || topPressed || bottomPressed;
            case "TopGamepad":
                return leftPressed || rightPressed || bottomPressed;
            case "BottomGamepad":
                return leftPressed || rightPressed || topPressed;
            default:
                return leftPressed || rightPressed || topPressed || bottomPressed;
        }
    }
}
