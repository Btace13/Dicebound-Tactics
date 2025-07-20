using UnityEngine;
using Sirenix.OdinInspector;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine.Events;
using UnityExtensions;

public class OverworldEntityController : MonoBehaviour
{
    [BoxGroup("Control Settings"), SerializeField] protected float moveSpeed = 5f;
    [BoxGroup("Control Settings"), SerializeField] protected float defaultAcceleration = 10f;
    [BoxGroup("Control Settings"), SerializeField] protected float sprintSpeed = 20f;
    [BoxGroup("Control Settings"), SerializeField] protected float sprintAcceleration = 100f;

    [BoxGroup("Control Settings"), SerializeField] protected float rotationSpeed = 720f;

    [BoxGroup("AI Movement Settings"), SerializeField] public float repathRate = 0.5f;

    protected CustomRichAI pathfindingAI;
    protected RVOController rvoController;
    protected UnitAnimationHandler unitAnimationHandler;
    protected float lastRepath = float.NegativeInfinity;
    [ShowInInspector, ReadOnly] protected Vector3 _currentVelocity = Vector3.zero;
    public CombatEncounter Encounter { get; set; }
    public CombatEncounter.EncounterSlot AssignedEncounterSlot { get; set; }

    protected virtual void Awake()
    {
        rvoController = gameObject.AddOrGetComponent<RVOController>();
        pathfindingAI = gameObject.AddOrGetComponent<CustomRichAI>();
        pathfindingAI.maxSpeed = moveSpeed;
        unitAnimationHandler = gameObject.GetComponentInChildren<UnitAnimationHandler>(true);
    }

    protected virtual void Update()
    {
        UpdateAnimationState();
    }

    public virtual void SetShouldSprint(bool shouldSprint)
    {
        if (shouldSprint)
        {
            pathfindingAI.maxSpeed = sprintSpeed;
            pathfindingAI.acceleration = sprintAcceleration;
        }
        else
        {
            pathfindingAI.maxSpeed = moveSpeed;
            pathfindingAI.acceleration = defaultAcceleration;
        }
    }

    public virtual void MoveToTarget(Transform target, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        if (pathfindingAI == null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        pathfindingAI.desiredFinalRotation = target.rotation;
        MoveToPosition(target.position, overrideTime, onTargetReached);
    }

    public virtual void MoveToPosition(Vector3 targetPosition, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        if (pathfindingAI == null)
        {
            return;
        }

        if (rvoController.locked)
        {
            rvoController.locked = false;
        }

        if (overrideTime || Time.time > lastRepath + repathRate)
        {
            lastRepath = Time.time;

            pathfindingAI.onTargetReached = onTargetReached;
            pathfindingAI.destination = targetPosition;
            pathfindingAI.SearchPath();
        }
    }

    public virtual void CancelPath()
    {
        if (pathfindingAI != null && pathfindingAI.hasPath)
        {
            pathfindingAI.SetPath(null);
        }
    }

    protected virtual void UpdateAnimationState()
    {
        if (unitAnimationHandler == null)
        {
            return;
        }

        _currentVelocity = Vector3.LerpUnclamped(_currentVelocity, transform.InverseTransformDirection(rvoController.velocity), 10f * Time.deltaTime);

        unitAnimationHandler.OnUnitVelocityChange(new Vector2(_currentVelocity.x, _currentVelocity.z) / pathfindingAI.maxSpeed);
    }
}
