using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraTriggerZone : MonoBehaviour
{
    [SerializeField] private LevelCameraController levelCameraController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<OverworldCharacterController>(out var characterController))
        {
            if (characterController.IsControlled)
            {
                CameraManager.Instance.SetActiveCamera(levelCameraController);
                levelCameraController.SetCameraTarget(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<OverworldCharacterController>(out var characterController))
        {
            if (characterController.IsControlled)
            {
                CameraManager.Instance.TrySetActiveCamera("OverworldCamera");
                levelCameraController.SetCameraTarget(null);
            }
        }
    }
}