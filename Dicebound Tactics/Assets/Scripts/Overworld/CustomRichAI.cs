using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

public class CustomRichAI : RichAI
{
    public Quaternion desiredFinalRotation = Quaternion.identity;
    public UnityAction onTargetReached;

    private bool shouldRotateAtEnd = false;

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
