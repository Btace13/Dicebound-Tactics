using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurrencySaveData", menuName = "Save System/Currency Save Data")]
public class CurrencySaveData : SaveData
{
    [SerializeField] private List<CurrencyAmount> currencies = new List<CurrencyAmount>();

    public List<CurrencyAmount> Currencies
    {
        get => currencies;
        set => currencies = value;
    }

    public override void Capture(GameObject go)
    {
        var currencyManager = go.GetComponent<CurrencyManager>();
        if (currencyManager != null)
        {
            currencies.Clear();
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                int amount = currencyManager.GetCurrency(type);
                if (amount > 0)
                {
                    currencies.Add(new CurrencyAmount(type, amount));
                }
            }
        }
    }

    public override void Apply(GameObject go)
    {
        var currencyManager = go.GetComponent<CurrencyManager>();
        if (currencyManager != null)
        {
            // Clear existing currencies
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyManager.SetCurrency(type, 0);
            }

            // Apply saved currencies
            foreach (var currency in currencies)
            {
                currencyManager.SetCurrency(currency.Type, currency.Amount);
            }
        }
    }
}