using UnityEngine;
using Unity.Cinemachine;

public class CombatCameraController : BaseCameraController
{
    protected CinemachineTargetGroup _targetGroup;
    public CinemachineTargetGroup TargetGroup => _targetGroup;

    public Transform FollowTarget { get; set; }


    protected override void Start()
    {
        base.Start();

        CreateTargetGroup();
    }

    // Additional combat-specific camera functionality can be added here
    private void CreateTargetGroup()
    {
        GameObject targetGroupObj = new GameObject($"{_cameraName}TargetGroup");

        _targetGroup = targetGroupObj.AddComponent<CinemachineTargetGroup>();

        if (CinemachineCam != null)
        {
            CinemachineCam.LookAt = _targetGroup.transform;
        }
    }

    public void AddTarget(Transform target, float weight = 1f, float radius = 1f)
    {
        if (_targetGroup != null)
        {
            _targetGroup.AddMember(target, weight, radius);
        }
        else
        {
            Debug.LogError("Target group is not initialized.");
        }
    }

    public void RemoveTarget(Transform target)
    {
        if (_targetGroup != null)
        {
            _targetGroup.RemoveMember(target);
        }
        else
        {
            Debug.LogError("Target group is not initialized.");
        }
    }

    public void UpdateFollowTarget(Transform target)
    {
        if (CinemachineCam != null)
        {
            CinemachineCam.Follow = target;
            FollowTarget = target;
        }
        else
        {
            Debug.LogError("CinemachineCam is not initialized.");
        }
    }

    public void UpdateTargetAtIndex(Transform target, int index, float weight = 1f, float radius = 1f)
    {
        if (_targetGroup != null)
        {
            if (index < 0 || index >= _targetGroup.Targets.Count)
            {
                Debug.LogError("Index out of range for target group.");
                return;
            }

            _targetGroup.Targets[index].Object = target;
            _targetGroup.Targets[index].Weight = weight;
            _targetGroup.Targets[index].Radius = radius;
        }
        else
        {
            Debug.LogError("Target group is not initialized.");
        }
    }
}
