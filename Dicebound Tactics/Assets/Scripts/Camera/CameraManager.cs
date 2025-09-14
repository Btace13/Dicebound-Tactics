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
    // Redundant subscriptions for reliability: some flows may miss OnNewActiveEntity
    EventManager.OnCharacterTurnStarted += HandleCharacterTurnStarted;
    EventManager.OnEnemyTurnStarted += HandleEnemyTurnStarted;
    }

    private void OnDisable()
    {
        EventManager.OnNewActiveEntity -= SetActiveCombatCharacter;
        EventManager.OnTargetChanged -= SetCombatTarget;
        EventManager.OnCombatEncounterStarted -= HandleCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded -= HandleCombatEncounterEnded;
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
        if (Cameras.TryGetValue(cameraName, out var cameraController))
        {
            SetActiveCamera(cameraController);
        }
        else
        {
            Debug.LogWarning($"[CameraManager] Camera with name {cameraName} not found.");
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
        Debug.Log($"[CameraManager] Found {Cameras.Values.OfType<CombatCameraController>().Count()} combat cameras");
        
        foreach (CombatCameraController combatCamera in Cameras.Values.OfType<CombatCameraController>())
        {
            Debug.Log($"[CameraManager] Processing combat camera: {combatCamera?.name ?? "NULL"}");
            
            if (combatCamera == null)
            {
                Debug.LogWarning("[CameraManager] Active camera is not a CombatCameraController.");
                continue;
            }
            if (combatCamera.TargetGroup == null)
            {
                Debug.LogError("[CameraManager] Target group is not initialized in CombatCameraController.");
                continue;
            }
            if (combatCamera.cameraTarget == CameraTarget.Target)
            {
                Debug.LogWarning("[CameraManager] Camera target is set to Target. Cannot set active character.");
                continue;
            }
            
            Debug.Log($"[CameraManager] Setting up camera {combatCamera.name} with {combatCamera.TargetGroup.Targets.Count} existing targets");
            
            if (combatCamera.TargetGroup.Targets.Count == 0)
            {
                Debug.Log($"[CameraManager] Adding new target to {combatCamera.name}");
                combatCamera.AddTarget(character);
            }
            else
            {
                Debug.Log($"[CameraManager] Updating existing target in {combatCamera.name}");
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

    // Additional handlers to guarantee camera updates on every turn start
    private void HandleCharacterTurnStarted(CharacterManager character)
    {
        if (character == null) return;
        Debug.Log($"[CameraManager] Character turn started: {character.name} -> updating camera target");
        SetActiveCombatCharacter(character.transform);
    }

    private void HandleEnemyTurnStarted(EnemyManager enemy)
    {
        if (enemy == null) return;
        Debug.Log($"[CameraManager] Enemy turn started: {enemy.name} -> updating camera target");
        SetActiveCombatCharacter(enemy.transform);
    }
}
