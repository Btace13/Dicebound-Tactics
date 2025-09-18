using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class CurrencyPanel : MonoBehaviour
{
  [Header("Currency Display Settings")]
  [SerializeField] private GameObject currencyDisplayPrefab;
  [SerializeField] private Transform currencyContainer;
  [SerializeField] private List<CurrencyType> displayedCurrencies = new List<CurrencyType>();

  [Header("Animation Settings")]
  [SerializeField] private float changeFlashDuration = 0.8f;
  [SerializeField] private float changeFlashAlpha = 1.0f;
  [SerializeField] private bool flashOnCurrencyChange = true;

  [Header("Auto Setup")]
  [SerializeField] private bool autoSetupAllCurrencies = true;

  private Dictionary<CurrencyType, CurrencyDisplay> currencyDisplays = new Dictionary<CurrencyType, CurrencyDisplay>();
  private CanvasGroup canvasGroup;

  private void Awake()
  {
    // Get or create CanvasGroup component
    canvasGroup = GetComponent<CanvasGroup>();
    if (canvasGroup == null)
    {
      canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    if (autoSetupAllCurrencies)
    {
      displayedCurrencies.Clear();
      foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
      {
        displayedCurrencies.Add(type);
      }
    }

    // Event Listeners
    EventManager.OnCharacterMenuOpened += ShowAllCurrencies;
    EventManager.OnCharacterMenuClosed += HideAllCurrencies;
  }

  private void OnEnable()
  {
    // Subscribe to currency change events
    if (flashOnCurrencyChange)
    {
      CurrencyManager.OnCurrencyChanged += OnCurrencyChanged;
      CurrencyManager.OnCurrencyGained += OnCurrencyGained;
      CurrencyManager.OnCurrencySpent += OnCurrencySpent;
    }
  }

  private void OnDisable()
  {
    // Unsubscribe from events
    EventManager.OnCharacterMenuOpened -= ShowAllCurrencies;
    EventManager.OnCharacterMenuClosed -= HideAllCurrencies;
    
    // Unsubscribe from currency events
    if (flashOnCurrencyChange)
    {
      CurrencyManager.OnCurrencyChanged -= OnCurrencyChanged;
      CurrencyManager.OnCurrencyGained -= OnCurrencyGained;
      CurrencyManager.OnCurrencySpent -= OnCurrencySpent;
    }
  }

  private void Start()
  {
    CreateCurrencyDisplays();
  }

  private void CreateCurrencyDisplays()
  {
    // Clear existing displays
    ClearDisplays();

    if (currencyDisplayPrefab == null || currencyContainer == null)
    {
      Debug.LogError("CurrencyPanel: Missing prefab or container reference!");
      return;
    }

    // Create displays for each currency type
    foreach (var currencyType in displayedCurrencies)
    {
      CreateCurrencyDisplay(currencyType);
    }
  }

  private void CreateCurrencyDisplay(CurrencyType type)
  {
    GameObject displayObject = Instantiate(currencyDisplayPrefab, currencyContainer);
    CurrencyDisplay display = displayObject.GetComponent<CurrencyDisplay>();

    if (display == null)
    {
      Debug.LogError($"CurrencyPanel: Prefab missing CurrencyDisplay component!");
      Destroy(displayObject);
      return;
    }

    display.SetCurrencyType(type);
    currencyDisplays[type] = display;

    // Set the name for easier identification
    displayObject.name = $"CurrencyDisplay_{type}";
  }

  private void ClearDisplays()
  {
    foreach (var display in currencyDisplays.Values)
    {
      if (display != null)
      {
        DestroyImmediate(display.gameObject);
      }
    }
    currencyDisplays.Clear();
  }

  public void ShowCurrency(CurrencyType type, bool animated = true)
  {
    if (currencyDisplays.TryGetValue(type, out CurrencyDisplay display))
    {
      display.SetVisibility(true, animated);
    }
  }

  public void HideCurrency(CurrencyType type, bool animated = true)
  {
    if (currencyDisplays.TryGetValue(type, out CurrencyDisplay display))
    {
      display.SetVisibility(false, animated);
    }
  }

  public void ShowAllCurrencies(bool animated = true)
  {
    foreach (var display in currencyDisplays.Values)
    {
      display.SetVisibility(true, animated);
    }
  }

  public void HideAllCurrencies(bool animated = true)
  {
    foreach (var display in currencyDisplays.Values)
    {
      display.SetVisibility(false, animated);
    }
  }

  public void AddCurrencyType(CurrencyType type)
  {
    if (!displayedCurrencies.Contains(type))
    {
      displayedCurrencies.Add(type);
      CreateCurrencyDisplay(type);
    }
  }

  public void RemoveCurrencyType(CurrencyType type)
  {
    if (displayedCurrencies.Contains(type))
    {
      displayedCurrencies.Remove(type);

      if (currencyDisplays.TryGetValue(type, out CurrencyDisplay display))
      {
        currencyDisplays.Remove(type);
        DestroyImmediate(display.gameObject);
      }
    }
  }

  // Called from editor or for runtime setup
  public void RefreshDisplays()
  {
    CreateCurrencyDisplays();
  }

  private void OnValidate()
  {
    // Remove duplicates in the editor
    if (displayedCurrencies != null)
    {
      displayedCurrencies = displayedCurrencies.Distinct().ToList();
    }
  }

  // Currency change event handlers
  private void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
  {
    if (displayedCurrencies.Contains(type))
    {
      FlashPanel();
    }
  }

  private void OnCurrencyGained(CurrencyType type, int amount)
  {
    if (displayedCurrencies.Contains(type))
    {
      FlashPanel();
    }
  }

  private void OnCurrencySpent(CurrencyType type, int amount)
  {
    if (displayedCurrencies.Contains(type))
    {
      FlashPanel();
    }
  }

  private void FlashPanel()
  {
    if (canvasGroup == null) return;

    // Kill any existing flash animations
    canvasGroup.DOKill();

    // If panel is currently hidden, make it visible temporarily
    bool wasHidden = canvasGroup.alpha < 0.1f;
    
    if (wasHidden)
    {
      // Quick fade in, flash, then fade back out
      canvasGroup.alpha = 0f;
      canvasGroup.DOFade(changeFlashAlpha, changeFlashDuration * 0.2f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
          // Hold visible for a moment
          canvasGroup.DOFade(changeFlashAlpha, changeFlashDuration * 0.4f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
              // Fade back out
              canvasGroup.DOFade(0f, changeFlashDuration * 0.4f)
                .SetEase(Ease.InQuad);
            });
        });
    }
    else
    {
      // Panel is already visible, just do a subtle flash
      float originalAlpha = canvasGroup.alpha;
      canvasGroup.DOFade(changeFlashAlpha, changeFlashDuration * 0.3f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
          canvasGroup.DOFade(originalAlpha, changeFlashDuration * 0.7f)
            .SetEase(Ease.InQuad);
        });
    }
  }

  private void ShowAllCurrencies()
  {
    // fade canvas in
    if (canvasGroup != null)
    {
      canvasGroup.DOKill();
      canvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);
      canvasGroup.interactable = true;
      canvasGroup.blocksRaycasts = true;
    }
  }

  private void HideAllCurrencies()
  {
    // fade canvas out
    if (canvasGroup != null)
    {
      canvasGroup.DOKill();
      canvasGroup.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }
  }
}