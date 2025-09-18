using UnityEngine;
using System;

[Serializable]
public class CraftingRecipe : ICraftable
{
    [Header("Recipe Information")]
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Crafting Cost")]
    [SerializeField] private CurrencyCost craftingCost;

    [Header("Requirements")]
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private string[] requiredFlags;
    [SerializeField] private int requiredLevel = 0;

    [Header("Output")]
    [SerializeField] private GameObject craftedItemPrefab;
    [SerializeField] private int outputQuantity = 1;

    // Events
    public event Action<CraftingRecipe> OnCrafted;
    public event Action<CraftingRecipe> OnCraftingFailed;

    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public CurrencyCost CraftingCost => craftingCost;
    public GameObject CraftedItemPrefab => craftedItemPrefab;
    public int OutputQuantity => outputQuantity;
    public bool IsUnlocked => isUnlocked && CheckRequirements();

    public bool CanCraft()
    {
        if (!IsUnlocked) return false;
        
        return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(craftingCost);
    }

    public void Craft()
    {
        if (!CanCraft())
        {
            Debug.LogWarning($"Cannot craft {itemName}");
            OnCraftingFailed?.Invoke(this);
            return;
        }

        if (CurrencyManager.Instance.SpendCurrency(craftingCost))
        {
            ExecuteCrafting();
            OnCrafted?.Invoke(this);
            Debug.Log($"Successfully crafted {itemName}");
        }
        else
        {
            OnCraftingFailed?.Invoke(this);
        }
    }

    private void ExecuteCrafting()
    {
        // Override in derived classes or handle through events
        OnItemCrafted();
    }

    protected virtual void OnItemCrafted()
    {
        // Override in derived classes for specific behavior
        // e.g., add crafted item to inventory, create objects, etc.
    }

    private bool CheckRequirements()
    {
        // Check level requirement (if you have a player level system)
        // if (PlayerLevel.Instance != null && PlayerLevel.Instance.Level < requiredLevel)
        //     return false;

        // Check flag requirements
        if (GameStateManager.Instance != null && requiredFlags != null)
        {
            foreach (string flag in requiredFlags)
            {
                if (!GameStateManager.Instance.Get(flag))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SetCost(CurrencyCost newCost)
    {
        craftingCost = newCost;
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
    }

    public void UnlockRecipe()
    {
        isUnlocked = true;
        
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Set($"recipe_{itemName}_unlocked", true);
        }
    }

    // For loading saved unlock state
    public void LoadUnlockState()
    {
        if (GameStateManager.Instance != null)
        {
            isUnlocked = GameStateManager.Instance.Get($"recipe_{itemName}_unlocked");
        }
    }
}