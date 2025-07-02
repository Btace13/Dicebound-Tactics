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
            if (desiredFinalRotation == Quaternion.identity)
            {
                desiredFinalRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            }

            rotation = Quaternion.RotateTowards(
                rotation,
                desiredFinalRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(rotation, desiredFinalRotation) < 1f)
            {
                shouldRotateAtEnd = false;
            }
        }
    }
}
