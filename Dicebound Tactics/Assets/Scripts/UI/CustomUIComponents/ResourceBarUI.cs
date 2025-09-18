using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;
using System.Collections;

[System.Serializable]
public class ResourceBarSettings
{
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public Ease animationEase = Ease.OutCubic;
    public bool enableScaleEffect = true;
    public float scaleEffectStrength = 1.05f;
    public float scaleEffectDuration = 0.2f;
    
    [Header("Auto-Hide Settings")]
    public bool autoHide = true;
    public float displayDuration = 3f;
    
    [Header("Color Settings")]
    public bool enableColorFlash = true;
    public Color flashColor = new Color(0.2f, 1f, 0.2f, 1f);
    public float flashDuration = 0.2f;
}

public enum ResourceType
{
    Health,
    Experience
}

public class ResourceBarUI : MonoBehaviour
{
    [BoxGroup("Configuration")]
    [SerializeField] private ResourceType resourceType = ResourceType.Health;
    [BoxGroup("Configuration")]
    [SerializeField] private ResourceBarSettings advancedSettings = new ResourceBarSettings();
    
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI resourceAmountText;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI resourceNameText;
    [BoxGroup("References"), SerializeField] private Slider slider;
    [BoxGroup("Settings"), SerializeField] private string resourceName = "HP";
    [BoxGroup("Settings"), SerializeField] private int maxResource = 100;
    [BoxGroup("Settings"), SerializeField] private int currentResource = 100;
    [BoxGroup("Settings"), SerializeField, ColorUsage(false)] private Color barColor = Color.green;
    [BoxGroup("Settings"), SerializeField] private bool addColonToName = true;

    // Internal state for advanced features
    private int previousValue = -1;
    private Coroutine hideCoroutine;
    private Color originalTextColor;
    private bool isInitialized = false;
    private Image fillImage;

    public int MaxResource
    {
        get => maxResource;
        set
        {
            maxResource = Mathf.Max(0, value);
            UpdateBarInternal();
        }
    }

    public int CurrentResource
    {
        get => currentResource;
        set
        {
            currentResource = Mathf.Clamp(value, 0, maxResource);
            UpdateBarInternal();
        }
    }

    public string ResourceName
    {
        get => resourceName;
        set
        {
            resourceName = value;
            if (resourceNameText != null)
                resourceNameText.text = value + (addColonToName ? ":" : "");
        }
    }

    private void Awake()
    {
        // Store original text color for flash effects
        if (resourceAmountText != null)
        {
            originalTextColor = resourceAmountText.color;
        }
        
        // Experience bars always start hidden
        if (resourceType == ResourceType.Experience)
        {
            SetVisibility(false);
        }
    }

    public void SetResource(int resource)
    {
        currentResource = Mathf.Clamp(resource, 0, maxResource);
        UpdateBarInternal();
    }
    
    /// <summary>
    /// Advanced update method that supports animations and conditional visibility
    /// </summary>
    /// <param name="currentValue">Current resource value</param>
    /// <param name="maxValue">Maximum resource value</param>
    /// <param name="forceUpdate">Force update even if value hasn't changed</param>
    public void UpdateBar(int currentValue, int maxValue, bool forceUpdate = false)
    {
        bool valueChanged = !isInitialized || previousValue != currentValue || forceUpdate;
        
        // For experience bars with auto-hide, only show if value actually changed
        if (resourceType == ResourceType.Experience && advancedSettings.autoHide && !valueChanged && isInitialized)
        {
            return;
        }
        
        // Show the bar if it should be visible
        if (valueChanged && advancedSettings.autoHide)
        {
            SetVisibility(true);
            
            // Stop any existing hide timer
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
        }
        
        // Update internal values
        maxResource = maxValue;
        currentResource = currentValue;
        
        // Update slider with animation
        if (slider != null)
        {
            UpdateSliderAnimated(currentValue, maxValue, valueChanged);
        }
        
        // Update text with animation
        if (resourceAmountText != null)
        {
            UpdateTextAnimated(currentValue, maxValue, valueChanged);
        }
        
        // Store previous value for change detection
        previousValue = currentValue;
        isInitialized = true;
        
        // Start auto-hide timer if configured and value changed
        if (advancedSettings.autoHide && valueChanged)
        {
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private void UpdateSliderAnimated(int currentValue, int maxValue, bool valueChanged)
    {
        if (maxValue <= 0) return;
        
        // Set the slider's max value to match the resource max value
        slider.maxValue = maxValue;
        
        // Animate slider value directly to the current value
        slider.DOValue(currentValue, advancedSettings.animationDuration).SetEase(advancedSettings.animationEase);
        
        // Update fill color
        if (fillImage == null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }
        if (fillImage != null)
        {
            fillImage.color = barColor;
        }
        
        // Add scale effect if value changed and enabled
        if (valueChanged && advancedSettings.enableScaleEffect)
        {
            Transform sliderTransform = slider.transform;
            sliderTransform.DOKill(); // Stop any existing animations
            sliderTransform.DOScale(advancedSettings.scaleEffectStrength, advancedSettings.scaleEffectDuration)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    sliderTransform.DOScale(1f, advancedSettings.scaleEffectDuration * 1.5f).SetEase(Ease.InQuart);
                });
        }
    }
    
