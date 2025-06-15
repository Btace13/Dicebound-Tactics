using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

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
}
