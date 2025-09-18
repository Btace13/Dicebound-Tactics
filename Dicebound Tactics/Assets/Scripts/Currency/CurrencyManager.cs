using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Starting Currencies")]
    [SerializeField] private List<CurrencyAmount> startingCurrencies = new List<CurrencyAmount>();

    [Header("Current Currencies")]
    [SerializeField, ReadOnly] private List<CurrencyAmount> currentCurrencies = new List<CurrencyAmount>();

    // Events
    public static event Action<CurrencyType, int, int> OnCurrencyChanged; // type, oldAmount, newAmount
    public static event Action<CurrencyType, int> OnCurrencyGained; // type, amount
    public static event Action<CurrencyType, int> OnCurrencySpent; // type, amount

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCurrencies();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeCurrencies()
    {
        currentCurrencies.Clear();
        
        // Initialize all currency types
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            var startingAmount = startingCurrencies.FirstOrDefault(c => c.Type == type);
            int amount = startingAmount?.Amount ?? 0;
            currentCurrencies.Add(new CurrencyAmount(type, amount));
        }
    }

    #region Currency Access Methods

    public int GetCurrency(CurrencyType type)
    {
        var currency = currentCurrencies.FirstOrDefault(c => c.Type == type);
        return currency?.Amount ?? 0;
    }

    public void SetCurrency(CurrencyType type, int amount)
    {
        var currency = GetOrCreateCurrency(type);
        int oldAmount = currency.Amount;
        currency.SetAmount(amount);
        
        OnCurrencyChanged?.Invoke(type, oldAmount, currency.Amount);
    }

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        var currency = GetOrCreateCurrency(type);
        int oldAmount = currency.Amount;
        currency.AddAmount(amount);
        
        OnCurrencyChanged?.Invoke(type, oldAmount, currency.Amount);
        OnCurrencyGained?.Invoke(type, amount);
        
        Debug.Log($"Gained {amount} {type}. Total: {currency.Amount}");
    }

    public bool CanAfford(CurrencyType type, int amount)
    {
        return GetCurrency(type) >= amount;
    }

    public bool CanAfford(CurrencyCost cost)
    {
        return cost.Costs.All(c => CanAfford(c.Type, c.Amount));
    }

    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (!CanAfford(type, amount))
        {
            Debug.LogWarning($"Insufficient {type}. Required: {amount}, Available: {GetCurrency(type)}");
            return false;
        }

        var currency = GetOrCreateCurrency(type);
        int oldAmount = currency.Amount;
        currency.SpendAmount(amount);
        
        OnCurrencyChanged?.Invoke(type, oldAmount, currency.Amount);
        OnCurrencySpent?.Invoke(type, amount);
        
        Debug.Log($"Spent {amount} {type}. Remaining: {currency.Amount}");
        return true;
    }

    public bool SpendCurrency(CurrencyCost cost)
    {
        if (!CanAfford(cost))
        {
            var insufficientCosts = cost.Costs.Where(c => !CanAfford(c.Type, c.Amount));
            foreach (var insufficient in insufficientCosts)
            {
                Debug.LogWarning($"Insufficient {insufficient.Type}. Required: {insufficient.Amount}, Available: {GetCurrency(insufficient.Type)}");
            }
            return false;
        }

        // Spend all required currencies
        foreach (var currencyCost in cost.Costs)
        {
            SpendCurrency(currencyCost.Type, currencyCost.Amount);
        }

        return true;
    }

    #endregion

    #region Utility Methods

    private CurrencyAmount GetOrCreateCurrency(CurrencyType type)
    {
        var currency = currentCurrencies.FirstOrDefault(c => c.Type == type);
        if (currency == null)
        {
            currency = new CurrencyAmount(type, 0);
            currentCurrencies.Add(currency);
        }
        return currency;
    }

    public Dictionary<CurrencyType, int> GetAllCurrencies()
    {
        var result = new Dictionary<CurrencyType, int>();
        foreach (var currency in currentCurrencies)
        {
            result[currency.Type] = currency.Amount;
        }
        return result;
    }

    #endregion

    #region Debug Methods

    [Button("Add 100 Gold")]
    private void AddTestGold()
    {
        AddCurrency(CurrencyType.Gold, 100);
    }

    [Button("Add 50 Shards")]
    private void AddTestShards()
    {
        AddCurrency(CurrencyType.Shards, 50);
    }

    [Button("Test Spend 25 Gold")]
    private void TestSpendGold()
    {
        SpendCurrency(CurrencyType.Gold, 25);
    }

    [Button("Reset All Currencies")]
    private void ResetCurrencies()
    {
        foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
        {
            SetCurrency(type, 0);
        }
    }

    #endregion
}