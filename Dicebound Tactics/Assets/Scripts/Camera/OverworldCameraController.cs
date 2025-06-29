using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class OverworldCameraController : BaseCameraController
{
    [SerializeField] private float _maxZoomDistance = 20f;
    [SerializeField] private float _minZoomDistance = 5f;
    [SerializeField] private float _zoomSpeed = 2f;

    private float _currentZoomLevel;
    private CinemachinePositionComposer _positionTransposer;

    protected override void Start()
    {
        base.Start();
        _cameraName = "OverworldCamera";

        if (CinemachineCam != null)
        {
            // Get the framing transposer to control zoom
            _positionTransposer = CinemachineCam.GetComponent<CinemachineCamera>()?.GetComponent<CinemachinePositionComposer>();

            if (_positionTransposer != null)
            {
                _currentZoomLevel = _positionTransposer.CameraDistance;
            }

            CinemachineCam.Follow = PartyManager.Instance.PartyLeader.transform;
            CinemachineCam.LookAt = PartyManager.Instance.PartyLeader.transform;
        }
    }

    private void Update()
    {
        HandleZoom();
    }

    private void HandleZoom()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0 && _positionTransposer != null)
        {
            _currentZoomLevel -= scrollDelta * _zoomSpeed;
            _currentZoomLevel = Mathf.Clamp(_currentZoomLevel, _minZoomDistance, _maxZoomDistance);
            _positionTransposer.CameraDistance = _currentZoomLevel;
        }
    }

    public void SetZoomLevel(float zoomLevel, float duration = 0f)
    {
        if (_positionTransposer == null) return;

        float targetZoom = Mathf.Clamp(zoomLevel, _minZoomDistance, _maxZoomDistance);

        if (duration > 0f)
        {
            DOTween.To(() => _currentZoomLevel,
                x =>
                {
                    _currentZoomLevel = x;
                    _positionTransposer.CameraDistance = x;
                },
                targetZoom, duration).SetEase(Ease.InOutQuad);
        }
        else
        {
            _currentZoomLevel = targetZoom;
            _positionTransposer.CameraDistance = targetZoom;
        }
    }

    // Override UpdateCameraTarget to maintain zoom level when changing targets
    public override void UpdateCameraTarget(Transform target, float duration = 0f)
    {
        float currentZoom = _currentZoomLevel;
        base.UpdateCameraTarget(target, duration);

        // Restore zoom level after changing target
        if (_positionTransposer != null)
        {
            _positionTransposer.CameraDistance = currentZoom;
        }
    }
}
