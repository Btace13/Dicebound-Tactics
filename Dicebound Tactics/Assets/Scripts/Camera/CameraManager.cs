using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;
using Unity.VisualScripting;
using System.Linq;
using TacticsToolkit;
using andywiecko.BurstTriangulator;

// Camera state class to store camera position data
public class CameraState
{
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public float Distance { get; private set; }
    public UDictionary<string, ICameraController> Cameras = new UDictionary<string, ICameraController>();
    private Dictionary<string, CameraState> _cameraStates = new Dictionary<string, CameraState>();

    const int DefaultCameraPriority = 10;
    const int HighCameraPriority = 20;

    public CameraState(Vector3 position, Quaternion rotation, float distance)
    {
        Position = position;
        Rotation = rotation;
        Distance = distance;
    }
}

public enum CameraFramingOptions
{
    CENTER = 0,
    UPPER_LEFT = 1,
    UPPER_RIGHT = 2,
    LOWER_LEFT = 3,
    LOWER_RIGHT = 4
}

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    public BaseCameraController ActiveCamera { get; private set; }

    public UDictionary<string, BaseCameraController> Cameras = new UDictionary<string, BaseCameraController>();

    private Dictionary<string, CameraState> _cameraStates = new Dictionary<string, CameraState>();

    const int DefaultCameraPriority = 10;
    const int HighCameraPriority = 20;

    public Transform activeCharacter { get; private set; }
    public Transform activeTarget { get; private set; }

    [Header("General Settings")]
    [SerializeField] private CameraShakeSettings defaultCameraShakeSettings;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // Event Listeners
        EventManager.OnNewActiveEntity += SetActiveCombatCharacter;
        EventManager.OnTargetChanged += SetCombatTarget;
        EventManager.OnCombatEncounterStarted += HandleCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded += HandleCombatEncounterEnded;
        EventManager.OnAbilityStarted += HandleAbilityStarted;
        EventManager.OnAbilityEnded += HandleAbilityEnded;
        EventManager.OnCharacterTurnStarted += HandleCharacterTurnStarted;
        EventManager.OnEnemyTurnStarted += HandleEnemyTurnStarted;
    }

    private void OnDisable()
    {
        EventManager.OnNewActiveEntity -= SetActiveCombatCharacter;
        EventManager.OnTargetChanged -= SetCombatTarget;
        EventManager.OnCombatEncounterStarted -= HandleCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded -= HandleCombatEncounterEnded;
        EventManager.OnAbilityStarted -= HandleAbilityStarted;
        EventManager.OnAbilityEnded -= HandleAbilityEnded;
        EventManager.OnCharacterTurnStarted -= HandleCharacterTurnStarted;
        EventManager.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
    }

    void Start()
    {
        TrySetActiveCamera("OverworldCamera");
    }

    public void RegisterCamera(string cameraName, BaseCameraController cameraController)
    {
        if (!Cameras.ContainsKey(cameraName))
        {
            Cameras.Add(cameraName, cameraController);
            if (ActiveCamera == null)
            {
                SetActiveCamera(cameraController);
            }
        }
        else
        {
            Debug.LogWarning($"[CameraManager] Camera with name {cameraName} is already registered.");
        }
    }

    public void UnregisterCamera(string cameraName)
    {
        if (Cameras.ContainsKey(cameraName))
        {
            Cameras.Remove(cameraName);
            if (ActiveCamera != null && ActiveCamera.CameraName == cameraName)
            {
                ActiveCamera = null; // Clear active camera if it was unregistered
            }
        }
        else
        {
            // Debug.LogWarning($"Camera with name {cameraName} is not registered.");
        }
    }

    public void TrySetActiveCamera(string cameraName)
    {
        Debug.Log($"[CameraManager] TrySetActiveCamera called with: {cameraName}");
        Debug.Log($"[CameraManager] Current active camera: {ActiveCamera?.CameraName ?? "NULL"}");
        
        if (Cameras.TryGetValue(cameraName, out var cameraController))
        {
            Debug.Log($"[CameraManager] Found camera controller for: {cameraName}");
            SetActiveCamera(cameraController);
            Debug.Log($"[CameraManager] Successfully switched to camera: {cameraName}");
        }
        else
        {
            Debug.LogWarning($"[CameraManager] Camera with name {cameraName} not found.");
            Debug.Log("[CameraManager] Available cameras:");
            foreach (var kvp in Cameras)
            {
                Debug.Log($"  - {kvp.Key}");
            }
        }
    }

    private void HandleCombatEncounterEnded(CombatEncounter encounter)
    {
        foreach (var cam in encounter.GetAllCameraControllers())
        {
            UnregisterCamera(cam.name);
        }

        TrySetActiveCamera("OverworldCamera");
    }

    private void HandleCombatEncounterStarted(CombatEncounter encounter)
    {
        foreach (var cam in encounter.GetAllCameraControllers())
        {
            RegisterCamera(cam.name, cam);
        }
    }

    public void SetActiveCamera(BaseCameraController cameraController)
    {
        // Implementation to set the active camera
        foreach (var cam in Cameras.Values)
        {
            if (cam == cameraController)
            {
                cam.SetCameraPriority(HighCameraPriority);
            }
            else
            {
                cam.SetCameraPriority(DefaultCameraPriority);
            }
        }

        ActiveCamera = cameraController;

        if (activeCharacter != null)
        {
            SetActiveCombatCharacter(activeCharacter);
        }

        if (activeTarget != null)
        {
            SetCombatTarget(activeTarget);
        }
    }

    public void ShakeActiveCamera(CameraShakeSettings cameraShakeSettings = null)
    {
        if (ActiveCamera == null)
        {
            // Debug.LogWarning("No active camera to shake.");
            return;
        }
        BaseCameraController cameraToShake = ActiveCamera;
        if (cameraShakeSettings == null)
        {
            cameraShakeSettings = defaultCameraShakeSettings;
            // print("Using default camera shake settings.");
        }
        if (cameraShakeSettings.Intensity <= 0 || cameraShakeSettings.Duration <= 0)
        {
            // Debug.LogWarning("Intensity and duration must be greater than zero for camera shake.");
            return;
        }
        CinemachineBasicMultiChannelPerlin noise = cameraToShake.CinemachineCam.GetOrAddComponent<CinemachineBasicMultiChannelPerlin>();
        noise.NoiseProfile = cameraShakeSettings.NoiseSettings;
        noise.enabled = true; // Ensure noise is enabled
        float initialAmplitude = noise.AmplitudeGain;
        float initialFrequency = noise.FrequencyGain;
        float x = 0f;
        DOTween.To(value => x = value, 0f, 1f, cameraShakeSettings.Duration)
        .OnUpdate(() =>
        {
            float curveValue = cameraShakeSettings.ShakeCurve.Evaluate(x);
            noise.AmplitudeGain = cameraShakeSettings.Intensity * curveValue;
            noise.FrequencyGain = cameraShakeSettings.Frequency * curveValue;
        })
        .OnComplete(() =>
        {
            noise.AmplitudeGain = initialAmplitude;
            noise.FrequencyGain = initialFrequency;
            noise.enabled = false; // Disable noise after shaking
            // print("Camera shake completed and noise disabled.");
        });
    }


    public void SaveCameraState(string cameraName, string stateId)
    {
        if (Cameras.TryGetValue(cameraName, out BaseCameraController camera))
        {
            // Store position, rotation, zoom level, etc.
            _cameraStates[stateId] = new CameraState(
                camera.CinemachineCam.transform.position,
                camera.CinemachineCam.transform.rotation,
                camera.CinemachineCam.GetComponent<CinemachinePositionComposer>()?
                                     .GetComponent<CinemachinePositionComposer>()?.CameraDistance ?? 10f
            );
        }
    }

    public void RestoreCameraState(string cameraName, string stateId, float blendTime = 0.5f)
    {
        if (Cameras.TryGetValue(cameraName, out BaseCameraController camera) &&
            _cameraStates.TryGetValue(stateId, out CameraState state))
        {
            // Restore the saved state with appropriate blending
            camera.CinemachineCam.transform.position = state.Position;
            camera.CinemachineCam.transform.rotation = state.Rotation;

            // Restore zoom level if applicable
            var composer = camera.CinemachineCam.GetComponent<CinemachinePositionComposer>();
            if (composer != null)
            {
                composer.CameraDistance = state.Distance;
            }
        }
    }

    public void FrameCamera(string cameraName, CameraFramingOptions framingOption)
    {
        if (Cameras.TryGetValue(cameraName, out BaseCameraController camera))
        {
            CinemachineRotationComposer composer = camera.CinemachineCam.GetComponent<CinemachineRotationComposer>();
            if (composer == null)
            {
                return;
            }

            switch (framingOption)
            {
                case CameraFramingOptions.UPPER_LEFT:
                    composer.Composition.ScreenPosition = new Vector2(-0.25f, -0.25f);
                    break;
                case CameraFramingOptions.UPPER_RIGHT:
                    composer.Composition.ScreenPosition = new Vector2(0.25f, -0.25f);
                    break;
                case CameraFramingOptions.LOWER_LEFT:
                    composer.Composition.ScreenPosition = new Vector2(-0.25f, 0.25f);
                    break;
                case CameraFramingOptions.LOWER_RIGHT:
                    composer.Composition.ScreenPosition = new Vector2(0.25f, 0.25f);
                    break;
                case CameraFramingOptions.CENTER:
                default:
                    composer.Composition.ScreenPosition = new Vector2(0f, 0f);
                    break;
            }
        }
    }

    [ContextMenu("Camera Framing/Set Center Framing")]
    public void SetCenterFraming()
    {
        FrameCamera(ActiveCamera.CameraName, CameraFramingOptions.CENTER);
    }

    [ContextMenu("Camera Framing/Set Upper Left Framing")]
    public void SetUpperLeftFraming()
    {
        FrameCamera(ActiveCamera.CameraName, CameraFramingOptions.UPPER_LEFT);
    }

    [ContextMenu("Camera Framing/Set Lower Left Framing")]
    public void SetLowerLeftFraming()
    {
        FrameCamera(ActiveCamera.CameraName, CameraFramingOptions.LOWER_LEFT);
    }

    [ContextMenu("Camera Framing/Set Upper Right Framing")]
    public void SetUpperRightFraming()
    {
        FrameCamera(ActiveCamera.CameraName, CameraFramingOptions.UPPER_RIGHT);
    }

    [ContextMenu("Camera Framing/Set Lower Right Framing")]
    public void SetLowerRightFraming()
    {
        FrameCamera(ActiveCamera.CameraName, CameraFramingOptions.LOWER_RIGHT);
    }

    public void SetCombatTarget(Transform target)
    {
        if (target == null)
        {
            // Debug.LogError("CameraManager: Target is null. Cannot set combat target.");
            return;
        }
        print("CameraManager: Setting combat target: " + target.name);
        foreach (CombatCameraController combatCameraController in Cameras.Values.OfType<CombatCameraController>())
        {
            if (combatCameraController == null)
            {
                // Debug.LogWarning("CameraManager: Active camera is not a CombatCameraController.");
                continue;
            }
            if (combatCameraController.TargetGroup == null)
            {
                // Debug.LogError("CameraManager: Target group is not initialized in CombatCameraController.");
                continue;
            }
            if (combatCameraController.cameraTarget == CameraTarget.ActivePlayer)
            {
                // Debug.LogWarning("CameraManager: Camera target is set to ActivePlayer. Cannot set target.");
                continue;
            }
            
            // Instead of clearing all targets, manage them properly
            // Remove the current target if it exists (should be at index 1 for Target cameras)
            if (combatCameraController.TargetGroup.Targets.Count > 1)
            {
                combatCameraController.TargetGroup.RemoveMember(combatCameraController.TargetGroup.Targets[1].Object);
            }
            
            // Add the new target
            combatCameraController.AddTarget(target);
        }
        activeTarget = target;
    }

    public void SetCombatTarget(Entity targetEntity)
    {
        SetCombatTarget(targetEntity.transform);
    }

    public void SetActiveCombatCharacter(Transform character)
    {
        Debug.Log($"[CameraManager] SetActiveCombatCharacter called with: {character?.name ?? "NULL"}");
        
        // Early exit if the character is already the active character
        if (activeCharacter == character)
        {
            Debug.Log($"[CameraManager] Character {character?.name ?? "NULL"} is already the active character. Skipping update.");
            return;
        }
        
        Debug.Log($"[CameraManager] Found {Cameras.Values.OfType<CombatCameraController>().Count()} combat cameras");
        
        foreach (CombatCameraController combatCamera in Cameras.Values.OfType<CombatCameraController>())
        {
            if (combatCamera == null)
            {
                Debug.LogWarning("[CameraManager] Found null combat camera controller.");
                continue;
            }
            if (combatCamera.TargetGroup == null)
            {
                Debug.LogError($"[CameraManager] Target group is not initialized in CombatCameraController: {combatCamera.name}");
                continue;
            }
            if (combatCamera.cameraTarget == CameraTarget.Target)
            {
                // Skip cameras that are specifically for targets, not active characters
                continue;
            }
            
            Debug.Log($"[CameraManager] Updating camera {combatCamera.name}");
            
            if (combatCamera.TargetGroup.Targets.Count == 0)
            {
                combatCamera.AddTarget(character);
            }
            else
            {
                combatCamera.UpdateTargetAtIndex(character, 0);
            }
            combatCamera.UpdateFollowTarget(character);
        }
        activeCharacter = character;
        Debug.Log($"[CameraManager] Active character set to: {activeCharacter?.name ?? "NULL"}");
    }

    public void SetActiveCombatCharacter(Entity entity)
    {
        SetActiveCombatCharacter(entity.transform);
    }

    private void HandleAbilityStarted(Entity user, Entity target)
    {
        Debug.Log($"[CameraManager] Ability started: {user.name} -> {target.name}");
        
        // Switch to a side camera for ability execution
        // First try Side1Camera, then Side2Camera, then AttackCamera as fallback
        string[] sideCameraNames = { "Side1Camera", "Side2Camera", "AttackCamera" };
        
        foreach (string cameraName in sideCameraNames)
        {
            if (Cameras.ContainsKey(cameraName))
            {
                Debug.Log($"[CameraManager] Switching to {cameraName} for ability execution");
                TrySetActiveCamera(cameraName);
                return;
            }
        }
        
        Debug.LogWarning("[CameraManager] No side cameras found for ability execution");
    }

    private void HandleAbilityEnded(Entity user, Entity target)
    {
        Debug.Log($"[CameraManager] Ability ended: {user.name} -> {target.name}");
        
        // Check if the user is a character and if they can use more abilities
        if (user is CharacterManager character)
        {
            if (character.CanUseMoreAbilitiesThisTurn())
            {
                // Turn is continuing - return camera focus to the character
                Debug.Log($"[CameraManager] Turn continuing, returning camera to {character.name}");
                SetActiveCombatCharacter(user.transform);
            }
            else
            {
                // Turn is ending - camera will be handled by the new turn started events
                Debug.Log($"[CameraManager] Turn ending for {character.name}, camera will switch on next turn");
            }
        }
        else
        {
            // For non-character entities (enemies), just return to the user
            SetActiveCombatCharacter(user.transform);
        }
        
        // Also trigger the panel camera switch if the UI is active
        // This will be handled by the delayed camera switch in CombatUIManager
    }

    private void HandleCharacterTurnStarted(CharacterManager character)
    {
        Debug.Log($"[CameraManager] Character turn started: {character.name}");
        
        // Delay camera switch to allow action panel to appear first
        StartCoroutine(DelayedCharacterCameraSwitch(character));
    }

    private void HandleEnemyTurnStarted(EnemyManager enemy)
    {
        Debug.Log($"[CameraManager] Enemy turn started: {enemy.name}");
        
        // Switch camera to the new enemy who is starting their turn
        SetActiveCombatCharacter(enemy.transform);
    }

    private IEnumerator DelayedCharacterCameraSwitch(CharacterManager character)
    {
        // Wait a short time to allow action panel to appear and settle
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"[CameraManager] Delayed camera switch to character: {character.name}");
        SetActiveCombatCharacter(character.transform);
    }
}
