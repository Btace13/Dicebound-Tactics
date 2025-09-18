using UnityEngine;
using System.Collections.Generic;

public static class CurrencyUtils
{
    /// <summary>
    /// Quick method to add currency without needing to access CurrencyManager directly
    /// </summary>
    public static void AddCurrency(CurrencyType type, int amount)
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(type, amount);
        }
    }

    /// <summary>
    /// Quick method to spend currency without needing to access CurrencyManager directly
    /// </summary>
    public static bool SpendCurrency(CurrencyType type, int amount)
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.SpendCurrency(type, amount);
    }

    /// <summary>
    /// Quick method to check if player can afford a cost
    /// </summary>
    public static bool CanAfford(CurrencyType type, int amount)
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(type, amount);
    }

    /// <summary>
    /// Quick method to check if player can afford a complex cost
    /// </summary>
    public static bool CanAfford(CurrencyCost cost)
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.CanAfford(cost);
    }

    /// <summary>
    /// Get current amount of a specific currency
    /// </summary>
    public static int GetCurrency(CurrencyType type)
    {
        return CurrencyManager.Instance?.GetCurrency(type) ?? 0;
    }

    /// <summary>
    /// Create a currency cost with multiple currencies
    /// </summary>
    public static CurrencyCost CreateCost(params (CurrencyType type, int amount)[] costs)
    {
        var cost = new CurrencyCost();
        foreach (var (type, amount) in costs)
        {
            cost.AddCost(type, amount);
        }
        return cost;
    }

    /// <summary>
    /// Create a simple currency cost for a single currency type
    /// </summary>
    public static CurrencyCost CreateSimpleCost(CurrencyType type, int amount)
    {
        return new CurrencyCost(new CurrencyAmount(type, amount));
    }

    /// <summary>
    /// Spawn a currency pickup at a specific location using configured prefabs
    /// </summary>
    public static GameObject SpawnCurrencyPickup(Vector3 position, CurrencyType type, int amount, Transform parent = null)
    {
        return CurrencyPickup.CreateCurrencyPickup(position, type, amount, parent);
    }

    /// <summary>
    /// Spawn multiple currency pickups with scatter effect
    /// </summary>
    public static void SpawnCurrencyScatter(Vector3 centerPosition, CurrencyType type, int totalAmount, int pickupCount = 5, float scatterRadius = 2f, Transform parent = null)
    {
        int amountPerPickup = Mathf.Max(1, totalAmount / pickupCount);
        int remainder = totalAmount % pickupCount;

        for (int i = 0; i < pickupCount; i++)
        {
            // Calculate scattered position
            Vector2 randomOffset = Random.insideUnitCircle * scatterRadius;
            Vector3 spawnPosition = centerPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            // Add remainder to first few pickups
            int pickupAmount = amountPerPickup + (i < remainder ? 1 : 0);

            SpawnCurrencyPickup(spawnPosition, type, pickupAmount, parent);
        }
    }

    /// <summary>
    /// Format currency amount for display (e.g., 1000 -> 1K)
    /// </summary>
    public static string FormatCurrencyForDisplay(int amount)
    {
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

    /// <summary>
    /// Get a formatted string showing all player currencies
    /// </summary>
    public static string GetCurrencyStatusString()
    {
        if (CurrencyManager.Instance == null) return "Currency Manager not available";

        var currencies = CurrencyManager.Instance.GetAllCurrencies();
        var parts = new List<string>();

        foreach (var currency in currencies)
        {
            parts.Add($"{currency.Value} {currency.Key}");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Award currency for completing a battle/encounter
    /// </summary>
    public static void AwardBattleRewards(int goldAmount, int shardsAmount = 0)
    {
        if (goldAmount > 0) AddCurrency(CurrencyType.Gold, goldAmount);
        if (shardsAmount > 0) AddCurrency(CurrencyType.Shards, shardsAmount);
    }

    /// <summary>
    /// Check if a shop item can be purchased
    /// </summary>
    public static bool CanPurchaseItem(CurrencyCost itemCost)
    {
        return CanAfford(itemCost);
    }

    /// <summary>
    /// Attempt to purchase an item with currency cost
    /// </summary>
    public static bool PurchaseItem(CurrencyCost itemCost)
    {
        return CurrencyManager.Instance != null && CurrencyManager.Instance.SpendCurrency(itemCost);
    }
}