    private void UpdateTextAnimated(int currentValue, int maxValue, bool valueChanged)
    {
        // Get current value from text for smooth number animation
        int currentTextValue = previousValue >= 0 ? previousValue : 0;
        
        // Animate the number change
        DOTween.To(() => currentTextValue, x =>
        {
            currentTextValue = x;
            resourceAmountText.text = $"{currentTextValue}/{maxValue}";
        }, currentValue, advancedSettings.animationDuration).SetEase(advancedSettings.animationEase);
        
        // Add color flash effect if value increased and enabled
        if (valueChanged && advancedSettings.enableColorFlash && currentValue > previousValue)
        {
            resourceAmountText.DOKill(); // Stop any existing color animations
            resourceAmountText.DOColor(advancedSettings.flashColor, advancedSettings.flashDuration)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    resourceAmountText.DOColor(originalTextColor, advancedSettings.flashDuration * 2.5f).SetEase(Ease.InQuart);
                });
        }
    }

    private void UpdateBarInternal()
    {
        if (slider != null)
        {
            slider.maxValue = maxResource;
            slider.value = currentResource;
            if (fillImage == null)
            {
                fillImage = slider.fillRect.GetComponent<Image>();
            }
            if (fillImage != null)
            {
                fillImage.color = barColor;
            }
            if (resourceAmountText != null)
            {
                resourceAmountText.text = $"{currentResource}/{maxResource}";
            }
        }
    }
    
    /// <summary>
    /// Sets the visibility of the resource bar
    /// </summary>
    public void SetVisibility(bool visible)
    {
        if (slider != null)
            slider.gameObject.SetActive(visible);
        if (resourceAmountText != null)
            resourceAmountText.gameObject.SetActive(visible);
        if (resourceNameText != null)
            resourceNameText.gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Resets the resource bar state
    /// </summary>
    public void Reset()
    {
        previousValue = -1;
        isInitialized = false;
        
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
        
        if (slider != null)
        {
            slider.value = 0f;
        }
        
        if (resourceAmountText != null)
        {
            resourceAmountText.text = string.Empty;
        }
        
        // Experience bars always start hidden
        if (resourceType == ResourceType.Experience)
        {
            SetVisibility(false);
        }
    }
    
    /// <summary>
    /// Coroutine to hide the bar after a delay
    /// </summary>
    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(advancedSettings.displayDuration);
        SetVisibility(false);
        hideCoroutine = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure the resource name is set
        if (resourceNameText != null)
        {
            resourceNameText.text = resourceName + (addColonToName ? ":" : "");
        }

        // Update the resource amount
        UpdateBarInternal();
    }
#endif
    
    private void OnDestroy()
    {
        // Clean up any running coroutines
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        
        // Kill any DOTween animations
        if (slider != null)
        {
            slider.DOKill();
        }
        if (resourceAmountText != null)
        {
            resourceAmountText.DOKill();
        }
    }
}
