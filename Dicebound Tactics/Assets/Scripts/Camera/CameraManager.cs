using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;
using Unity.VisualScripting;

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

    public ICameraController ActiveCamera { get; private set; }

    public UDictionary<string, ICameraController> Cameras = new UDictionary<string, ICameraController>();

    private Dictionary<string, CameraState> _cameraStates = new Dictionary<string, CameraState>();

    const int DefaultCameraPriority = 10;
    const int HighCameraPriority = 20;

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
    }

    public void RegisterCamera(string cameraName, ICameraController cameraController)
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

    public void SetActiveCamera(ICameraController cameraController)
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
    }

    public ICameraController GetActiveCamera()
    {
        return ActiveCamera;
    }

    public void TransitionToCamera(string cameraName, float blendTime = 0.5f)
    {
        if (Cameras.TryGetValue(cameraName, out ICameraController camera))
        {
            // Set this camera to high priority and others to lower priority
            foreach (var cam in Cameras.Values)
            {
                cam.SetCameraPriority(cam == camera ? 20 : 10);
            }

            // Could also use custom blending profiles or handle special transitions
        }
        else
        {
            Debug.LogWarning($"Camera {cameraName} not found in CameraManager");
        }
    }

    public void ShakeCamera(string cameraName, float intensity = 1f, float duration = 0.5f)
    {
        if (Cameras.TryGetValue(cameraName, out ICameraController camera))
        {
            if (camera.CinemachineCam == null)
            {
                Debug.LogError($"CinemachineCam is null in camera {cameraName}");
                return;
            }

            if (intensity <= 0 || duration <= 0)
            {
                Debug.LogWarning("Intensity and duration must be greater than zero for camera shake.");
                return;
            }

            CinemachineBasicMultiChannelPerlin noise = camera.CinemachineCam.GetOrAddComponent<CinemachineBasicMultiChannelPerlin>();
            noise.AmplitudeGain = intensity;

            DG.Tweening.Sequence s = DOTween.Sequence();
            s.AppendInterval(duration);
            s.AppendCallback(() => noise.AmplitudeGain = 0);
        }
    }


    public void SaveCameraState(string cameraName, string stateId)
    {
        if (Cameras.TryGetValue(cameraName, out ICameraController camera))
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
        if (Cameras.TryGetValue(cameraName, out ICameraController camera) &&
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
        if (Cameras.TryGetValue(cameraName, out ICameraController camera))
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
        if (ActiveCamera != null)
        {
            CombatCameraController combatCameraController = ActiveCamera as CombatCameraController;
            if (combatCameraController == null)
            {
                Debug.LogWarning("Active camera is not a CombatCameraController.");
                return;
            }

            if (combatCameraController.TargetGroup == null)
            {
                Debug.LogError("Target group is not initialized in CombatCameraController.");
                return;
            }

            if (combatCameraController.TargetGroup.Targets.Count == 1)
            {
                combatCameraController.AddTarget(target);
            }
            else
            {
                // Update the first target in the group
                combatCameraController.UpdateTargetAtIndex(target, 1);
            }
        }
        else
        {
            Debug.LogWarning("No active camera to set target for.");
        }
    }

    public void SetActiveCombatCharacter(Transform character)
    {
        if (ActiveCamera != null)
        {
            CombatCameraController combatCamera = ActiveCamera as CombatCameraController;
            if (combatCamera == null)
            {
                Debug.LogWarning("Active camera is not a CombatCameraController.");
                return;
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
        else
        {
            Debug.LogWarning("No active camera to set active character for.");
        }
    }
}
