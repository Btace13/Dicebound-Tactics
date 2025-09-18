using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CurrencyDisplay : MonoBehaviour
{
    [Header("Currency Settings")]
    [SerializeField] private CurrencyType currencyType;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Image currencyIcon;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float pulseScale = 1.2f;
    [SerializeField] private Color gainColor = Color.green;
    [SerializeField] private Color spendColor = Color.red;

    private Color originalTextColor;
    private Vector3 originalScale;

    private void Awake()
    {
        if (currencyText != null)
        {
            originalTextColor = currencyText.color;
        }
        originalScale = transform.localScale;
        
        // Auto-setup icon and styling from configuration
        SetupCurrencyVisuals();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
        {
            UpdateDisplay();
        }

        CurrencyManager.OnCurrencyChanged += OnCurrencyChanged;
        CurrencyManager.OnCurrencyGained += OnCurrencyGained;
        CurrencyManager.OnCurrencySpent += OnCurrencySpent;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCurrencyChanged -= OnCurrencyChanged;
        CurrencyManager.OnCurrencyGained -= OnCurrencyGained;
        CurrencyManager.OnCurrencySpent -= OnCurrencySpent;
    }

    private void Start()
    {
        UpdateDisplay();
    }

    private void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        if (type == currencyType)
        {
            UpdateDisplay();
        }
    }

    private void OnCurrencyGained(CurrencyType type, int amount)
    {
        if (type == currencyType)
        {
            AnimateGain();
        }
    }

    private void OnCurrencySpent(CurrencyType type, int amount)
    {
        if (type == currencyType)
        {
            AnimateSpend();
        }
    }

    private void UpdateDisplay()
    {
        if (CurrencyManager.Instance == null || currencyText == null) return;

        int amount = CurrencyManager.Instance.GetCurrency(currencyType);
        currencyText.text = FormatCurrencyAmount(amount);
    }

    private string FormatCurrencyAmount(int amount)
    {
        // Format large numbers with K, M suffixes
        if (amount >= 1000000)
        {
            return $"{amount / 1000000f:F1}M";
        }
        else if (amount >= 1000)
        {
            return $"{amount / 1000f:F1}K";
        }
        else
        {
            return amount.ToString();
        }
    }

    private void AnimateGain()
    {
        AnimateCurrencyChange(gainColor);
    }

    private void AnimateSpend()
    {
        AnimateCurrencyChange(spendColor);
    }

    private void AnimateCurrencyChange(Color flashColor)
    {
        // Kill any existing animations
        transform.DOKill();
        if (currencyText != null)
        {
            currencyText.DOKill();
        }

        // Scale pulse animation
        transform.DOScale(originalScale * pulseScale, animationDuration * 0.5f)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                transform.DOScale(originalScale, animationDuration * 0.5f)
                    .SetEase(Ease.InQuart);
            });

        // Color flash animation
        if (currencyText != null)
        {
            currencyText.DOColor(flashColor, animationDuration * 0.3f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    currencyText.DOColor(originalTextColor, animationDuration * 0.7f)
                        .SetEase(Ease.InQuart);
                });
        }
    }

    public void SetCurrencyType(CurrencyType type)
    {
        currencyType = type;
        SetupCurrencyVisuals();
        UpdateDisplay();
    }

    public void SetVisibility(bool visible, bool animated = true)
    {
        if (canvasGroup == null) return;

        if (animated)
        {
            canvasGroup.DOFade(visible ? 1f : 0f, animationDuration)
                .SetEase(Ease.InOutQuart);
        }
        else
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private void SetupCurrencyVisuals()
    {
        if (CurrencyConfiguration.Instance == null) return;

        // Set icon from configuration
        if (currencyIcon != null)
        {
            var icon = CurrencyConfiguration.Instance.GetIcon(currencyType);
            if (icon != null)
            {
                currencyIcon.sprite = icon;
                currencyIcon.color = CurrencyConfiguration.Instance.GetColor(currencyType);
            }
        }

        // Set text color from configuration (optional)
        if (currencyText != null)
        {
            var configColor = CurrencyConfiguration.Instance.GetColor(currencyType);
            currencyText.color = configColor;
            originalTextColor = configColor; // Update the original color for animations
        }
    }
}