using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;

/// <summary>
/// Component that handles leap/jump movement for characters, allowing them to smoothly jump to target positions
/// instead of using normal pathfinding. Useful for dramatic encounter transitions.
/// </summary>
public class LeapMovementController : MonoBehaviour
{
    [BoxGroup("Leap Settings"), SerializeField]
    private float maxLeapTime = 3.0f; // Max seconds allowed for a leap
    [BoxGroup("Leap Settings"), SerializeField]
    private float maxLeapDistance = 20.0f; // Max distance allowed from target before teleport
    [BoxGroup("Leap Settings"), SerializeField] 
    private float leapDuration = 1.0f;
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private float leapHeight = 3.0f;
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private AnimationCurve leapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private bool rotateTowardsTarget = true;

    [BoxGroup("Animation Integration"), SerializeField] 
    private bool useLeapAnimation = true;

    private UnitAnimationHandler animationHandler;
    private Rigidbody rb;
    private bool isLeaping = false;
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private Coroutine currentLeapCoroutine;
    private bool wasKinematic = false;

    public bool IsLeaping => isLeaping;

    // Events
    public UnityEvent OnLeapStarted;
    public UnityEvent OnLeapCompleted;
    public UnityEvent OnLeapLanded;

    private void Awake()
    {
        animationHandler = GetComponentInChildren<UnitAnimationHandler>();
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Performs a leap to the target position with a smooth arc animation
    /// </summary>
    /// <param name="target">Target position to leap to</param>
    /// <param name="onComplete">Callback when leap is completed</param>
    public void LeapToPosition(Vector3 target, UnityAction onComplete = null)
    {
        if (isLeaping)
        {
            CancelLeap();
        }

        targetPosition = target;
        originalPosition = transform.position;
        
        currentLeapCoroutine = StartCoroutine(PerformLeap(onComplete));
    }

    /// <summary>
    /// Performs a leap to a target transform with optional callback
    /// </summary>
    /// <param name="target">Target transform to leap to</param>
    /// <param name="onComplete">Callback when leap is completed</param>
    public void LeapToTarget(Transform target, UnityAction onComplete = null)
    {
        if (target == null)
        {
            Debug.LogError("Target transform is null!");
            return;
        }

        LeapToPosition(target.position, onComplete);
    }

    /// <summary>
    /// Cancels the current leap if one is in progress
    /// </summary>
    public void CancelLeap()
    {
        if (currentLeapCoroutine != null)
        {
            StopCoroutine(currentLeapCoroutine);
            currentLeapCoroutine = null;
        }
        
        isLeaping = false;
        
        // Ensure character lands properly
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
    }

    private IEnumerator PerformLeap(UnityAction onComplete = null)
    {
        isLeaping = true;
        OnLeapStarted?.Invoke();

        // Disable physics temporarily for smooth movement
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        // Rotate towards target if enabled
        if (rotateTowardsTarget)
        {
            Vector3 directionToTarget = (targetPosition - originalPosition).normalized;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
                
                // Quick rotation before leap
                float rotationTime = 0.2f;
                float elapsedTime = 0f;
                Quaternion startRotation = transform.rotation;
                
                while (elapsedTime < rotationTime)
                {
                    elapsedTime += Time.deltaTime;
                    float progress = elapsedTime / rotationTime;
                    transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
                    yield return null;
                }
                
                transform.rotation = targetRotation;
            }
        }

        // Play leap animation if available
        if (useLeapAnimation && animationHandler != null && animationHandler.CanPlayLeapAnimations())
        {
            animationHandler.PlayJumpAnimation();
        }

        // Perform the leap with smooth arc movement

        float elapsedLeapTime = 0f;
        Vector3 startPosition = originalPosition;
        bool teleported = false;

        float safeRange = 10000f; // Unity's floating point precision is reliable within this range
        while (elapsedLeapTime < leapDuration)
        {
            elapsedLeapTime += Time.deltaTime;
            float progress = elapsedLeapTime / leapDuration;
            // Apply animation curve for smooth motion
            float curvedProgress = leapCurve.Evaluate(progress);
            // Calculate horizontal movement
            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, curvedProgress);
            // Calculate vertical arc movement
            float heightProgress = Mathf.Sin(progress * Mathf.PI); // Sin wave for natural arc
            Vector3 currentPosition = horizontalPosition + Vector3.up * (heightProgress * leapHeight);

            // Clamp position to safe range
            if (Mathf.Abs(currentPosition.x) > safeRange || Mathf.Abs(currentPosition.y) > safeRange || Mathf.Abs(currentPosition.z) > safeRange)
            {
                Debug.LogWarning($"[LeapMovementController] Attempted to set {gameObject.name} to out-of-bounds position {currentPosition}. Clamping to safe range.");
                currentPosition.x = Mathf.Clamp(currentPosition.x, -safeRange, safeRange);
                currentPosition.y = Mathf.Clamp(currentPosition.y, -safeRange, safeRange);
                currentPosition.z = Mathf.Clamp(currentPosition.z, -safeRange, safeRange);
            }
            transform.position = currentPosition;

            // Safety: teleport if too far or too long
            float distToTarget = Vector3.Distance(transform.position, targetPosition);
            if (elapsedLeapTime > maxLeapTime || distToTarget > maxLeapDistance)
            {
                Debug.LogWarning($"[LeapMovementController] Teleporting {gameObject.name} to target due to timeout or excessive distance.");
                transform.position = targetPosition;
                teleported = true;
                break;
            }
            yield return null;
        }

        // Ensure we land exactly at target
        if (!teleported)
            transform.position = targetPosition;

        // Play landing animation if available
        if (useLeapAnimation && animationHandler != null && animationHandler.CanPlayLeapAnimations())
        {
            animationHandler.PlayLandingAnimation();
        }

        // Restore physics
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }

        isLeaping = false;
        OnLeapLanded?.Invoke();
        OnLeapCompleted?.Invoke();
        onComplete?.Invoke();
        
        currentLeapCoroutine = null;
    }

    /// <summary>
    /// Sets custom leap parameters for this leap
    /// </summary>
    public void SetLeapParameters(float duration, float height, AnimationCurve curve = null)
    {
        leapDuration = duration;
        leapHeight = height;
        if (curve != null)
        {
            leapCurve = curve;
        }
    }

    [Button("Test Leap Forward")]
    private void TestLeapForward()
    {
        if (Application.isPlaying)
        {
            Vector3 testTarget = transform.position + transform.forward * 5f;
            LeapToPosition(testTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isLeaping)
        {
            // Draw leap arc
            Gizmos.color = Color.yellow;
            Vector3 start = originalPosition;
            Vector3 end = targetPosition;
            
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                Vector3 horizontalPos = Vector3.Lerp(start, end, t);
                float heightOffset = Mathf.Sin(t * Mathf.PI) * leapHeight;
                Vector3 arcPos = horizontalPos + Vector3.up * heightOffset;
                
                Gizmos.DrawWireSphere(arcPos, 0.1f);
                
                if (i > 0)
                {
                    float prevT = (i - 1) / 20f;
                    Vector3 prevHorizontalPos = Vector3.Lerp(start, end, prevT);
                    float prevHeightOffset = Mathf.Sin(prevT * Mathf.PI) * leapHeight;
                    Vector3 prevArcPos = prevHorizontalPos + Vector3.up * prevHeightOffset;
                    
                    Gizmos.DrawLine(prevArcPos, arcPos);
                }
            }
        }
    }
}
