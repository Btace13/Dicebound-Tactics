using System;
using UnityEngine;

[Serializable]
public class CurrencyAmount
{
    [SerializeField] private CurrencyType type;
    [SerializeField] private int amount;

    public CurrencyType Type => type;
    public int Amount => amount;

    public CurrencyAmount(CurrencyType currencyType, int currencyAmount)
    {
        type = currencyType;
        amount = currencyAmount;
    }

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(0, newAmount);
    }

    public void AddAmount(int addAmount)
    {
        amount = Mathf.Max(0, amount + addAmount);
    }

    public bool CanAfford(int cost)
    {
        return amount >= cost;
    }

    public bool SpendAmount(int spendAmount)
    {
        if (CanAfford(spendAmount))
        {
            amount -= spendAmount;
            return true;
        }
        return false;
    }

    public override string ToString()
    {
        return $"{amount} {type}";
    }
}