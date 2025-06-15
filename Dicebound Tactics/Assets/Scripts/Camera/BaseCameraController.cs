using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening;

public class BaseCameraController : MonoBehaviour, ICameraController
{
    public CinemachineCamera CinemachineCam { get; set; }

    public string CameraName { get => _cameraName; set => _cameraName = value; }
    [SerializeField] protected string _cameraName = "BaseCamera";

    protected Tween _moveTween;

    protected virtual void Awake()
    {
        CinemachineCam = GetComponent<CinemachineCamera>();
    }

    protected virtual void Start()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.RegisterCamera(CameraName, this);
        }
        else
        {
            Debug.LogError($"CameraManager instance is null in {GetType().Name} Start");
        }
    }

    protected virtual void OnDestroy()
    {
        if (_moveTween != null && _moveTween.IsActive())
        {
            _moveTween.Kill();
        }
    }

    public virtual void SetCameraTarget(Transform t)
    {
        if (CinemachineCam == null)
        {
            Debug.LogError($"CinemachineCam is null in {GetType().Name}");
            return;
        }

        CinemachineCam.Follow = t;
        CinemachineCam.LookAt = t;
    }

    public virtual void UpdateCameraTarget(Transform target, float duration = 0f)
    {
        if (CinemachineCam == null)
        {
            Debug.LogError($"CinemachineCam is null in {GetType().Name}");
            return;
        }

        if (duration > 0f)
        {
            if (_moveTween != null && _moveTween.IsActive())
            {
                _moveTween.Kill();
            }

            _moveTween = DOTween.To(
                () => CinemachineCam.transform.position,
                pos => CinemachineCam.transform.position = pos,
                target.position,
                duration
            ).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                CinemachineCam.Follow = target;
                CinemachineCam.LookAt = target;
            });
            return;
        }

        CinemachineCam.Follow = target;
        CinemachineCam.LookAt = target;
    }

    public virtual void SetCameraPriority(int priority)
    {
        if (CinemachineCam == null)
        {
            Debug.LogError($"CinemachineCam is null in {GetType().Name}");
            return;
        }

        CinemachineCam.Priority = priority;
    }

    public virtual void RegisterCamera(string cameraName)
    {
        CameraName = cameraName;
    }
}
