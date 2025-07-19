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
        EventManager.OnCombatEncounterEnded += HandleCombatEncounterEnded;
    }

    private void OnDisable()
    {
        EventManager.OnNewActiveEntity -= SetActiveCombatCharacter;
        EventManager.OnTargetChanged -= SetCombatTarget;
        EventManager.OnCombatEncounterEnded -= HandleCombatEncounterEnded;
    }

    public void RegisterCamera(string cameraName, BaseCameraController cameraController)
    {
        print($"Attempting to register camera: {cameraName}");

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
            Debug.LogWarning($"Camera with name {cameraName} is already registered.");
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
            Debug.LogWarning($"Camera with name {cameraName} not found.");
        }
    }

    private void HandleCombatEncounterEnded(CombatEncounter encounter)
    {
        TrySetActiveCamera("OverworldCamera");
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
            Debug.LogWarning("No active camera to shake.");
            return;
        }

        BaseCameraController cameraToShake = ActiveCamera;

        if (cameraShakeSettings == null)
        {
            cameraShakeSettings = defaultCameraShakeSettings;
            print("Using default camera shake settings.");
        }

        if (cameraShakeSettings.Intensity <= 0 || cameraShakeSettings.Duration <= 0)
        {
            Debug.LogWarning("Intensity and duration must be greater than zero for camera shake.");
            return;
        }

        CinemachineBasicMultiChannelPerlin noise = cameraToShake.CinemachineCam.GetOrAddComponent<CinemachineBasicMultiChannelPerlin>();

        noise.NoiseProfile = cameraShakeSettings.NoiseSettings;
        noise.enabled = true; // Ensure noise is enabled

        // Setup the sequence to sample the animation curve over time
        float initialAmplitude = noise.AmplitudeGain;
        float initialFrequency = noise.FrequencyGain;

        float x = 0f;

        // Add tween that updates the noise parameters based on the animation curve
        DOTween.To(value => x = value, 0f, 1f, cameraShakeSettings.Duration)
        .OnUpdate(() =>
        {
            // Use the animation curve to control the intensity over time
            float curveValue = cameraShakeSettings.ShakeCurve.Evaluate(x);
            noise.AmplitudeGain = cameraShakeSettings.Intensity * curveValue;
            noise.FrequencyGain = cameraShakeSettings.Frequency * curveValue;
        })
        .OnComplete(() =>
        {
            // Reset the noise parameters to their initial values
            noise.AmplitudeGain = initialAmplitude;
            noise.FrequencyGain = initialFrequency;
            noise.enabled = false; // Disable noise after shaking
            print("Camera shake completed and noise disabled.");
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
            Debug.LogError("Target is null. Cannot set combat target.");
            return;
        }

        print("Setting combat target: " + target.name);

        foreach (CombatCameraController combatCameraController in Cameras.Values.OfType<CombatCameraController>())
        {
            if (combatCameraController == null)
            {
                Debug.LogWarning("Active camera is not a CombatCameraController.");
                continue;
            }

            if (combatCameraController.TargetGroup == null)
            {
                Debug.LogError("Target group is not initialized in CombatCameraController.");
                continue;
            }

            if (combatCameraController.cameraTarget == CameraTarget.ActivePlayer)
            {
                Debug.LogWarning("Camera target is set to ActivePlayer. Cannot set target.");
                continue;
            }

            combatCameraController.TargetGroup.Targets.Clear();
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
        foreach (CombatCameraController combatCamera in Cameras.Values.OfType<CombatCameraController>())
        {
            if (combatCamera == null)
            {
                Debug.LogWarning("Active camera is not a CombatCameraController.");
                continue;
            }

            if (combatCamera.TargetGroup == null)
            {
                Debug.LogError("Target group is not initialized in CombatCameraController.");
                continue;
            }

            if (combatCamera.cameraTarget == CameraTarget.Target)
            {
                Debug.LogWarning("Camera target is set to Target. Cannot set active character.");
                continue;
            }

            if (combatCamera.TargetGroup.Targets.Count == 0)
            {
                combatCamera.AddTarget(character);
            }
            else
            {
                // Update the first target in the group
                combatCamera.UpdateTargetAtIndex(character, 0);
            }

            combatCamera.UpdateFollowTarget(character);
        }

        activeCharacter = character;
    }

    public void SetActiveCombatCharacter(Entity entity)
    {
        SetActiveCombatCharacter(entity.transform);
    }
}
