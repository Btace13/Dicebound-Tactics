using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class OverworldCameraController : BaseCameraController
{
    [SerializeField] private float _maxZoomDistance = 20f;
    [SerializeField] private float _minZoomDistance = 5f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _panSpeed = 20f;
    [SerializeField] private bool _enableEdgePanning = true;
    [SerializeField] private float _edgePanThreshold = 20f;

    private float _currentZoomLevel;
    private CinemachinePositionComposer _positionTransposer;

    protected override void Awake()
    {
        base.Awake();
        _cameraName = "OverworldCamera";

        if (CinemachineCam != null)
        {
            // Get the framing transposer to control zoom
            _positionTransposer = CinemachineCam.GetComponent<CinemachineCamera>()?.GetComponent<CinemachinePositionComposer>();

            if (_positionTransposer != null)
            {
                _currentZoomLevel = _positionTransposer.CameraDistance;
            }
        }
    }

    private void Update()
    {
        HandleZoom();
        HandlePanning();
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

    private void HandlePanning()
    {
        // WASD keyboard panning
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || (Input.mousePosition.y >= Screen.height - _edgePanThreshold && _enableEdgePanning))
        {
            moveDirection += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S) || (Input.mousePosition.y <= _edgePanThreshold && _enableEdgePanning))
        {
            moveDirection += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A) || (Input.mousePosition.x <= _edgePanThreshold && _enableEdgePanning))
        {
            moveDirection += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D) || (Input.mousePosition.x >= Screen.width - _edgePanThreshold && _enableEdgePanning))
        {
            moveDirection += Vector3.right;
        }

        if (moveDirection != Vector3.zero)
        {
            // Convert from local to world coordinates
            moveDirection = transform.TransformDirection(moveDirection.normalized);
            // Only move in XZ plane
            moveDirection.y = 0;

            transform.position += moveDirection * _panSpeed * Time.deltaTime;
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

    public void SetPanningEnabled(bool enabled)
    {
        _enableEdgePanning = enabled;
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
