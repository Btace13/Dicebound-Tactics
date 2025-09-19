using UnityEngine;
using Unity.Cinemachine;

public enum LevelCameraTargetMode
{
    None,
    Follow,
    Rotate
}

public class LevelCameraController : BaseCameraController
{
    [Header("Target Settings")]
    [SerializeField] private LevelCameraTargetMode targetMode = LevelCameraTargetMode.Follow;
    [SerializeField] private float cameraDistance = 10f;

    private Transform _currentTarget;
    private CinemachineFollow _cameraFollow;
    private CinemachineRotationComposer _cameraRotation;

    protected override void Start()
    {
        base.Start();

        _cameraFollow = CinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Body) as CinemachineFollow;
        _cameraRotation = CinemachineCam.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachineRotationComposer;

        _cameraFollow.enabled = targetMode == LevelCameraTargetMode.Follow;
        _cameraRotation.enabled = targetMode == LevelCameraTargetMode.Rotate || targetMode == LevelCameraTargetMode.Follow;
    }

    public override void SetCameraTarget(Transform t)
    {
        _currentTarget = t;

        switch (targetMode)
        {
            case LevelCameraTargetMode.Follow:
                base.SetCameraTarget(t);
                UpdateCameraDistance();
                break;
            case LevelCameraTargetMode.Rotate:
                base.SetCameraTarget(t);
                break;
            case LevelCameraTargetMode.None:
                base.SetCameraTarget(null);
                break;
        }
    }

    private void Update()
    {
        if (_currentTarget == null || targetMode == LevelCameraTargetMode.None)
            return;

        if (targetMode == LevelCameraTargetMode.Follow || targetMode == LevelCameraTargetMode.Rotate)
        {
            // Maintain distance from target
            Vector3 direction = (CinemachineCam.transform.position - _currentTarget.position).normalized;
            //CinemachineCam.transform.position = _currentTarget.position + direction * cameraDistance;

            if (targetMode == LevelCameraTargetMode.Rotate)
            {
                CinemachineCam.LookAt = _currentTarget;
            }
        }
    }

    private void UpdateCameraDistance()
    {
        if (CinemachineCam == null) return;

        if (_cameraFollow)
        {
            if (!_cameraFollow.enabled)
                _cameraFollow.enabled = true;

            var offset = _cameraFollow.FollowOffset;
            offset.z = -cameraDistance; // Negative if camera is behind the target
            _cameraFollow.FollowOffset = offset;
        }
    }

    public void SetCameraDistance(float distance)
    {
        cameraDistance = distance;
        UpdateCameraDistance();
    }
}
