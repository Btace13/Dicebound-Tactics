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
    [BoxGroup("AI Movement Settings"), SerializeField] protected float chaseTimeout = 5f;
    [BoxGroup("AI Movement Settings"), SerializeField] protected float maxChaseDistance = 10f;

    public bool HasPath => pathfindingAI.hasPath;
    public bool HasReachedDestination => pathfindingAI.reachedEndOfPath;
    public bool HasSpottedTarget { get; private set; } = false;
    public Vector3? LastKnownTargetPosition { get; private set; } = null;
    public bool IsChasingTarget => LastKnownTargetPosition.HasValue;

    private float _timeSinceSeen = 0;

    protected override void Awake()
    {
        base.Awake();

        // Initialize pathfinding AI
        LastKnownTargetPosition = Vector3.positiveInfinity;
        HasSpottedTarget = false;
    }

    protected override void Update()
    {
        base.Update();

        if (IsChasingTarget)
        {
            _timeSinceSeen += Time.deltaTime;
            float distanceToLastKnown = Vector3.Distance(transform.position, LastKnownTargetPosition.Value);
            if (_timeSinceSeen > chaseTimeout || distanceToLastKnown > maxChaseDistance)
            {
                ClearLastKnownTargetPosition();
            }
        }
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
            MoveToPosition(LastKnownTargetPosition.Value, false, onTargetReached);
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
        _timeSinceSeen = 0f; // Reset the time since last seen
        HasSpottedTarget = true; // Set spotted state to true
        LastKnownTargetPosition = position;
    }

    public void ClearLastKnownTargetPosition()
    {
        LastKnownTargetPosition = null;
        HasSpottedTarget = false; // Reset spotted state
    }

    #endregion
}