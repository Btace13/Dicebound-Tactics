using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CurrencyRewardSystem : MonoBehaviour
{
    [Header("Reward Configuration")]
    [SerializeField] private List<CurrencyAmount> baseRewards = new List<CurrencyAmount>();
    
    [Header("Multipliers")]
    [SerializeField] private float difficultyMultiplier = 1f;
    [SerializeField] private float levelMultiplier = 1f;
    [SerializeField] private bool randomizeRewards = true;
    [SerializeField, ShowIf("randomizeRewards")] private float randomVariation = 0.2f; // +/- 20%

    // Events
    public static System.Action<Dictionary<CurrencyType, int>> OnRewardsGranted;

    public void GrantRewards()
    {
        var rewards = CalculateRewards();
        GrantCurrencies(rewards);
    }

    public void GrantRewards(List<CurrencyAmount> customRewards)
    {
        var rewards = new Dictionary<CurrencyType, int>();
        
        foreach (var reward in customRewards)
        {
            int amount = CalculateFinalAmount(reward.Amount);
            rewards[reward.Type] = amount;
        }
        
        GrantCurrencies(rewards);
    }

    public void GrantCurrency(CurrencyType type, int baseAmount)
    {
        int finalAmount = CalculateFinalAmount(baseAmount);
        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(type, finalAmount);
        }

        var rewards = new Dictionary<CurrencyType, int> { { type, finalAmount } };
        OnRewardsGranted?.Invoke(rewards);
    }

    private Dictionary<CurrencyType, int> CalculateRewards()
    {
        var rewards = new Dictionary<CurrencyType, int>();
        
        foreach (var baseReward in baseRewards)
        {
            int finalAmount = CalculateFinalAmount(baseReward.Amount);
            rewards[baseReward.Type] = finalAmount;
        }
        
        return rewards;
    }

    private int CalculateFinalAmount(int baseAmount)
    {
        float amount = baseAmount;
        
        // Apply multipliers
        amount *= difficultyMultiplier;
        amount *= levelMultiplier;
        
        // Apply randomization
        if (randomizeRewards)
        {
            float variation = Random.Range(-randomVariation, randomVariation);
            amount *= (1f + variation);
        }
        
        return Mathf.Max(1, Mathf.RoundToInt(amount));
    }

    private void GrantCurrencies(Dictionary<CurrencyType, int> rewards)
    {
        if (CurrencyManager.Instance == null) return;

        foreach (var reward in rewards)
        {
            CurrencyManager.Instance.AddCurrency(reward.Key, reward.Value);
        }

        OnRewardsGranted?.Invoke(rewards);
    }

    public void SetDifficultyMultiplier(float multiplier)
    {
        difficultyMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public void SetLevelMultiplier(float multiplier)
    {
        levelMultiplier = Mathf.Max(0.1f, multiplier);
    }

    [Button("Test Grant Rewards")]
    private void TestGrantRewards()
    {
        GrantRewards();
    }

    [Button("Grant 100 Gold")]
    private void TestGrantGold()
    {
        GrantCurrency(CurrencyType.Gold, 100);
    }

    [Button("Grant 50 Shards")]
    private void TestGrantShards()
    {
        GrantCurrency(CurrencyType.Shards, 50);
    }
}