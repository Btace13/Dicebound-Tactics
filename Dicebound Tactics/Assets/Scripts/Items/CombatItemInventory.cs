using System.Collections.Generic;
using UnityEngine;

public class CombatItemInventory : MonoBehaviour
{
    [Header("Item Management")]
    public UDictionary<CombatItem, int> combatItems = new UDictionary<CombatItem, int>(); // List of combat items in the inventory

    public bool HasItem(CombatItem item)
    {
        if (combatItems == null)
        {
            Debug.LogWarning("Combat items dictionary is not initialized.");
            return false;
        }

        if (combatItems.ContainsKey(item))
        {
            int itemCount = combatItems[item];
            if (itemCount > 0)
            {
                return true; // Item exists in the inventory and has a count greater than zero
            }
            else
            {
                Debug.Log($"Item '{item.ItemName}' exists in the inventory but has a count of zero.");
            }
        }

        return false;
    }

    public void AddItem(CombatItem item, int quantity = 1)
    {
        if (combatItems == null)
        {
            combatItems = new UDictionary<CombatItem, int>();
        }

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

        if (combatItems.ContainsKey(item))
        {
            combatItems[item] += quantity; // Increase the count of the existing item
        }
        else
        {
            combatItems.Add(item, quantity); // Add the new item with its quantity
        }

        Debug.Log($"Added {quantity} of '{item.ItemName}' to the inventory.");
    }

    public void RemoveItem(CombatItem item, int quantity = 1)
    {
        if (combatItems == null || !combatItems.ContainsKey(item))
        {
            Debug.LogWarning($"Cannot remove '{item.ItemName}' because it does not exist in the inventory.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("Quantity must be greater than zero.");
            return;
        }

        if (combatItems[item] < quantity)
        {
            Debug.LogWarning($"Cannot remove {quantity} of '{item.ItemName}' because only {combatItems[item]} are available.");
            return;
        }

        combatItems[item] -= quantity; // Decrease the count of the item

        if (combatItems[item] <= 0)
        {
            combatItems.Remove(item); // Remove the item if its count is zero or less
            Debug.Log($"Removed '{item.ItemName}' from the inventory as its count reached zero.");
        }
        else
        {
            Debug.Log($"Removed {quantity} of '{item.ItemName}' from the inventory. Remaining: {combatItems[item]}");
        }
    }
}
