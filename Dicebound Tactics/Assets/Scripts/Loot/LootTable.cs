using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LootTable", menuName = "Loot/Loot Table")]
public class LootTable : ScriptableObject
{
    [System.Serializable]
    public class LootEntry
    {
        public CurrencyType currencyType;
        public int minAmount = 1;
        public int maxAmount = 10;
        [Range(0f, 1f)] public float dropChance = 1f; // 1 = always, 0.5 = 50%
    }

    public List<LootEntry> lootEntries = new List<LootEntry>();

    public List<(CurrencyType, int)> RollLoot()
    {
        var results = new List<(CurrencyType, int)>();
        foreach (var entry in lootEntries)
        {
            if (Random.value <= entry.dropChance)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                results.Add((entry.currencyType, amount));
            }
        }
        return results;
    }
}
