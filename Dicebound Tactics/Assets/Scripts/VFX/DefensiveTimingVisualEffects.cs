using UnityEngine;
using DG.Tweening;

public class DefensiveTimingVisualEffects : MonoBehaviour
{
    [Header("Screen Effects")]
    [SerializeField] private CanvasGroup screenFlashOverlay;
    [SerializeField] private Color successFlashColor = Color.green;
    [SerializeField] private Color failureFlashColor = Color.red;
    [SerializeField] private Color blockFlashColor = Color.blue;
    
    [Header("Camera Effects")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 0.3f;
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private ParticleSystem failureParticles;
    [SerializeField] private ParticleSystem blockParticles;
    
    private void OnEnable()
    {
        // Subscribe to events
        EventManager.OnDefensiveSequenceCompleted += OnSequenceCompleted;
        EventManager.OnDefensiveSequenceFailed += OnSequenceFailed;
        EventManager.OnAttackBlocked += OnAttackBlocked;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        EventManager.OnDefensiveSequenceCompleted -= OnSequenceCompleted;
        EventManager.OnDefensiveSequenceFailed -= OnSequenceFailed;
        EventManager.OnAttackBlocked -= OnAttackBlocked;
    }
    
    private void OnSequenceCompleted()
    {
        // Screen flash effect
        FlashScreen(successFlashColor);
        
        // Play particles
        if (successParticles != null)
        {
            successParticles.Play();
        }
        
        Debug.Log("Visual Effect: Defensive sequence completed!");
    }
    
    private void OnSequenceFailed()
    {
        // Screen flash effect
        FlashScreen(failureFlashColor);
        
        // Camera shake
        ShakeCamera();
        
        // Play particles
        if (failureParticles != null)
        {
            failureParticles.Play();
        }
        
        Debug.Log("Visual Effect: Defensive sequence failed!");
    }
    
    private void OnAttackBlocked()
    {
        // Strong screen flash for successful block
        FlashScreen(blockFlashColor);
        
        // Play block particles
        if (blockParticles != null)
        {
            blockParticles.Play();
        }
        
        Debug.Log("Visual Effect: Attack blocked!");
    }
    
    private void FlashScreen(Color flashColor)
    {
        if (screenFlashOverlay != null)
        {
            var image = screenFlashOverlay.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = flashColor;
            }
            
            screenFlashOverlay.alpha = 0.5f;
            screenFlashOverlay.DOFade(0f, 0.3f);
        }
    }
    
    private void ShakeCamera()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.DOShakePosition(shakeDuration, shakeIntensity);
        }
    }
}
