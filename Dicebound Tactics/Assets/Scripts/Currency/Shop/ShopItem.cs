using UnityEngine;
using System;

[Serializable]
public class ShopItem : IPurchasable
{
    [Header("Item Information")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    
    [Header("Cost")]
    [SerializeField] private CurrencyCost cost;
    
    [Header("Availability")]
    [SerializeField] private bool isAvailable = true;
    [SerializeField] private int stockQuantity = -1; // -1 for unlimited
    [SerializeField] private bool oneTimePurchase = false;

    [Header("Conditions")]
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private string[] restrictedFlags;

    private bool hasPurchased = false;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public CurrencyCost Cost => cost;
    public bool IsAvailable => isAvailable && CheckStockAvailability() && CheckFlagConditions();
    public int StockQuantity => stockQuantity;
    public bool HasBeenPurchased => hasPurchased;

    // Events
    public event Action<ShopItem> OnPurchased;
    public event Action<ShopItem> OnPurchaseFailed;

    public bool CanPurchase()
    {
        if (!IsAvailable) return false;
        if (oneTimePurchase && hasPurchased) return false;
        
        return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
    }

    public void Purchase()
    {
        if (!CanPurchase())
        {
            Debug.LogWarning($"Cannot purchase {itemName}");
            OnPurchaseFailed?.Invoke(this);
            return;
        }

        if (CurrencyManager.Instance.SpendCurrency(cost))
        {
            ExecutePurchase();
            OnPurchased?.Invoke(this);
            Debug.Log($"Successfully purchased {itemName}");
        }
        else
        {
            OnPurchaseFailed?.Invoke(this);
        }
    }

    private void ExecutePurchase()
    {
        hasPurchased = true;

        // Reduce stock if limited
        if (stockQuantity > 0)
        {
            stockQuantity--;
        }

        // Set flags if this is a one-time purchase
        if (oneTimePurchase && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Set($"purchased_{itemName}", true);
        }

        // Override in derived classes for specific item effects
        OnItemPurchased();
    }

    protected virtual void OnItemPurchased()
    {
        // Override in derived classes for specific behavior
        // e.g., add item to inventory, unlock features, etc.
    }

    private bool CheckStockAvailability()
    {
        return stockQuantity != 0; // -1 is unlimited, 0 is out of stock
    }

    private bool CheckFlagConditions()
    {
        if (GameStateManager.Instance == null) return true;

        // Check required flags
        if (requiredFlags != null)
        {
            foreach (string flag in requiredFlags)
            {
                if (!GameStateManager.Instance.Get(flag))
                {
                    return false;
                }
            }
        }

        // Check restricted flags
        if (restrictedFlags != null)
        {
            foreach (string flag in restrictedFlags)
            {
                if (GameStateManager.Instance.Get(flag))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SetCost(CurrencyCost newCost)
    {
        cost = newCost;
    }

    public void SetAvailability(bool available)
    {
        isAvailable = available;
    }

    public void SetStock(int quantity)
    {
        stockQuantity = quantity;
    }

    public void RestockItem(int quantity = -1)
    {
        if (quantity == -1)
        {
            stockQuantity = -1; // Unlimited
        }
        else
        {
            stockQuantity = quantity;
        }
    }

    // For loading saved purchase state
    public void LoadPurchaseState(bool purchased)
    {
        hasPurchased = purchased;
    }
}