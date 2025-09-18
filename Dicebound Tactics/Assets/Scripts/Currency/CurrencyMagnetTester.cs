using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Testing utility for currency magnetic pickup functionality.
/// Place this on any GameObject in the scene to test currency spawning and magnetic behavior.
/// </summary>
public class CurrencyMagnetTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private CurrencyType testCurrencyType = CurrencyType.Gold;
    [SerializeField] private int testAmount = 10;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int spawnCount = 5;
    
    [Header("Visual Helpers")]
    [SerializeField] private bool showSpawnArea = true;
    
    [Button("Spawn Test Currencies")]
    private void SpawnTestCurrencies()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can only spawn currencies during play mode!");
            return;
        }
        
        for (int i = 0; i < spawnCount; i++)
        {
            // Generate random position around this object
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomCircle.x, 0.5f, randomCircle.y);
            
            // Create currency pickup at position
            GameObject pickup = CurrencyPickup.CreateCurrencyPickup(spawnPosition, testCurrencyType, testAmount);
            
            if (pickup != null)
            {
                Debug.Log($"Spawned {testCurrencyType} pickup with {testAmount} amount at {spawnPosition}");
            }
        }
    }
    
    [Button("Spawn Single Currency at Mouse")]
    private void SpawnAtMouse()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Can only spawn currencies during play mode!");
            return;
        }
        
        // Cast ray from camera to mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 spawnPosition = hit.point + Vector3.up * 0.5f;
            GameObject pickup = CurrencyPickup.CreateCurrencyPickup(spawnPosition, testCurrencyType, testAmount);
            
            if (pickup != null)
            {
                Debug.Log($"Spawned {testCurrencyType} pickup at mouse position: {spawnPosition}");
            }
        }
        else
        {
            Debug.LogWarning("Could not find valid spawn position at mouse location!");
        }
    }
    
    [Button("Clear All Currencies")]
    private void ClearAllCurrencies()
    {
        CurrencyPickup[] allPickups = FindObjectsOfType<CurrencyPickup>();
        
        foreach (var pickup in allPickups)
        {
            if (Application.isPlaying)
            {
                Destroy(pickup.gameObject);
            }
            else
            {
                DestroyImmediate(pickup.gameObject);
            }
        }
        
        Debug.Log($"Cleared {allPickups.Length} currency pickups from scene");
    }
    
    [Button("Test Magnetic Range")]
    private void TestMagneticRange()
    {
        CurrencyPickup[] allPickups = FindObjectsOfType<CurrencyPickup>();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        Debug.Log($"Found {allPickups.Length} currency pickups and {players.Length} players");
        
        foreach (var pickup in allPickups)
        {
            foreach (var player in players)
            {
                float distance = Vector3.Distance(pickup.transform.position, player.transform.position);
                Debug.Log($"Distance from {pickup.name} to {player.name}: {distance:F2} units");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showSpawnArea) return;
        
        // Draw spawn area
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw center point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
    }
}