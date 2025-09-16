using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Utility component to help set up leap movement on characters.
/// Automatically adds required components and validates the setup.
/// </summary>
public class LeapMovementSetup : MonoBehaviour
{
    [BoxGroup("Setup"), SerializeField] 
    private bool autoSetup = true;
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private float defaultLeapDuration = 1.0f;
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private float defaultLeapHeight = 3.0f;
    
    [BoxGroup("Leap Settings"), SerializeField] 
    private AnimationCurve defaultLeapCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [BoxGroup("References"), SerializeField, ReadOnly] 
    private LeapMovementController leapController;
    
    [BoxGroup("References"), SerializeField, ReadOnly] 
    private UnitAnimationHandler animationHandler;
    
    [BoxGroup("References"), SerializeField, ReadOnly] 
    private CustomRichAI customRichAI;

    private void Start()
    {
        if (autoSetup)
        {
            SetupLeapMovement();
        }
    }

    [Button("Setup Leap Movement")]
    public void SetupLeapMovement()
    {
        // Get or add LeapMovementController
        leapController = GetComponent<LeapMovementController>();
        if (leapController == null)
        {
            leapController = gameObject.AddComponent<LeapMovementController>();
        }

        // Configure leap controller with default settings
        leapController.SetLeapParameters(defaultLeapDuration, defaultLeapHeight, defaultLeapCurve);

        // Find required components
        animationHandler = GetComponentInChildren<UnitAnimationHandler>();
        customRichAI = GetComponent<CustomRichAI>();

        ValidateSetup();
    }

    [Button("Remove Leap Movement")]
    public void RemoveLeapMovement()
    {
        leapController = GetComponent<LeapMovementController>();
        if (leapController != null)
        {
            if (Application.isPlaying)
            {
                Destroy(leapController);
            }
            else
            {
                DestroyImmediate(leapController);
            }
        }
        else
        {
            Debug.LogWarning($"No LeapMovementController found on {gameObject.name}");
        }
    }

    [Button("Test Leap Movement")]
    public void TestLeapMovement()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Leap movement test can only be run in play mode.");
            return;
        }

        if (leapController == null)
        {
            SetupLeapMovement();
        }

        Vector3 testTarget = transform.position + transform.forward * 5f;
        leapController.LeapToPosition(testTarget, () =>
        {
            // Leap movement test completed
        });
    }

    [Button("Validate Setup")]
    public void ValidateSetup()
    {
        bool isValid = true;
        
        // Check for LeapMovementController
        if (GetComponent<LeapMovementController>() == null)
        {
            Debug.LogWarning($"Missing LeapMovementController on {gameObject.name}");
            isValid = false;
        }

        // Check for UnitAnimationHandler
        if (GetComponentInChildren<UnitAnimationHandler>() == null)
        {
            Debug.LogWarning($"Missing UnitAnimationHandler on {gameObject.name} or its children");
            isValid = false;
        }

        // Check for CustomRichAI (optional but recommended)
        if (GetComponent<CustomRichAI>() == null)
        {
            Debug.LogWarning($"Missing CustomRichAI on {gameObject.name} - leap movement integration may not work properly");
        }

        // Check for Rigidbody
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning($"Missing Rigidbody on {gameObject.name} - physics integration may not work properly");
        }

        // Check animation data
        UnitAnimationHandler animHandler = GetComponentInChildren<UnitAnimationHandler>();
        if (animHandler != null && animHandler.AnimationData != null)
        {
            if (!animHandler.AnimationData.CanJump)
            {
                Debug.LogWarning($"EntityAnimationData on {gameObject.name} does not have CanJump enabled");
            }
            else if (animHandler.AnimationData.jumpAnimation == null || animHandler.AnimationData.landingAnimation == null)
            {
                Debug.LogWarning($"EntityAnimationData on {gameObject.name} is missing jump or landing animations");
            }
        }

        if (isValid)
        {
            // Leap movement setup validation passed
        }
    }

    /// <summary>
    /// Get the leap controller on this object, setting it up if necessary
    /// </summary>
    public LeapMovementController GetLeapController()
    {
        if (leapController == null)
        {
            leapController = GetComponent<LeapMovementController>();
        }
        return leapController;
    }
}
