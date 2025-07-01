using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Pathfinding;

public class OverworldCharacterController : MonoBehaviour
{
    [BoxGroup("Control Settings"), SerializeField] protected float moveSpeed = 5f;
    [BoxGroup("Control Settings"), SerializeField] protected float rotationSpeed = 720f;
    [BoxGroup("Control Settings"), SerializeField] protected bool isControlled = false;

    [BoxGroup("AI Movement Settings"), SerializeField] protected float aiFollowDistance = 2f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float nextWaypointDistance = 0.1f;
    [BoxGroup("AI Movement Settings"), SerializeField] public float repathRate = 0.5f;

    private CharacterController characterController;
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    private float lastRepath = float.NegativeInfinity;

    [HideInInspector] public bool reachedEndOfPath;
    public bool IsControlled { get { return isControlled; } private set { isControlled = value; } }
    public bool CanFollowLeader { get; set; } = true;

    private void Awake()
    {
        characterController = gameObject.GetOrAddComponent<CharacterController>();
        seeker = gameObject.GetOrAddComponent<Seeker>();
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

                if (path != null)
                {
                    float distanceFromDestination = Vector3.Distance(path.vectorPath[path.vectorPath.Count - 1], leaderPosition);

                    // Check if the leader is too far away
                    if (distanceFromDestination > aiFollowDistance && distanceToLeader > aiFollowDistance)
                    {
                        // If the leader is too far away, we need to move towards the leader's position
                        MoveToPosition(leaderPosition);
                    }
                    else if (distanceToLeader <= aiFollowDistance)
                    {
                        // If the leader is within the follow distance, we can stop moving
                        CancelPath();
                    }
                    else
                    {
                        // If the leader is within the follow distance, we can continue following the path
                        FollowPath();
                    }
                }
                else if (path == null && distanceToLeader > aiFollowDistance)
                {
                    // If no path exists, start a new path towards the leader
                    MoveToPosition(leaderPosition);
                }
                else if (path != null)
                {
                    // If we are close enough to the leader, we can stop moving
                    CancelPath();
                }
            }
            else
            {
                if (path != null)
                {
                    CancelPath(); // Cancel the path if we cannot follow the leader
                }
            }
        }
    }

    private void HandleMovement(Vector2 input)
    {
        if (!IsControlled || characterController == null)
        {
            return;
        }

        Vector3 moveDirection = new Vector3(input.x, 0, input.y).normalized;

        if (moveDirection != Vector3.zero)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

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

    #region AI MOVEMENT
    public void MoveToPosition(Vector3 targetPosition)
    {
        if (characterController == null)
        {
            Debug.LogWarning("CharacterController is not initialized.");
            return;
        }

        if (seeker == null)
        {
            Debug.LogWarning("Seeker is not initialized.");
            return;
        }

        if (Time.time > lastRepath + repathRate && seeker.IsDone())
        {
            lastRepath = Time.time;

            // when the path has been calculated (which may take a few frames depending on the complexity)
            seeker.StartPath(transform.position, targetPosition, OnPathComplete);
        }
    }

    private void OnPathComplete(Path p)
    {
        p.Claim(this);
        if (!p.error)
        {
            if (path != null) path.Release(this);
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;
        }
        else
        {
            p.Release(this);
        }
    }

    public void FollowPath()
    {
        if (path == null || path.vectorPath.Count == 0)
        {
            Debug.LogWarning("No valid path to follow.");
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;

        float distanceToWaypoint;

        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
            if (distanceToWaypoint < nextWaypointDistance)
            {
                // Check if there is another waypoint or if we have reached the end of the path
                if (currentWaypoint + 1 < path.vectorPath.Count)
                {
                    currentWaypoint++;
                }
                else
                {
                    reachedEndOfPath = true;
                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector3 velocity = dir * moveSpeed * speedFactor;

        print($"Moving towards waypoint {currentWaypoint} at velocity {velocity}");

        // Move the character controller in the direction of the current waypoint
        characterController.Move(velocity * Time.deltaTime);
        dir.y = 0; // Ensure the direction is constrained to the XZ plane
        characterController.transform.rotation = Quaternion.RotateTowards(characterController.transform.rotation, Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
    }

    public void CancelPath()
    {
        print("Canceling path");
        path = null;
        path.Release(this);
    }
    #endregion
}
