using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Runtime debug tool for troubleshooting currency system events and UI behavior.
/// Focuses on real-time event monitoring and UI panel debugging.
/// Place this on any GameObject in the scene to debug currency events during gameplay.
/// 
/// Use this when:
/// - Currency pickups aren't working
/// - UI panels aren't showing/updating
/// - Events aren't firing correctly
/// - Debugging in builds (where CurrencySystemWindow isn't available)
/// </summary>
public class CurrencyDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logAllCurrencyEvents = true;
    [SerializeField] private bool logUIEvents = true;
    
    [Header("Test Values - Use CurrencySystemWindow for advanced testing")]
    [SerializeField] private CurrencyType testCurrencyType = CurrencyType.Gold;
    [SerializeField] private int testAmount = 50;

    private void OnEnable()
    {
        if (logAllCurrencyEvents)
        {
            CurrencyManager.OnCurrencyChanged += OnCurrencyChanged;
            CurrencyManager.OnCurrencyGained += OnCurrencyGained;
            CurrencyManager.OnCurrencySpent += OnCurrencySpent;
        }
        
        if (logUIEvents)
        {
            CurrencyPickup.OnCurrencyPickedUp += OnCurrencyPickedUp;
        }
    }

    private void OnDisable()
    {
        if (logAllCurrencyEvents)
        {
            CurrencyManager.OnCurrencyChanged -= OnCurrencyChanged;
            CurrencyManager.OnCurrencyGained -= OnCurrencyGained;
            CurrencyManager.OnCurrencySpent -= OnCurrencySpent;
        }
        
        if (logUIEvents)
        {
            CurrencyPickup.OnCurrencyPickedUp -= OnCurrencyPickedUp;
        }
    }

    [Button("Quick Test: Add Currency")]
    private void TestAddCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            Debug.Log($"[CurrencyDebugger] Manually adding {testAmount} {testCurrencyType}");
            CurrencyManager.Instance.AddCurrency(testCurrencyType, testAmount);
        }
        else
        {
            Debug.LogError("[CurrencyDebugger] CurrencyManager.Instance is null!");
        }
    }

    [Button("Quick Test: Spend Currency")]
    private void TestSpendCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            Debug.Log($"[CurrencyDebugger] Manually spending {testAmount} {testCurrencyType}");
            bool success = CurrencyManager.Instance.SpendCurrency(testCurrencyType, testAmount);
            Debug.Log($"[CurrencyDebugger] Spend result: {success}");
        }
        else
        {
            Debug.LogError("[CurrencyDebugger] CurrencyManager.Instance is null!");
        }
    }

    [Button("Find Currency Panels")]
    private void FindCurrencyPanels()
    {
        CurrencyPanel[] panels = FindObjectsByType<CurrencyPanel>(FindObjectsSortMode.None);
        Debug.Log($"[CurrencyDebugger] Found {panels.Length} CurrencyPanel(s) in scene:");
        
        for (int i = 0; i < panels.Length; i++)
        {
            Debug.Log($"[CurrencyDebugger] Panel {i}: {panels[i].name} - Active: {panels[i].gameObject.activeInHierarchy}");
            
            // Check if it has a CanvasGroup
            CanvasGroup canvasGroup = panels[i].GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"[CurrencyDebugger] - CanvasGroup Alpha: {canvasGroup.alpha}");
            }
            else
            {
                Debug.Log($"[CurrencyDebugger] - No CanvasGroup found!");
            }
        }
    }

    [Button("Find Currency Displays")]
    private void FindCurrencyDisplays()
    {
        CurrencyDisplay[] displays = FindObjectsByType<CurrencyDisplay>(FindObjectsSortMode.None);
        Debug.Log($"[CurrencyDebugger] Found {displays.Length} CurrencyDisplay(s) in scene:");
        
        for (int i = 0; i < displays.Length; i++)
        {
            Debug.Log($"[CurrencyDebugger] Display {i}: {displays[i].name} - Active: {displays[i].gameObject.activeInHierarchy}");
        }
    }

    [Button("Test Currency Pickup Creation")]
    private void TestCreatePickup()
    {
        Vector3 spawnPos = transform.position + Vector3.up * 2f;
        GameObject pickup = CurrencyPickup.CreateCurrencyPickup(spawnPos, testCurrencyType, testAmount);
        if (pickup != null)
        {
            Debug.Log($"[CurrencyDebugger] Created test pickup at {spawnPos} - Use CurrencySystemWindow for advanced pickup creation");
        }
        else
        {
            Debug.LogError("[CurrencyDebugger] Failed to create test pickup!");
        }
    }

    // Event handlers for logging
    private void OnCurrencyChanged(CurrencyType type, int oldAmount, int newAmount)
    {
        Debug.Log($"[CurrencyDebugger] Currency Changed: {type} from {oldAmount} to {newAmount} (change: {newAmount - oldAmount})");
    }

    private void OnCurrencyGained(CurrencyType type, int amount)
    {
        Debug.Log($"[CurrencyDebugger] Currency Gained: +{amount} {type}");
    }

    private void OnCurrencySpent(CurrencyType type, int amount)
    {
        Debug.Log($"[CurrencyDebugger] Currency Spent: -{amount} {type}");
    }

    private void OnCurrencyPickedUp(CurrencyType type, int amount)
    {
        Debug.Log($"[CurrencyDebugger] Currency Picked Up: +{amount} {type}");
    }
}