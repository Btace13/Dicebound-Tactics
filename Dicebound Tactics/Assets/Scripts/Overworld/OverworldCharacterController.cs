using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine.Events;

public class OverworldCharacterController : MonoBehaviour
{
    [BoxGroup("Control Settings"), SerializeField] protected float overworldMoveSpeed = 5f;
    [BoxGroup("Control Settings"), SerializeField] protected float combatMoveSpeed = 20f;
    public float moveSpeed
    {
        get
        {
            return GameStateManager.Instance.CurrentGameState == GameState.Overworld ? overworldMoveSpeed : combatMoveSpeed;
        }
    }
    [BoxGroup("Control Settings"), SerializeField] protected float rotationSpeed = 720f;
    [BoxGroup("Control Settings"), SerializeField] protected bool isControlled = false;

    [BoxGroup("AI Movement Settings"), SerializeField] protected float aiFollowDistance = 5f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float nextWaypointDistance = 0.1f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float repathRate = 0.5f;

    private CustomRichAI pathfindingAI;
    private RVOController rvoController;
    private float lastRepath = float.NegativeInfinity;

    public bool IsControlled { get { return isControlled; } private set { isControlled = value; } }
    public bool CanFollowLeader { get; set; } = true;

    private void Awake()
    {
        rvoController = gameObject.GetOrAddComponent<RVOController>();
        pathfindingAI = gameObject.GetOrAddComponent<CustomRichAI>();
        pathfindingAI.maxSpeed = moveSpeed;
    }


    public void Update()
    {
        if (InputManager.Instance == null || InputManager.Instance.InputActions == null)
        {
            Debug.LogWarning("InputManager or InputActions is not initialized.");
            return;
        }

        if (IsControlled)
        {
            HandleMovement(InputManager.Instance.InputActions.Player.Move.ReadValue<Vector2>());
        }
        else
        {
            // If not controlled, check if we should follow the leader
            if (CheckShouldFollowLeader())
            {
                Vector3 leaderPosition = PartyManager.Instance.PartyLeader.transform.position;

                float distanceToLeader = Vector3.Distance(transform.position, leaderPosition);

                if (pathfindingAI.hasPath)
                {
                    float distanceFromDestination = Vector3.Distance(pathfindingAI.destination, leaderPosition);

                    // Check if the leader is too far away
                    if (distanceFromDestination > aiFollowDistance || distanceToLeader > aiFollowDistance)
                    {
                        // If the leader is too far away, we need to move towards the leader's position
                        MoveToPosition(leaderPosition);
                    }
                    else if (distanceToLeader <= aiFollowDistance)
                    {
                        // If the leader is within the follow distance, we can stop moving
                        CancelPath();
                    }
                }
                else if (distanceToLeader > aiFollowDistance)
                {
                    // If no path exists, start a new path towards the leader
                    MoveToPosition(leaderPosition);
                }
            }
        }
    }

    #region PLAYER MOVEMENT
    private void HandleMovement(Vector2 input)
    {
        if (!IsControlled)
        {
            return;
        }

        if (rvoController.locked)
        {
            rvoController.locked = false; // Unlock RVO controller if it is locked
        }

        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            rvoController.velocity = moveDirection * moveSpeed;
            pathfindingAI.destination = transform.position + moveDirection * moveSpeed * Time.deltaTime;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            pathfindingAI.desiredFinalRotation = targetRotation;
        }
    }
    #endregion

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Overworld)
        {
            isControlled = true;
        }
        else
        {
            isControlled = false;
        }
    }


    #region AI MOVEMENT
    public bool CheckShouldFollowLeader()
    {
        if (PartyManager.Instance == null || PartyManager.Instance.PartyLeader == null)
        {
            Debug.LogWarning("PartyManager or PartyLeader is not initialized.");
            return false;
        }

        if (PartyManager.Instance.PartyLeader.GetComponent<OverworldCharacterController>() == null)
        {
            Debug.LogWarning("PartyLeader does not have an OverworldCharacterController component.");
            return false;
        }

        // Check if the party leader is controlled and if following the leader is allowed
        return PartyManager.Instance.PartyLeader.OverworldCharacterController.IsControlled && CanFollowLeader;
    }

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
}
