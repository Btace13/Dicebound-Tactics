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
        }
    }

    void Update()
    {
        if (shouldRotateAtEnd)
        {
            enableRotation = false;

            rotation = Quaternion.RotateTowards(
                rotation,
                desiredFinalRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Mathf.Abs(Quaternion.Angle(rotation, desiredFinalRotation)) < 1f)
            {
                shouldRotateAtEnd = false;
                enableRotation = true; // Re-enable rotation after reaching the target
            }
        }
    }
}
