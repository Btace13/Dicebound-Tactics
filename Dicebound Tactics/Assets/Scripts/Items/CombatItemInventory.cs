using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class CombatItemEntry
{
    [Tooltip("The combat item")]
    public CombatItem item;
    
    [Tooltip("The quantity of this item")]
    [Min(0)]
    public int quantity;

    public CombatItemEntry(CombatItem item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}

public class CombatItemInventory : MonoBehaviour
{
    [Header("Item Management")]
    [Tooltip("List of combat items in the inventory with their quantities")]
    public List<CombatItemEntry> combatItems = new List<CombatItemEntry>();

    private void Awake()
    {
        CleanupNullEntries();
    }

    private void OnValidate()
    {
        // Only clean up in play mode or when not actively editing in inspector
        if (Application.isPlaying)
        {
            CleanupNullEntries();
        }
        else
        {
            // In edit mode, just do a basic check without modifying
            ValidateInventoryState();
        }
    }

    /// <summary>
    /// Validates inventory state without modifying it (safe for edit mode)
    /// </summary>
    private void ValidateInventoryState()
    {
        if (combatItems == null)
        {
            combatItems = new List<CombatItemEntry>();
            return;
        }

        try
        {
            // Just count null entries without removing them
            int nullCount = 0;
            foreach (var entry in combatItems)
            {
                if (entry?.item == null)
                {
                    nullCount++;
                }
            }

            if (nullCount > 0)
            {
                Debug.LogWarning($"Found {nullCount} null entries in combat inventory. Use 'Clean Up Null Entries' to fix.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error validating inventory: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes any null entries from the combat items list
    /// </summary>
    private void CleanupNullEntries()
    {
        if (combatItems == null)
        {
            combatItems = new List<CombatItemEntry>();
            return;
        }

        try
        {
            // Remove null entries or entries with null items
            for (int i = combatItems.Count - 1; i >= 0; i--)
            {
                if (combatItems[i] == null || combatItems[i].item == null)
                {
                    Debug.LogWarning("Removed null item entry from combat inventory.");
                    combatItems.RemoveAt(i);
                }
                else if (combatItems[i].quantity <= 0)
                {
                    Debug.LogWarning($"Removed '{combatItems[i].item.ItemName}' with zero or negative quantity from inventory.");
                    combatItems.RemoveAt(i);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during inventory cleanup: {ex.Message}");
            // If cleanup fails, create a new empty list
            combatItems = new List<CombatItemEntry>();
        }
    }

    public bool HasItem(CombatItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot check for null item in inventory.");
            return false;
        }

        CleanupNullEntries();

        if (combatItems == null)
        {
            Debug.LogWarning("Combat items list is not initialized.");
            return false;
        }

        var entry = combatItems.FirstOrDefault(e => e.item == item);
        if (entry != null && entry.quantity > 0)
        {
            return true; // Item exists in the inventory and has a count greater than zero
        }
        else if (entry != null && entry.quantity <= 0)
        {
            Debug.Log($"Item '{item.ItemName}' exists in the inventory but has a count of zero.");
        }

        return false;
    }

    public void AddItem(CombatItem item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot add a null item to the inventory.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("Quantity must be greater than zero.");
            return;
        }

        CleanupNullEntries();

        if (combatItems == null)
        {
            combatItems = new List<CombatItemEntry>();
        }

        var existingEntry = combatItems.FirstOrDefault(e => e.item == item);
        if (existingEntry != null)
        {
            existingEntry.quantity += quantity; // Increase the count of the existing item
        }
        else
        {
            combatItems.Add(new CombatItemEntry(item, quantity)); // Add the new item with its quantity
        }

        Debug.Log($"Added {quantity} of '{item.ItemName}' to the inventory.");
    }

    public void RemoveItem(CombatItem item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot remove a null item from the inventory.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("Quantity must be greater than zero.");
            return;
        }

        CleanupNullEntries();

        var existingEntry = combatItems?.FirstOrDefault(e => e.item == item);
        if (existingEntry == null)
        {
            Debug.LogWarning($"Cannot remove '{item.ItemName}' because it does not exist in the inventory.");
            return;
        }

        if (existingEntry.quantity < quantity)
        {
            Debug.LogWarning($"Cannot remove {quantity} of '{item.ItemName}' because only {existingEntry.quantity} are available.");
            return;
        }

        existingEntry.quantity -= quantity; // Decrease the count of the item

        if (existingEntry.quantity <= 0)
        {
            combatItems.Remove(existingEntry); // Remove the entry if its count is zero or less
            Debug.Log($"Removed '{item.ItemName}' from the inventory as its count reached zero.");
        }
        else
        {
            Debug.Log($"Removed {quantity} of '{item.ItemName}' from the inventory. Remaining: {existingEntry.quantity}");
        }
    }

    /// <summary>
    /// Manual cleanup method that can be called from inspector or code
    /// </summary>
    [ContextMenu("Clean Up Null Entries")]
    public void ManualCleanupNullEntries()
    {
        CleanupNullEntries();
        Debug.Log("Manual cleanup of null entries completed.");
    }

    /// <summary>
    /// Get all valid items in the inventory (excluding null entries)
    /// </summary>
    public Dictionary<CombatItem, int> GetValidItems()
    {
        var validItems = new Dictionary<CombatItem, int>();
        
        if (combatItems == null)
        {
            return validItems;
        }

        try
        {
            foreach (var entry in combatItems)
            {
                if (entry?.item != null && entry.quantity > 0)
                {
                    validItems[entry.item] = entry.quantity;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error getting valid items: {ex.Message}");
        }

        return validItems;
    }

    /// <summary>
    /// Get the quantity of a specific item in the inventory
    /// </summary>
    public int GetItemQuantity(CombatItem item)
    {
        if (item == null) return 0;
        
        var entry = combatItems?.FirstOrDefault(e => e.item == item);
        return entry?.quantity ?? 0;
    }
}
