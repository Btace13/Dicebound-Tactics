using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Currency pickup component that handles collection, animation, and magnetic attraction to players.
/// Features:
/// - Magnetic pickup: Currencies move towards nearby players within a configurable range
/// - Smooth animations with floating, rotation, and pickup effects
/// - Audio and visual feedback systems
/// - Configurable random amounts and custom prefab support
/// - Performance optimized player detection with interval-based checks
/// </summary>
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

    [Header("Magnetic Pickup Settings")]
    [SerializeField] private bool enableMagneticPickup = true;
    [SerializeField] private float magneticRange = 3f;
    [SerializeField] private float magneticSpeed = 8f;
    [SerializeField] private AnimationCurve magneticCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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
    
    // Magnetic pickup variables
    private Transform nearestPlayer;
    private bool isBeingMagnetized = false;
    private Vector3 magneticStartPosition;
    private float lastPlayerCheckTime = 0f;
    private const float playerCheckInterval = 0.1f; // Check for players every 0.1 seconds

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

    private void Update()
    {
        if (hasBeenPickedUp || !enableMagneticPickup) return;

        // Only check for players at intervals to improve performance
        if (Time.time - lastPlayerCheckTime >= playerCheckInterval)
        {
            CheckForNearbyPlayers();
            lastPlayerCheckTime = Time.time;
        }
        
        if (isBeingMagnetized && nearestPlayer != null)
        {
            MoveMagneticallyToPlayer();
        }
    }

    private void CheckForNearbyPlayers()
    {
        // Find all player objects (optimized to avoid constant GameObject.FindGameObjectsWithTag calls)
        GameObject[] players = GameObject.FindGameObjectsWithTag(playerTag);
        
        if (players.Length == 0) return;

        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        // Find the closest player within magnetic range
        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= magneticRange && distance < closestDistance)
            {
                closestPlayer = player.transform;
                closestDistance = distance;
            }
        }

        // Start magnetizing if we found a close player and aren't already magnetizing
        if (closestPlayer != null && !isBeingMagnetized)
        {
            StartMagneticMovement(closestPlayer);
        }
        // Stop magnetizing if no close players or player moved away
        else if (closestPlayer == null && isBeingMagnetized)
        {
            StopMagneticMovement();
        }
        // Update target if a different closer player is found
        else if (closestPlayer != null && closestPlayer != nearestPlayer)
        {
            nearestPlayer = closestPlayer;
        }
    }

    private void StartMagneticMovement(Transform player)
    {
        nearestPlayer = player;
        isBeingMagnetized = true;
        magneticStartPosition = transform.position;
        
        // Stop floating animation to avoid conflicts
        transform.DOKill();
        
        // Start a subtle pulsing effect to indicate magnetization
        transform.DOScale(transform.localScale * 1.1f, 0.2f)
            .SetEase(Ease.OutQuart)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopMagneticMovement()
    {
        isBeingMagnetized = false;
        nearestPlayer = null;
        
        // Stop magnetization effects and restart normal animations
        transform.DOKill();
        transform.localScale = Vector3.one; // Reset scale
        StartAnimations(); // Restart floating/rotation animations
    }

    private void MoveMagneticallyToPlayer()
    {
        if (nearestPlayer == null) return;

        // Calculate direction and distance to player
        Vector3 directionToPlayer = (nearestPlayer.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, nearestPlayer.position);
        
        // If very close to player, trigger pickup instead of continuing movement
        if (distanceToPlayer <= 0.5f)
        {
            PickupCurrency();
            return;
        }
        
        // Use curve to modify speed based on distance for smooth approach
        float normalizedDistance = Mathf.Clamp01(distanceToPlayer / magneticRange);
        float curveMultiplier = magneticCurve.Evaluate(1f - normalizedDistance);
        
        // Calculate move vector with distance check to prevent overshooting
        float moveDistance = magneticSpeed * curveMultiplier * Time.deltaTime;
        if (moveDistance > distanceToPlayer)
        {
            moveDistance = distanceToPlayer;
        }
        
        Vector3 moveVector = directionToPlayer * moveDistance;
        transform.position += moveVector;
        
        // Add slight bobbing motion while being magnetized for visual appeal
        float bobOffset = Mathf.Sin(Time.time * 6f) * 0.05f;
        transform.position = new Vector3(transform.position.x, transform.position.y + bobOffset * Time.deltaTime, transform.position.z);
    }

    private void OnDrawGizmosSelected()
    {
        if (!enableMagneticPickup) return;
        
        // Draw magnetic range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, magneticRange);
        
        // Draw line to nearest player if magnetizing
        if (isBeingMagnetized && nearestPlayer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, nearestPlayer.position);
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
            // Stop magnetic movement when player enters trigger
            if (isBeingMagnetized)
            {
                StopMagneticMovement();
            }
            
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
            Debug.Log($"[CurrencyPickup] Adding {amount} {currencyType} to manager");
            CurrencyManager.Instance.AddCurrency(currencyType, amount);
        }
        else
        {
            Debug.LogError("[CurrencyPickup] CurrencyManager.Instance is null!");
        }

        // Trigger events
        OnCurrencyPickedUp?.Invoke(currencyType, amount);
        Debug.Log($"[CurrencyPickup] Triggered OnCurrencyPickedUp event for {amount} {currencyType}");

        // Play audio
        PlayPickupAudio();

        // Spawn VFX
        SpawnPickupEffects();

        // Animate pickup
        AnimatePickup();
    }

    [Button("Test Magnetic Movement"), ShowIf("enableMagneticPickup")]
    private void TestMagneticMovement()
    {
        if (Application.isPlaying)
        {
            CheckForNearbyPlayers();
        }
        else
        {
            Debug.LogWarning("Magnetic movement can only be tested during play mode.");
        }
    }

    [Button("Toggle Magnetic Pickup")]
    private void ToggleMagneticPickup()
    {
        enableMagneticPickup = !enableMagneticPickup;
        
        if (!enableMagneticPickup && isBeingMagnetized)
        {
            StopMagneticMovement();
        }
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
        GameObject pickupObject = null;
        
        // Try to use prefab from configuration first
        if (CurrencyConfiguration.Instance != null)
        {
            var prefab = CurrencyConfiguration.Instance.GetPickupPrefab(type);
            if (prefab != null)
            {
                pickupObject = Instantiate(prefab, position, Quaternion.identity, parent);
                
                // Configure the pickup component if it exists
                var pickup = pickupObject.GetComponent<CurrencyPickup>();
                if (pickup != null)
                {
                    pickup.SetCurrencyAmount(type, amount);
                }
                else
                {
                    Debug.LogWarning($"Prefab for {type} doesn't have a CurrencyPickup component!");
                }
                
                return pickupObject;
            }
        }
        
        // Fallback: Create a basic pickup if no prefab is configured
        Debug.LogWarning($"No prefab configured for {type}, creating basic pickup");
        pickupObject = new GameObject($"CurrencyPickup_{type}");
        pickupObject.transform.position = position;
        
        if (parent != null)
        {
            pickupObject.transform.SetParent(parent);
        }

        // Add required components
        var basicPickup = pickupObject.AddComponent<CurrencyPickup>();
        var collider = pickupObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f;

        // Set currency properties
        basicPickup.SetCurrencyAmount(type, amount);

        return pickupObject;
    }

    private void OnDestroy()
    {
        // Clean up any DOTween animations
        transform.DOKill();
    }
}