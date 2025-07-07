using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine.Events;

public class OverworldCharacterController : OverworldEntityController
{
    [BoxGroup("Control Settings"), SerializeField] protected bool isControlled = false;
    [BoxGroup("AI Movement Settings"), SerializeField] protected float aiFollowDistance = 5f;

    public bool IsControlled { get { return isControlled; } private set { isControlled = value; } }
    public bool CanFollowLeader { get; set; } = true;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        if (GameStateManager.Instance.CurrentGameState == GameState.Overworld && (Encounter == null || !Encounter.IsActive))
        {
            if (IsControlled)
            {
                if (InputManager.Instance == null || InputManager.Instance.InputActions == null)
                {
                    Debug.LogWarning("InputManager or InputActions is not initialized.");
                    return;
                }

                if (InputManager.Instance.InputActions.Player.Move.IsPressed())
                {
                    HandleMovement(InputManager.Instance.InputActions.Player.Move.ReadValue<Vector2>());
                }
                else
                {
                    HandleMovement(Vector2.zero);
                }
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

        base.Update();
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

        // Calculate movement directions relative to the camera
        var forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();
        var right = Camera.main.transform.right;
        right.y = 0;
        right.Normalize();

        if (pathfindingAI.updatePosition && input.magnitude < 0.01f)
        {
            // If the input is too small, stop the character
            rvoController.velocity = Vector3.zero;
            pathfindingAI.SetPath(null);
            return;
        }

        Vector3 movement = input.x * right + input.y * forward;

        Vector3 zeroedYVelocity = movement.normalized;
        zeroedYVelocity.y = 0;

        if (zeroedYVelocity.magnitude < 0.01f)
        {
            return; // Avoid setting rotation if there's no movement
        }

        if (!pathfindingAI.updatePosition)
        {
            if (pathfindingAI.hasPath)
            {
                pathfindingAI.SetPath(null);
            }

            rvoController.velocity = zeroedYVelocity * pathfindingAI.maxSpeed;
            transform.position += rvoController.velocity * Time.deltaTime;
        }
        else
        {
            pathfindingAI.destination = transform.position + zeroedYVelocity * pathfindingAI.maxSpeed;
        }

        if (!pathfindingAI.enableRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(zeroedYVelocity, Vector3.up);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            pathfindingAI.desiredFinalRotation = targetRotation;
        }
    }
    #endregion

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Overworld)
        {
            isControlled = true;
            rvoController.collidesWith = 0 << 0; // Disable RVO collisions when controlled
            rvoController.layer = RVOLayer.Layer30; // Set to layer 30 when controlled
            pathfindingAI.enableRotation = false;
            SetShouldSprint(false);
        }
        else
        {
            isControlled = false;
            rvoController.collidesWith = 0 << 1; // Enable RVO collisions when not controlled
            rvoController.layer = RVOLayer.DefaultAgent; // Set to layer 31 when not controlled
            pathfindingAI.enableRotation = true;
            SetShouldSprint(true);
        }
    }

    #region AI MOVEMENT
    public bool CheckShouldFollowLeader()
    {
        if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentGameState != GameState.Overworld)
        {
            return false; // Only follow leader in Overworld state
        }

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

    #endregion
}
