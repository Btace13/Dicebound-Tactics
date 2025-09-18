using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class CurrencyPickup : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private CurrencyType currencyType = CurrencyType.Gold;
    [SerializeField] private int amount = 10;
    [SerializeField] private bool randomAmount = false;
    
    [Header("Random Amount Settings")]
    [SerializeField, ShowIf("randomAmount")] private int minAmount = 5;
    [SerializeField, ShowIf("randomAmount")] private int maxAmount = 15;

    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float pickupDelay = 0.1f;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Animation Settings")]
    [SerializeField] private bool animateFloat = true;
    [SerializeField, ShowIf("animateFloat")] private float floatHeight = 0.5f;
    [SerializeField, ShowIf("animateFloat")] private float floatDuration = 2f;
    [SerializeField] private bool animateRotate = true;
    [SerializeField, ShowIf("animateRotate")] private float rotationSpeed = 90f;

    [Header("Pickup Animation")]
    [SerializeField] private float pickupScaleMultiplier = 1.5f;
    [SerializeField] private float pickupAnimationDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float volume = 1f;

    [Header("VFX")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private ParticleSystem pickupParticles;

    private Vector3 startPosition;
    private bool hasBeenPickedUp = false;
    private Collider pickupCollider;
    private Renderer[] renderers;

    // Events
    public static System.Action<CurrencyType, int> OnCurrencyPickedUp;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
        
        // Ensure collider is set as trigger
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        startPosition = transform.position;
        
        // Set random amount if enabled
        if (randomAmount)
        {
            amount = Random.Range(minAmount, maxAmount + 1);
        }

        StartAnimations();
    }

    private void StartAnimations()
    {
        // Floating animation
        if (animateFloat)
        {
            transform.DOMoveY(startPosition.y + floatHeight, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        // Rotation animation
        if (animateRotate)
        {
            transform.DORotate(new Vector3(0, 360, 0), 360f / rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPickedUp) return;

        if (other.CompareTag(playerTag))
        {
            PickupCurrency();
        }
    }

    [Button("Test Pickup")]
    private void PickupCurrency()
    {
        if (hasBeenPickedUp) return;

        hasBeenPickedUp = true;

        // Add currency to manager
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(currencyType, amount);
        }

        // Trigger events
        OnCurrencyPickedUp?.Invoke(currencyType, amount);

        // Play audio
        PlayPickupAudio();

        // Spawn VFX
        SpawnPickupEffects();

        // Animate pickup
        AnimatePickup();
    }

    private void PlayPickupAudio()
    {
        if (pickupSound != null)
        {
            // Create a temporary audio source for one-shot audio
            GameObject audioObject = new GameObject("PickupAudio");
            AudioSource audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.clip = pickupSound;
            audioSource.volume = volume;
            audioSource.Play();

            // Destroy the audio object after the clip finishes
            Destroy(audioObject, pickupSound.length);
        }
    }

    private void SpawnPickupEffects()
    {
        // Instantiate pickup effect
        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);
            
            // Auto-destroy effect after a few seconds
            Destroy(effect, 5f);
        }

        // Play particle system
        if (pickupParticles != null)
        {
            pickupParticles.Play();
        }
    }

    private void AnimatePickup()
    {
        // Disable collider to prevent multiple pickups
        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        // Stop all existing animations
        transform.DOKill();

        // Scale up then down
        transform.DOScale(transform.localScale * pickupScaleMultiplier, pickupAnimationDuration * 0.3f)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, pickupAnimationDuration * 0.7f)
                    .SetEase(Ease.InQuart)
                    .OnComplete(() =>
                    {
                        if (destroyOnPickup)
                        {
                            Destroy(gameObject);
                        }
                        else
                        {
                            gameObject.SetActive(false);
                        }
                    });
            });

        // Fade out renderers
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color originalColor = material.color;
                    material.DOColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0), pickupAnimationDuration)
                        .SetEase(Ease.InQuart);
                }
            }
        }
    }

    public void SetCurrencyAmount(CurrencyType type, int newAmount)
    {
        currencyType = type;
        amount = newAmount;
    }

    public void SetRandomAmountRange(int min, int max)
    {
        randomAmount = true;
        minAmount = min;
        maxAmount = max;
        
        if (Application.isPlaying)
        {
            amount = Random.Range(minAmount, maxAmount + 1);
        }
    }

    // For spawning systems
    public static GameObject CreateCurrencyPickup(Vector3 position, CurrencyType type, int amount, Transform parent = null)
    {
        // This would need a prefab reference, but for now we'll create a basic one
        GameObject pickupObject = new GameObject($"CurrencyPickup_{type}");
        pickupObject.transform.position = position;
        
        if (parent != null)
        {
            pickupObject.transform.SetParent(parent);
        }

        // Add required components
        var pickup = pickupObject.AddComponent<CurrencyPickup>();
        var collider = pickupObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f;

        // Set currency properties
        pickup.SetCurrencyAmount(type, amount);

        return pickupObject;
    }

    private void OnDestroy()
    {
        // Clean up any DOTween animations
        transform.DOKill();
    }
}