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
                ApplyDamage(amount, user, target);
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
                    EventManager.TriggerCharacterTurnStarted(character);
                }
            });
            while (!returnDone) yield return new WaitForSeconds(1.6f);
        }
        else
        {

            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, amount, (wasDefended) =>
            {
                defensiveActionSucceeded = wasDefended;
            });
            yield return new WaitForSeconds(1.6f);
            if (user is CharacterManager character)
            {
                EventManager.TriggerCharacterTurnStarted(character);
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

        // Play attack animation and wait for completion
        bool animDone = false;
        bool hitTriggered = false;

        if (animationHandler == null)
        {
            // No animation, apply damage immediately
            ApplyDamage(damageAmount, user, target, false);
            hitTriggered = true;
            animDone = true;
            onDefensiveResult?.Invoke(false);
        }
        else
        {
            animationHandler.UseAbility(this, time =>
            {
                if (!hitTriggered)
                {
                    // Apply damage with defensive result
                    ApplyDamage(damageAmount, user, target, defensiveActionSucceeded);
                    hitTriggered = true;
                }
            }, () => { animDone = true; });

            // Start defensive timing window if the target is a player character
            if (target is CharacterManager && defensiveWindowDuration > 0)
            {
                // Find a MonoBehaviour to run the coroutine (using the target's controller)
                var targetController = target.GetComponent<OverworldEntityController>();
                if (targetController != null)
                {
                    targetController.StartCoroutine(HandleDefensiveTimingWindow(target, defensiveWindowStart, (success) =>
                    {
                        defensiveActionSucceeded = success;
                        onDefensiveResult?.Invoke(success);
                    }));
                }
                else
                {
                    onDefensiveResult?.Invoke(false);
                }
            }
            else
            {
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

                ProjectileManager.CreateProjectile(
                    projectileSpawnPoint.position,
                    direction,
                    Vector3.one * 0.4f,
                    projectileData,
                    projectileData.projectilePath,
                    projectileData.projectileSpeed,
                    damageAmount,
                    -1,
                    null,
                    projectileData.particleObject,
                    "",
                    target.transform
                );
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
            if (!hitTriggered) 
            {
                ApplyDamage(damageAmount, user, target, defensiveActionSucceeded);
                onDefensiveResult?.Invoke(defensiveActionSucceeded);
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
            EventManager.TriggerAttackBlocked();
            Debug.Log($"Defensive action succeeded! Damage reduced from {amount} to {finalDamage}");
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

        // Try to find the UI system
        var defensiveUI = Object.FindFirstObjectByType(System.Type.GetType("DefensiveTimingUI"));
        
        if (defensiveUI != null)
        {
            bool sequenceCompleted = false;
            
            // Use reflection to call ShowDefensivePrompt method
            var showMethod = defensiveUI.GetType().GetMethod("ShowDefensivePrompt");
            if (showMethod != null)
            {
                System.Action<bool> callback = (success) => sequenceCompleted = success;
                showMethod.Invoke(defensiveUI, new object[] { requiredButtonSequence, buttonSequenceTimeLimit, callback });
                
                // Wait for the UI sequence to complete
                float maxWaitTime = buttonSequenceTimeLimit + 1f; // Add buffer time
                float waitTimer = 0f;
                
                while (waitTimer < maxWaitTime)
                {
                    waitTimer += Time.deltaTime;
                    yield return null;
                }
                
                onComplete?.Invoke(sequenceCompleted);
                yield break;
            }
        }
        
        // Fallback to the original console-based system
        yield return HandleDefensiveTimingFallback(target, onComplete);
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
