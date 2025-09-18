using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CurrencySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool useRandomPosition = true;
    [SerializeField, ShowIf("useRandomPosition")] private float spawnRadius = 2f;
    [SerializeField, ShowIf("useRandomPosition")] private bool spawnOnGround = true;
    [SerializeField, ShowIf("useRandomPosition")] private LayerMask groundLayer = 1;

    [Header("Drop Configuration")]
    [SerializeField] private List<CurrencyDrop> currencyDrops = new List<CurrencyDrop>();

    [System.Serializable]
    public class CurrencyDrop
    {
        public CurrencyType currencyType;
        public int amount;
        [Range(0f, 1f)] public float dropChance = 1f;
        public bool randomAmount = false;
        [ShowIf("randomAmount")] public int minAmount = 1;
        [ShowIf("randomAmount")] public int maxAmount = 10;
    }

    [Button("Spawn All Currencies")]
    public void SpawnAllCurrencies()
    {
        foreach (var drop in currencyDrops)
        {
            SpawnCurrency(drop);
        }
    }

    public void SpawnCurrency(CurrencyType type, int amount)
    {
        var drop = new CurrencyDrop
        {
            currencyType = type,
            amount = amount,
            dropChance = 1f
        };
        SpawnCurrency(drop);
    }

    public void SpawnCurrency(CurrencyDrop drop)
    {
        // Check drop chance
        if (Random.Range(0f, 1f) > drop.dropChance)
            return;

        // Calculate amount
        int finalAmount = drop.randomAmount ? 
            Random.Range(drop.minAmount, drop.maxAmount + 1) : 
            drop.amount;

        // Calculate spawn position
        Vector3 spawnPosition = GetSpawnPosition();

        // Create the pickup
        var pickup = CurrencyPickup.CreateCurrencyPickup(spawnPosition, drop.currencyType, finalAmount, transform);
        
        // Add some spread if spawning multiple items
        if (useRandomPosition)
        {
            var randomOffset = Random.insideUnitSphere * 0.5f;
            randomOffset.y = 0; // Keep on same Y level
            pickup.transform.position += randomOffset;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 basePosition = transform.position;

        if (useRandomPosition)
        {
            // Random position within spawn radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPosition = basePosition + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (spawnOnGround)
            {
                // Raycast down to find ground
                if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
                {
                    return hit.point + Vector3.up * 0.1f; // Slightly above ground
                }
            }

            return randomPosition;
        }

        return basePosition;
    }

    // Quick spawn methods for common use cases
    [Button("Spawn Random Gold (10-50)")]
    public void SpawnRandomGold()
    {
        SpawnCurrency(CurrencyType.Gold, Random.Range(10, 51));
    }

    [Button("Spawn Random Shards (1-10)")]
    public void SpawnRandomShards()
    {
        SpawnCurrency(CurrencyType.Shards, Random.Range(1, 11));
    }

    [Button("Spawn Victory Rewards")]
    public void SpawnVictoryRewards()
    {
        // Example victory rewards
        SpawnCurrency(CurrencyType.Gold, Random.Range(50, 101));
        if (Random.Range(0f, 1f) < 0.3f) // 30% chance for shards
        {
            SpawnCurrency(CurrencyType.Shards, Random.Range(1, 6));
        }
    }

    // Called from other systems (like enemy death, quest completion, etc.)
    public void OnEnemyDefeated(int enemyLevel = 1)
    {
        int goldAmount = Mathf.RoundToInt(10 + (enemyLevel * 5) + Random.Range(-5, 6));
        SpawnCurrency(CurrencyType.Gold, Mathf.Max(1, goldAmount));

        // Chance for shards based on enemy level
        float shardChance = 0.1f + (enemyLevel * 0.05f);
        if (Random.Range(0f, 1f) < shardChance)
        {
            SpawnCurrency(CurrencyType.Shards, Random.Range(1, 4));
        }
    }

    public void OnQuestCompleted(int questDifficulty = 1)
    {
        int goldReward = questDifficulty * Random.Range(25, 51);
        int shardReward = questDifficulty * Random.Range(1, 4);
        
        SpawnCurrency(CurrencyType.Gold, goldReward);
        SpawnCurrency(CurrencyType.Shards, shardReward);
    }

    // Visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        if (useRandomPosition)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }
}