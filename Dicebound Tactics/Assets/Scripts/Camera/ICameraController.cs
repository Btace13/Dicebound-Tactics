using UnityEngine;
using Unity.Cinemachine;

public interface ICameraController
{
    public string CameraName { get; set; }
    public CinemachineCamera CinemachineCam { get; set; }
    public void SetCameraTarget(Transform t);
    public void UpdateCameraTarget(Transform target, float duration = 0f);
    public void SetCameraPriority(int priority);
    public void RegisterCamera(string cameraName);
}
