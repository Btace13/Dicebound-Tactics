using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Pathfinding;
using Pathfinding.RVO;
using Unity.VisualScripting;

public class OverworldEnemyController : MonoBehaviour
{
    [BoxGroup("Control Settings"), SerializeField] protected float moveSpeed = 5f;
    [BoxGroup("Control Settings"), SerializeField] protected float defaultAcceleration = 10f;
    [BoxGroup("Control Settings"), SerializeField] protected float sprintSpeed = 20f;
    [BoxGroup("Control Settings"), SerializeField] protected float sprintAcceleration = 100f;

    [BoxGroup("Control Settings"), SerializeField] protected float rotationSpeed = 720f;

    [BoxGroup("AI Movement Settings"), SerializeField] protected float aiFollowDistance = 5f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float nextWaypointDistance = 0.1f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float repathRate = 0.5f;

    public bool HasPath => pathfindingAI.hasPath;
    public bool HasReachedDestination => pathfindingAI.reachedEndOfPath;

    private CustomRichAI pathfindingAI;
    private RVOController rvoController;
    private UnitAnimationHandler unitAnimationHandler;
    private float lastRepath = float.NegativeInfinity;

    [ShowInInspector, ReadOnly] private Vector3 _currentVelocity;

    private void Awake()
    {
        rvoController = gameObject.GetOrAddComponent<RVOController>();
        pathfindingAI = gameObject.GetOrAddComponent<CustomRichAI>();
        pathfindingAI.maxSpeed = moveSpeed;
        unitAnimationHandler = gameObject.GetComponentInChildren<UnitAnimationHandler>(true);
    }

    public void Update()
    {
        // Update the animation state based on the current velocity
        UpdateAnimationState();
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

    public void SetShouldSprint(bool shouldSprint)
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

    #region PATHFINDING MOVEMENT
    public void MoveToTarget(Transform target, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        if (pathfindingAI == null)
        {
            Debug.LogWarning("pathfindingAI is not initialized.");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("Target is null. Cannot move to a null target.");
            return;
        }

        pathfindingAI.desiredFinalRotation = target.rotation;
        MoveToPosition(target.position, overrideTime, onTargetReached);
    }

    public void MoveToPosition(Vector3 targetPosition, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        if (pathfindingAI == null)
        {
            Debug.LogWarning("pathfindingAI is not initialized.");
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

    public void CancelPath()
    {
        print("Canceling path");

        if (pathfindingAI != null)
        {
            pathfindingAI.destination = transform.position; // Set destination to current position to stop moving
            pathfindingAI.SearchPath(); // Force a search to update the AI state
        }
    }
    #endregion

    #region ANIMATION HANDLING

    private void UpdateAnimationState()
    {
        if (unitAnimationHandler == null)
        {
            Debug.LogWarning("UnitAnimationHandler is not assigned.");
            return;
        }

        _currentVelocity = transform.InverseTransformDirection(pathfindingAI.velocity / Time.deltaTime);

        unitAnimationHandler.OnUnitVelocityChange(new Vector2(_currentVelocity.x, _currentVelocity.z) / pathfindingAI.maxSpeed);
    }
    #endregion
}
