using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

public class CustomRichAI : RichAI
{
    public Quaternion desiredFinalRotation = Quaternion.identity;
    public UnityAction onTargetReached;

    private bool shouldRotateAtEnd = false;
    private LeapMovementController leapController;

    protected override void Start()
    {
        base.Start();
        leapController = GetComponent<LeapMovementController>();
    }

    /// <summary>
    /// Check if this AI is currently leaping (and should not use pathfinding)
    /// </summary>
    public bool IsLeaping => leapController != null && leapController.IsLeaping;

    /// <summary>
    /// Access to the leap controller for external components
    /// </summary>
    public LeapMovementController LeapController => leapController;

    /// <summary>
    /// Try to leap to a target transform if leap controller is available
    /// </summary>
    /// <param name="target">Target transform to leap to</param>
    /// <param name="onComplete">Callback when leap is completed</param>
    /// <returns>True if leap was initiated, false if using normal pathfinding</returns>
    public bool TryLeapToTarget(Transform target, UnityAction onComplete = null)
    {
        if (leapController == null)
        {
            Debug.LogWarning($"No LeapMovementController found on {name}. Using normal pathfinding.");
            return false;
        }

        // Clear any existing path and suspend pathfinding while leaping
        SetPath(null);
        canMove = false;
        
        leapController.LeapToTarget(target, () =>
        {
            // Set destination to current position to prevent unwanted movement
            destination = transform.position;
            // Re-enable pathfinding after leap
            canMove = true;
            onComplete?.Invoke();
        });
        
        return true;
    }

    /// <summary>
    /// Try to leap to a target position if leap controller is available
    /// </summary>
    /// <param name="targetPosition">Target position to leap to</param>
    /// <param name="onComplete">Callback when leap is completed</param>
    /// <returns>True if leap was initiated, false if using normal pathfinding</returns>
    public bool TryLeapToPosition(Vector3 targetPosition, UnityAction onComplete = null)
    {
        if (leapController == null)
        {
            Debug.LogWarning($"No LeapMovementController found on {name}. Using normal pathfinding.");
            return false;
        }

        // Clear any existing path and suspend pathfinding while leaping
        SetPath(null);
        canMove = false;
        
        leapController.LeapToPosition(targetPosition, () =>
        {
            // Set destination to current position to prevent unwanted movement
            destination = transform.position;
            // Re-enable pathfinding after leap
            canMove = true;
            onComplete?.Invoke();
        });
        
        return true;
    }

    protected override void OnTargetReached()
    {
        base.OnTargetReached();
        shouldRotateAtEnd = true;

        if (onTargetReached != null)
        {
            onTargetReached.Invoke();
            onTargetReached = null; // Clear the callback after invoking
        }
    }

    void Update()
    {
        // Don't run normal pathfinding update if we're currently leaping
        if (IsLeaping)
        {
            return;
        }

        if (shouldRotateAtEnd)
        {
            bool tmpRotation = enableRotation;
            enableRotation = false;

            rotation = Quaternion.RotateTowards(
                rotation,
                desiredFinalRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Mathf.Abs(Quaternion.Angle(rotation, desiredFinalRotation)) < 1f)
            {
                shouldRotateAtEnd = false;
            }
        }
        else
        {
            if (velocity.magnitude < 0.05f)
            {
                Vector3 rotationDirection = transform.forward;
                rotationDirection.y = 0; // Keep only the horizontal direction
                rotation = Quaternion.RotateTowards(
                    rotation,
                    Quaternion.LookRotation(rotationDirection, Vector3.up),
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {
                Vector3 flatVelocity = new Vector3(velocity.x, 0, velocity.z); // Ignore y-axis for rotation
                rotation = Quaternion.RotateTowards(
                    rotation,
                    Quaternion.LookRotation(flatVelocity, Vector3.up),
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
