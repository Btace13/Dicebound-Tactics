using UnityEngine;
using DG.Tweening;
using TacticsToolkit;
using System.Collections;
using System.Linq;
using Unity.Cinemachine;

/// <summary>
/// Manages the slow motion effect for the final killing blow in combat
/// </summary>
public class FinalBlowEffectManager : MonoBehaviour
{
    [Header("Slow Motion Settings")]
    [SerializeField] private float slowMotionTimeScale = 0.3f;
    [SerializeField] private float slowMotionDuration = 2f;
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private AnimationCurve slowMotionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem finalBlowParticles;
    [SerializeField] private GameObject finalBlowVFXPrefab;
    [SerializeField] private Color screenTintColor = new Color(1f, 0.8f, 0.8f, 0.3f);
    [SerializeField] private CanvasGroup screenTintOverlay;
    
    [Header("Audio")]
    [SerializeField] private AudioClip finalBlowSFX;
    [SerializeField] private AudioSource audioSource;
    
    public static FinalBlowEffectManager Instance { get; private set; }
    
    private bool isInSlowMotion = false;
    
    /// <summary>
    /// Static method to check if the manager is properly set up
    /// </summary>
    public static bool IsManagerAvailable()
    {
        bool available = Instance != null;
        Debug.Log($"[FinalBlowEffect] Manager availability check: {available}");
        return available;
    }
    private Entity currentAttacker;
    private Entity currentTarget;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[FinalBlowEffect] Destroying duplicate FinalBlowEffectManager instance");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Debug.Log("[FinalBlowEffect] FinalBlowEffectManager instance created and ready");
    }
    
    private void OnEnable()
    {
        // Subscribe to combat events
        EventManager.OnAbilityStarted += OnAbilityStarted;
        EventManager.OnFinalBlowTriggered += OnFinalBlowTriggered;
        EventManager.OnCombatEncounterEnded += OnCombatEncounterEnded;
        
        Debug.Log("[FinalBlowEffect] Event subscriptions registered");
    }
    
    private void OnDisable()
    {
        // Unsubscribe from combat events
        EventManager.OnAbilityStarted -= OnAbilityStarted;
        EventManager.OnFinalBlowTriggered -= OnFinalBlowTriggered;
        EventManager.OnCombatEncounterEnded -= OnCombatEncounterEnded;
        
        // Reset time scale when disabled
        if (isInSlowMotion)
        {
            ResetTimeScale();
        }
    }
    
    /// <summary>
    /// Called when an ability is started to check if this will be a final blow
    /// </summary>
    private void OnAbilityStarted(Entity attacker, Entity target)
    {
        Debug.Log($"[FinalBlowEffect] OnAbilityStarted called: {attacker?.name} -> {target?.name}");
        
        if (ShouldTriggerFinalBlowEffect(attacker, target))
        {
            Debug.Log("[FinalBlowEffect] ShouldTriggerFinalBlowEffect returned TRUE - starting effect!");
            StartCoroutine(ExecuteFinalBlowEffect(attacker, target));
        }
        else
        {
            Debug.Log("[FinalBlowEffect] ShouldTriggerFinalBlowEffect returned FALSE - no effect");
        }
    }
    
    /// <summary>
    /// Called when a final blow is explicitly triggered
    /// </summary>
    private void OnFinalBlowTriggered(Entity attacker, Entity target)
    {
        Debug.Log($"[FinalBlowEffect] OnFinalBlowTriggered called: {attacker?.name} -> {target?.name}");
        
        // Start the effect immediately without waiting
        if (!isInSlowMotion)
        {
            Debug.Log("[FinalBlowEffect] Starting final blow effect from trigger!");
            StartCoroutine(ExecuteFinalBlowEffect(attacker, target));
        }
        else
        {
            Debug.Log("[FinalBlowEffect] Already in slow motion, skipping trigger");
        }
    }
    
    /// <summary>
    /// Called when a combat encounter ends - ensures proper cleanup
    /// </summary>
    private void OnCombatEncounterEnded(CombatEncounter encounter, bool playerWon)
    {
        Debug.Log("[FinalBlowEffect] Combat encounter ended, ensuring proper cleanup");
        
        // Force reset any active effects
        if (isInSlowMotion)
        {
            Debug.Log("[FinalBlowEffect] Encounter ended while in slow motion, force resetting");
            StopAllCoroutines();
            ResetTimeScale();
        }
        
        // Ensure camera returns to overworld (with a slight delay)
        StartCoroutine(EnsureOverworldCameraCoroutine());
    }
    
    /// <summary>
    /// Ensures camera returns to overworld camera after encounter ends
    /// </summary>
    private IEnumerator EnsureOverworldCameraCoroutine()
    {
        // Wait a moment for normal encounter end logic to run
        yield return new WaitForSecondsRealtime(1f);
        
        if (CameraManager.Instance != null)
        {
            var activeCamera = CameraManager.Instance.ActiveCamera;
            if (activeCamera == null || activeCamera.name != "OverworldCamera")
            {
                Debug.Log($"[FinalBlowEffect] Ensuring overworld camera (current: {activeCamera?.name ?? "null"})");
                CameraManager.Instance.TrySetActiveCamera("OverworldCamera");
            }
        }
    }
    
    /// <summary>
    /// Checks if an attack should trigger the final blow effect
    /// </summary>
    /// <param name="attacker">The entity performing the attack</param>
    /// <param name="target">The entity being attacked</param>
    /// <returns>True if this is a killing blow to the last enemy/player</returns>
    public bool ShouldTriggerFinalBlowEffect(Entity attacker, Entity target)
    {
        Debug.Log($"[FinalBlowEffect] Checking if should trigger: {attacker?.name} -> {target?.name}");
        
        if (attacker == null || target == null || !target.isAlive)
        {
            Debug.Log($"[FinalBlowEffect] Basic check failed: attacker={attacker?.name}, target={target?.name}, target.isAlive={target?.isAlive}");
            return false;
        }
            
        // Don't trigger if already in slow motion
        if (isInSlowMotion)
        {
            Debug.Log("[FinalBlowEffect] Already in slow motion, skipping");
            return false;
        }
            
        // Don't trigger if battle is not active
        if (TurnManager.Instance == null || !TurnManager.Instance.BattlePlaying)
        {
            Debug.Log($"[FinalBlowEffect] Battle not active: TurnManager={TurnManager.Instance != null}, BattlePlaying={TurnManager.Instance?.BattlePlaying}");
            return false;
        }
            
        // Check if this is the last enemy or last player FIRST (more important check)
        bool isLastEnemy = IsLastAliveInTeam(target, TurnManager.Instance.enemyUnits.Cast<Entity>().ToList());
        bool isLastPlayer = IsLastAliveInTeam(target, TurnManager.Instance.playerUnits.Cast<Entity>().ToList());
        
        Debug.Log($"[FinalBlowEffect] Team check: isLastEnemy={isLastEnemy}, isLastPlayer={isLastPlayer}");
        
        if (!isLastEnemy && !isLastPlayer)
        {
            Debug.Log("[FinalBlowEffect] Target is not the last alive member of their team");
            return false;
        }
        
        // For the last member of a team, use more lenient damage checking
        // Calculate potential damage (this is an estimate)
        int potentialDamage = CalculatePotentialDamage(attacker, target);
        Debug.Log($"[FinalBlowEffect] Potential damage: {potentialDamage}, Target health: {target.CurrentHealth}");
        
        // More lenient check: trigger if target has low health OR if potential damage is high enough
        // This accounts for abilities that might do more damage than we can accurately predict
        bool mightKillTarget = target.CurrentHealth <= potentialDamage || 
                              target.CurrentHealth <= (target.GetStat(TacticsToolkit.Stats.Health).statValue * 0.3f); // Less than 30% health
        
        if (!mightKillTarget)
        {
            Debug.Log($"[FinalBlowEffect] Attack unlikely to kill target. Health: {target.CurrentHealth}, Max Health: {target.GetStat(TacticsToolkit.Stats.Health).statValue}");
            return false;
        }
        
        Debug.Log($"[FinalBlowEffect] Enemy count: {TurnManager.Instance.enemyUnits.Count(e => e != null && e.isAlive)}");
        Debug.Log($"[FinalBlowEffect] Player count: {TurnManager.Instance.playerUnits.Count(e => e != null && e.isAlive)}");
        
        bool shouldTrigger = isLastEnemy || isLastPlayer;
        Debug.Log($"[FinalBlowEffect] Final decision: {shouldTrigger}");
        
        return shouldTrigger;
    }
    
    /// <summary>
    /// Calculates the potential damage an attacker might deal to a target
    /// Tries to use actual ability damage if available, otherwise estimates conservatively
    /// </summary>
    private int CalculatePotentialDamage(Entity attacker, Entity target)
    {
        try
        {
            int baseDamage;
            
            // Try to get the actual ability damage if an ability is selected
            if (CombatManager.Instance != null && CombatManager.Instance.SelectedAbility != null)
            {
                var selectedAbility = CombatManager.Instance.SelectedAbility;
                if (selectedAbility is DamageAbilitySO damageAbility)
                {
                    baseDamage = damageAbility.damageAmount;
                    Debug.Log($"[FinalBlowEffect] Using actual ability damage: {baseDamage} from {damageAbility.name}");
                }
                else
                {
                    // Non-damage ability, use basic attack damage
                    baseDamage = attacker.characterClass.Strength.baseStatValue;
                    Debug.Log($"[FinalBlowEffect] Non-damage ability, using basic damage: {baseDamage}");
                }
            }
            else
            {
                // No ability selected or no CombatManager, assume basic attack
                baseDamage = attacker.characterClass.Strength.baseStatValue;
                Debug.Log($"[FinalBlowEffect] No ability selected, using basic attack damage: {baseDamage}");
            }
            
            // Apply attacker's damage modifiers
            int modifiedDamage = attacker.CalculateDamageWithModifiers(baseDamage);
            Debug.Log($"[FinalBlowEffect] After attacker modifiers: {modifiedDamage}");
            
            // Apply target's damage reduction
            int finalDamage = target.CalculateDamageTakenWithModifiers(modifiedDamage);
            Debug.Log($"[FinalBlowEffect] Final damage after target reduction: {finalDamage}");
            
            return finalDamage;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FinalBlowEffect] Error calculating potential damage: {e.Message}");
            // Fallback to conservative estimate
            int fallback = (attacker.characterClass?.Strength?.baseStatValue ?? 10) * 2;
            Debug.Log($"[FinalBlowEffect] Using fallback damage estimate: {fallback}");
            return fallback;
        }
    }
    
    /// <summary>
    /// Checks if the given entity is the last alive member of their team
    /// </summary>
    private bool IsLastAliveInTeam(Entity entity, System.Collections.Generic.List<Entity> teamMembers)
    {
        if (teamMembers == null || teamMembers.Count == 0)
        {
            Debug.Log($"[FinalBlowEffect] Team check failed: teamMembers is null or empty");
            return false;
        }
            
        var aliveMembers = teamMembers.Where(e => e != null && e.isAlive).ToList();
        Debug.Log($"[FinalBlowEffect] Team analysis for {entity.name}: total members={teamMembers.Count}, alive members={aliveMembers.Count}");
        
        foreach (var member in teamMembers)
        {
            Debug.Log($"[FinalBlowEffect] Team member: {member?.name} - isAlive: {member?.isAlive}");
        }
        
        bool isLastAlive = aliveMembers.Count == 1 && aliveMembers.Contains(entity);
        Debug.Log($"[FinalBlowEffect] Is {entity.name} the last alive? {isLastAlive}");
        
        return isLastAlive;
    }
    
    /// <summary>
    /// Executes the final blow effect sequence
    /// </summary>
    private IEnumerator ExecuteFinalBlowEffect(Entity attacker, Entity target)
    {
        currentAttacker = attacker;
        currentTarget = target;
        isInSlowMotion = true;
        
        Debug.Log($"[FinalBlowEffect] Triggering final blow effect: {attacker.name} -> {target.name}");
        
        // Start slow motion effect
        StartSlowMotion();
        Debug.Log($"[FinalBlowEffect] Started slow motion - timeScale: {Time.timeScale}");
        
        // Apply screen tint for dramatic effect
        ApplyScreenTint(true);
        
        // Do camera shake AFTER slow motion starts
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ShakeActiveCamera();
        }
        
        // Wait for slow motion duration (using realtime since timeScale is modified)
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Debug.Log($"[FinalBlowEffect] Slow motion duration completed, restoring normal time");
        
        // Restore normal time
        EndSlowMotion();
        
        // Wait a bit more for time scale to normalize
        yield return new WaitForSecondsRealtime(0.3f);
        
        // Clean up
        isInSlowMotion = false;
        currentAttacker = null;
        currentTarget = null;
        
        // Remove screen tint
        ApplyScreenTint(false);
        
        // Failsafe: Ensure camera returns to overworld if battle is ending
        // This runs after a delay to let normal encounter end logic run first
        StartCoroutine(CameraFailsafeCoroutine());
        
        Debug.Log("[FinalBlowEffect] Final blow effect completed");
    }
    
    /// <summary>
    /// Starts slow motion effect immediately
    /// </summary>
    private void StartSlowMotion()
    {
        Debug.Log($"[FinalBlowEffect] StartSlowMotion - current timeScale: {Time.timeScale}, target: {slowMotionTimeScale}");
        
        // Kill any existing time scale tweens
        DOTween.Kill("TimeScale");
        
        // Slow down time smoothly
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, slowMotionTimeScale, 0.3f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .SetId("TimeScale")
            .OnComplete(() => Debug.Log($"[FinalBlowEffect] Slow motion started - final timeScale: {Time.timeScale}"));
    }
    
    /// <summary>
    /// Ends slow motion effect and returns to normal time
    /// </summary>
    private void EndSlowMotion()
    {
        Debug.Log($"[FinalBlowEffect] EndSlowMotion - current timeScale: {Time.timeScale}, target: {normalTimeScale}");
        
        // Kill any existing time scale tweens
        DOTween.Kill("TimeScale");
        
        // Speed back up to normal
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, normalTimeScale, 0.3f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .SetId("TimeScale")
            .OnComplete(() => Debug.Log($"[FinalBlowEffect] Normal time restored - final timeScale: {Time.timeScale}"));
    }
    
    /// <summary>
    /// Starts visual effects for the final blow
    /// </summary>
    private void StartVisualEffects()
    {
        StartVisualEffectsWithoutCameraShake();
        
        // Camera shake effect
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ShakeActiveCamera();
        }
    }
    
    /// <summary>
    /// Starts visual effects for the final blow without camera shake
    /// </summary>
    private void StartVisualEffectsWithoutCameraShake()
    {
        // Play particle effects at target location
        if (finalBlowParticles != null && currentTarget != null)
        {
            finalBlowParticles.transform.position = currentTarget.transform.position + Vector3.up * 2f;
            finalBlowParticles.Play();
        }
        
        // Instantiate VFX prefab if available
        if (finalBlowVFXPrefab != null && currentTarget != null)
        {
            GameObject vfxInstance = Instantiate(finalBlowVFXPrefab, currentTarget.transform.position, Quaternion.identity);
            Destroy(vfxInstance, 5f); // Clean up after 5 seconds
        }
    }
    
    /// <summary>
    /// Plays the final blow sound effect
    /// </summary>
    private void PlayFinalBlowSFX()
    {
        if (finalBlowSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(finalBlowSFX);
        }
    }
    
    /// <summary>
    /// Applies or removes screen tint effect
    /// </summary>
    private void ApplyScreenTint(bool apply)
    {
        if (screenTintOverlay != null)
        {
            if (apply)
            {
                var image = screenTintOverlay.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.color = screenTintColor;
                }
                
                screenTintOverlay.alpha = 0f;
                screenTintOverlay.DOFade(screenTintColor.a, 0.5f).SetUpdate(true);
            }
            else
            {
                screenTintOverlay.DOFade(0f, 1f).SetUpdate(true);
            }
        }
    }
    
    /// <summary>
    /// Resets time scale to normal (emergency cleanup)
    /// </summary>
    private void ResetTimeScale()
    {
        // Kill all time scale related tweens
        DOTween.Kill("TimeScale");
        DOTween.Kill(this);
        
        // Force time scale back to normal
        Time.timeScale = normalTimeScale;
        isInSlowMotion = false;
        
        // Clean up current effect variables
        currentAttacker = null;
        currentTarget = null;
        
        // Remove screen tint if it's active
        if (screenTintOverlay != null)
        {
            screenTintOverlay.DOKill();
            screenTintOverlay.alpha = 0f;
        }
        
        // Try to reset camera shake if it's stuck
        ResetCameraShake();
        
        Debug.Log("[FinalBlowEffect] Time scale and effects reset to normal");
    }
    
    /// <summary>
    /// Attempts to reset camera shake if it's stuck
    /// </summary>
    private void ResetCameraShake()
    {
        try
        {
            if (CameraManager.Instance != null && CameraManager.Instance.ActiveCamera != null)
            {
                var cinemachineCam = CameraManager.Instance.ActiveCamera.CinemachineCam;
                if (cinemachineCam != null)
                {
                    var noise = cinemachineCam.GetComponent<CinemachineBasicMultiChannelPerlin>();
                    if (noise != null)
                    {
                        noise.AmplitudeGain = 0f;
                        noise.FrequencyGain = 0f;
                        noise.enabled = false;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[FinalBlowEffect] Could not reset camera shake: {e.Message}");
        }
    }
    
    /// <summary>
    /// Failsafe coroutine to ensure camera returns to overworld after final blow effect
    /// </summary>
    private IEnumerator CameraFailsafeCoroutine()
    {
        // Wait a bit to let normal encounter end logic run first
        yield return new WaitForSecondsRealtime(2f);
        
        // Check if we're still in a battle that should have ended
        bool battleShouldHaveEnded = false;
        
        if (TurnManager.Instance != null)
        {
            // Check if all enemies are dead OR all players are dead
            bool allEnemiesDead = TurnManager.Instance.enemyUnits.All(e => e == null || !e.isAlive);
            bool allPlayersDead = TurnManager.Instance.playerUnits.All(p => p == null || !p.isAlive);
            
            battleShouldHaveEnded = allEnemiesDead || allPlayersDead;
            
            Debug.Log($"[FinalBlowEffect] Camera failsafe check: allEnemiesDead={allEnemiesDead}, allPlayersDead={allPlayersDead}, battleShouldHaveEnded={battleShouldHaveEnded}");
        }
        
        // If battle should have ended but camera is not on overworld, switch to overworld camera
        if (battleShouldHaveEnded && CameraManager.Instance != null)
        {
            var activeCamera = CameraManager.Instance.ActiveCamera;
            if (activeCamera != null && activeCamera.name != "OverworldCamera")
            {
                Debug.Log($"[FinalBlowEffect] Camera failsafe triggered! Switching from {activeCamera.name} to OverworldCamera");
                CameraManager.Instance.TrySetActiveCamera("OverworldCamera");
                
                // Also ensure game state is set to overworld
                if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentGameState != GameState.Overworld)
                {
                    Debug.Log("[FinalBlowEffect] Also ensuring game state is set to Overworld");
                    // Note: We can't directly change the game state here as we don't have access to the private method
                    // But the normal encounter end logic should handle this
                }
            }
            else if (activeCamera != null)
            {
                Debug.Log($"[FinalBlowEffect] Camera failsafe check passed - already on {activeCamera.name}");
            }
            else
            {
                Debug.LogWarning("[FinalBlowEffect] Camera failsafe check - no active camera found!");
                // Try to set overworld camera anyway
                CameraManager.Instance.TrySetActiveCamera("OverworldCamera");
            }
        }
        else
        {
            Debug.Log("[FinalBlowEffect] Camera failsafe check - battle not ended or camera manager not available");
        }
    }
    
    /// <summary>
    /// Public method to manually trigger final blow effect (for testing)
    /// </summary>
    [System.Obsolete("This method is for testing purposes only")]
    public void TriggerFinalBlowEffectManually(Entity attacker, Entity target)
    {
        if (attacker != null && target != null)
        {
            StartCoroutine(ExecuteFinalBlowEffect(attacker, target));
        }
    }
    
    /// <summary>
    /// Public method to force reset all effects (for emergency cleanup)
    /// </summary>
    public void ForceResetAllEffects()
    {
        // Stop all coroutines on this object
        StopAllCoroutines();
        
        // Reset everything
        ResetTimeScale();
        
        // Force camera back to overworld
        if (CameraManager.Instance != null)
        {
            Debug.Log("[FinalBlowEffect] Force resetting camera to overworld");
            CameraManager.Instance.TrySetActiveCamera("OverworldCamera");
        }
        
        Debug.Log("[FinalBlowEffect] Force reset all effects completed");
    }
    
#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    /// <summary>
    /// Debug method to test the effect with current entities in scene
    /// </summary>
    [Sirenix.OdinInspector.Button("Test Final Blow Effect")]
    private void TestFinalBlowEffect()
    {
        var allEntities = GameObject.FindObjectsByType<Entity>(FindObjectsSortMode.None);
        if (allEntities.Length >= 2)
        {
            StartCoroutine(ExecuteFinalBlowEffect(allEntities[0], allEntities[1]));
        }
        else
        {
            Debug.LogWarning("Need at least 2 entities in scene to test final blow effect");
        }
    }
    
    /// <summary>
    /// Debug method to check current system state
    /// </summary>
    [Sirenix.OdinInspector.Button("Check System State")]
    private void CheckSystemState()
    {
        Debug.Log($"[FinalBlowEffect] System State:" +
                 $"\n- In Slow Motion: {isInSlowMotion}" +
                 $"\n- Current Time Scale: {Time.timeScale}" +
                 $"\n- Current Attacker: {(currentAttacker != null ? currentAttacker.name : "None")}" +
                 $"\n- Current Target: {(currentTarget != null ? currentTarget.name : "None")}" +
                 $"\n- Battle Playing: {(TurnManager.Instance != null ? TurnManager.Instance.BattlePlaying : "No TurnManager")}");
    }
    
    /// <summary>
    /// Debug method to test detection logic with current battle state
    /// </summary>
    [Sirenix.OdinInspector.Button("Test Detection Logic")]
    private void TestDetectionLogic()
    {
        if (TurnManager.Instance == null)
        {
            Debug.LogError("[FinalBlowEffect] No TurnManager found!");
            return;
        }
        
        var currentUnit = TurnManager.Instance.GetCurrentUnit();
        if (currentUnit == null)
        {
            Debug.LogError("[FinalBlowEffect] No current unit!");
            return;
        }
        
        // Test against first enemy
        if (TurnManager.Instance.enemyUnits.Count > 0)
        {
            var enemy = TurnManager.Instance.enemyUnits[0];
            Debug.Log($"[FinalBlowEffect] Testing {currentUnit.name} vs {enemy.name}:");
            bool result = ShouldTriggerFinalBlowEffect(currentUnit, enemy);
            Debug.Log($"[FinalBlowEffect] Result: {result}");
        }
        
        // Test against first player
        if (TurnManager.Instance.playerUnits.Count > 0)
        {
            var player = TurnManager.Instance.playerUnits[0];
            Debug.Log($"[FinalBlowEffect] Testing {currentUnit.name} vs {player.name}:");
            bool result = ShouldTriggerFinalBlowEffect(currentUnit, player);
            Debug.Log($"[FinalBlowEffect] Result: {result}");
        }
    }
    
    /// <summary>
    /// Debug method to force trigger the effect for testing
    /// </summary>
    [Sirenix.OdinInspector.Button("Force Trigger Effect")]
    private void ForceTestEffect()
    {
        Debug.Log("[FinalBlowEffect] Force triggering effect for testing!");
        
        // Create dummy entities for testing
        var allEntities = GameObject.FindObjectsByType<Entity>(FindObjectsSortMode.None);
        if (allEntities.Length >= 2)
        {
            Debug.Log($"[FinalBlowEffect] Using entities: {allEntities[0].name} -> {allEntities[1].name}");
            EventManager.TriggerFinalBlowTriggered(allEntities[0], allEntities[1]);
        }
        else
        {
            Debug.LogWarning("[FinalBlowEffect] Not enough entities in scene for test");
        }
    }
#endif
    
    private void OnApplicationPause(bool pauseStatus)
    {
        // Reset time scale when application is paused/resumed
        if (!pauseStatus && isInSlowMotion)
        {
            ResetTimeScale();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        // Reset time scale when application loses/gains focus
        if (hasFocus && isInSlowMotion)
        {
            ResetTimeScale();
        }
    }
}