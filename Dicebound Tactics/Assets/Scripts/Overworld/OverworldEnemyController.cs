using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Pathfinding;
using Pathfinding.RVO;
using Unity.VisualScripting;

public class OverworldEnemyController : OverworldEntityController
{
    [BoxGroup("AI Movement Settings"), SerializeField] protected float viewDistance = 5f;
    [BoxGroup("AI Movement Settings"), SerializeField] protected float fovAngle = 120f;
    [BoxGroup("AI Movement Settings"), SerializeField] protected float chaseDelay = 1f;

    public bool HasPath => pathfindingAI.hasPath;
    public bool HasReachedDestination => pathfindingAI.reachedEndOfPath;
    public bool HasSpottedTarget { get; private set; } = false;
    public Vector3 LastKnownTargetPosition { get; private set; } = Vector3.positiveInfinity;
    public bool IsChasingTarget => LastKnownTargetPosition != Vector3.positiveInfinity && !HasReachedDestination;

    private float _timeSinceSeen = 0;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Overworld)
        {
            SetShouldSprint(false);
        }
        else
        {
            SetShouldSprint(true);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.transform == PartyManager.Instance.PartyLeader.transform)
        {
            if (Encounter != null && Encounter.IsAntagonistic && !Encounter.IsActive)
            {
                Encounter.StartEncounter();
            }
        }
    }

    #region PATHFINDING MOVEMENT
    public void MoveToLastKnownTargetPosition(UnityAction onTargetReached = null)
    {
        if (LastKnownTargetPosition != Vector3.positiveInfinity)
        {
            MoveToPosition(LastKnownTargetPosition, false, onTargetReached);
        }
    }
    #endregion

    #region SENSORS
    public bool CanSeeTarget(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Target Transform is null.");
            return false;
        }

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        if (distanceToTarget <= viewDistance)
        {
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget > fovAngle / 2f)
            {
                return false; // Target is outside field of view
            }

            if (Physics.Raycast(transform.position + Vector3.up * 1f, directionToTarget, out RaycastHit hit, viewDistance))
            {
                if (hit.transform == target)
                {
                    UpdateLastKnownTargetPosition(target.position);
                    return true; // Target is visible
                }
            }
        }

        return false; // Target is not visible
    }

    public void UpdateLastKnownTargetPosition(Vector3 position)
    {
        if (LastKnownTargetPosition == Vector3.positiveInfinity && _timeSinceSeen < chaseDelay)
        {
            CancelPath(); // Cancel path if we haven't seen the target for a while
            HasSpottedTarget = true;
            _timeSinceSeen += Time.deltaTime;
            return; // Delay before updating last known position
        }

        LastKnownTargetPosition = position;
    }

    public void ClearLastKnownTargetPosition()
    {
        LastKnownTargetPosition = Vector3.positiveInfinity;
        _timeSinceSeen = 0f; // Reset the time since last seen
        HasSpottedTarget = false; // Reset spotted state
    }

    #endregion
}