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

    private float sharpTurnBoostTimer = 0f;
    private const float sharpTurnBoostDuration = 0.25f;
    private float originalRotationSpeed;
    private float originalAcceleration;
    private const float sharpTurnRotationSpeed = 2000f; // Boosted value
    private const float sharpTurnAcceleration = 100f;   // Boosted value

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        // Decay sharp turn boost
        if (sharpTurnBoostTimer > 0f)
        {
            sharpTurnBoostTimer -= Time.deltaTime;
            if (sharpTurnBoostTimer <= 0f)
            {
                // Restore original values
                pathfindingAI.rotationSpeed = originalRotationSpeed;
                pathfindingAI.acceleration = originalAcceleration;
            }
        }

        if (GameStateManager.Instance.CurrentGameState == GameState.Overworld && (Encounter == null || !Encounter.IsActive))
        {
            if (IsControlled)
            {
                if (InputManager.Instance == null || InputManager.Instance.InputActions == null)
                {
                    Debug.LogWarning("InputManager or InputActions is not initialized.");
                    return;
                }

                // Always read input value, don't check if pressed - this improves responsiveness
                Vector2 inputValue = InputManager.Instance.InputActions.Player.Move.ReadValue<Vector2>();
                HandleMovement(inputValue);
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

        // Apply deadzone for more precise control
        if (input.magnitude < 0.1f)
        {
            // Stop the character smoothly
            rvoController.velocity = Vector3.Lerp(rvoController.velocity, Vector3.zero, Time.deltaTime * 15f);
            pathfindingAI.SetPath(null);
            return;
        }

        Vector3 movement = input.x * right + input.y * forward;
        Vector3 zeroedYVelocity = movement.normalized;
        zeroedYVelocity.y = 0;

        // Detect sharp turn
        if (rvoController.velocity.magnitude > 0.1f && Vector3.Dot(rvoController.velocity.normalized, zeroedYVelocity) < -0.8f)
        {
            // Sharp turn: instantly stop and reorient
            rvoController.velocity = Vector3.zero;

            // Temporarily boost rotation speed and acceleration (on pathfindingAI only)
            if (sharpTurnBoostTimer <= 0f)
            {
                originalRotationSpeed = pathfindingAI.rotationSpeed;
                originalAcceleration = pathfindingAI.acceleration;
            }
            pathfindingAI.rotationSpeed = sharpTurnRotationSpeed;
            pathfindingAI.acceleration = sharpTurnAcceleration;
            sharpTurnBoostTimer = sharpTurnBoostDuration;
        }

        // Use consistent direct velocity control for player movement
        // This bypasses pathfinding for more responsive control
        pathfindingAI.SetPath(null); // Clear any existing paths
        
        // Apply movement with input magnitude for variable speed
        float targetSpeed = pathfindingAI.maxSpeed * input.magnitude;
        Vector3 targetVelocity = zeroedYVelocity * targetSpeed;
        
        // Set RVO velocity for movement calculation
        rvoController.velocity = targetVelocity;
        
        // Use CharacterController for physics-based movement with collision detection
        CharacterController characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            // Use CharacterController.Move for proper collision detection
            Vector3 moveVector = rvoController.velocity * Time.deltaTime;
            
            // Add gravity if not grounded
            if (!characterController.isGrounded)
            {
                moveVector.y += Physics.gravity.y * Time.deltaTime;
            }
            
            characterController.Move(moveVector);
        }
        else
        {
            // Fallback: Use Rigidbody if available
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.MovePosition(transform.position + rvoController.velocity * Time.deltaTime);
            }
            else
            {
                // Last resort: Direct transform movement (no collision)
                transform.position += rvoController.velocity * Time.deltaTime;
            }
        }
        
        // Handle rotation manually for immediate response
        if (zeroedYVelocity.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(zeroedYVelocity, Vector3.up);
            float rotationRate = 12f; // Faster rotation for responsiveness
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationRate);
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
            pathfindingAI.enableRotation = false; // Disable AI rotation for manual control
            pathfindingAI.updatePosition = false; // Disable AI position updates for manual control
            SetShouldSprint(false);
            
            // Clear any existing paths when taking control
            pathfindingAI.SetPath(null);
            rvoController.velocity = Vector3.zero;
        }
        else
        {
            isControlled = false;
            rvoController.collidesWith = 0 << 1; // Enable RVO collisions when not controlled
            rvoController.layer = RVOLayer.DefaultAgent; // Set to layer 31 when not controlled
            pathfindingAI.enableRotation = true; // Re-enable AI rotation
            pathfindingAI.updatePosition = true; // Re-enable AI position updates
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
            // Debug.LogWarning("PartyManager or PartyLeader is not initialized.");
            return false;
        }

        if (PartyManager.Instance.PartyLeader.GetComponent<OverworldCharacterController>() == null)
        {
            // Debug.LogWarning("PartyLeader does not have an OverworldCharacterController component.");
            return false;
        }

        // Check if the party leader is controlled and if following the leader is allowed
        return PartyManager.Instance.PartyLeader.OverworldCharacterController.IsControlled && CanFollowLeader;
    }

    public override void MoveToPosition(Vector3 targetPosition, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        base.MoveToPosition(targetPosition, overrideTime, onTargetReached);
        // print("Moving to position: " + targetPosition);
    }

    public override void MoveToTarget(Transform target, bool overrideTime = false, UnityAction onTargetReached = null)
    {
        base.MoveToTarget(target, overrideTime, onTargetReached);
        if (target != null)
        {
            // print("Moving to target: " + target.name);
        }
    }

    #endregion
}
