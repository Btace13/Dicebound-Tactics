using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CurrencyCost
{
    [SerializeField] private List<CurrencyAmount> costs = new List<CurrencyAmount>();

    public List<CurrencyAmount> Costs => costs;

    public CurrencyCost()
    {
        costs = new List<CurrencyAmount>();
    }

    public CurrencyCost(params CurrencyAmount[] currencyCosts)
    {
        costs = currencyCosts.ToList();
    }

    public void AddCost(CurrencyType type, int amount)
    {
        var existingCost = costs.FirstOrDefault(c => c.Type == type);
        if (existingCost != null)
        {
            existingCost.AddAmount(amount);
        }
        else
        {
            costs.Add(new CurrencyAmount(type, amount));
        }
    }

    public int GetCost(CurrencyType type)
    {
        var cost = costs.FirstOrDefault(c => c.Type == type);
        return cost?.Amount ?? 0;
    }

    public bool HasCost(CurrencyType type)
    {
        return costs.Any(c => c.Type == type && c.Amount > 0);
    }

    public bool IsEmpty()
    {
        return costs.Count == 0 || costs.All(c => c.Amount <= 0);
    }

    public override string ToString()
    {
        if (IsEmpty()) return "Free";
        
        return string.Join(", ", costs.Where(c => c.Amount > 0).Select(c => c.ToString()));
    }
}