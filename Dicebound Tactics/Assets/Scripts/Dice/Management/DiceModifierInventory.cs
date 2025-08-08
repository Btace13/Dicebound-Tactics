using System.Collections.Generic;
using UnityEngine;

public class DiceModifierInventory : MonoBehaviour
{
    [Header("Item Management")]
    public UDictionary<DiceModifier, int> diceModifierItems = new UDictionary<DiceModifier, int>(); // List of dice modifier items in the inventory

    public bool HasItem(DiceModifier item)
    {
        if (diceModifierItems == null)
        {
            Debug.LogWarning("Dice modifier items dictionary is not initialized.");
            return false;
        }

        if (diceModifierItems.ContainsKey(item))
        {
            int itemCount = diceModifierItems[item];
            if (itemCount > 0)
            {
                return true; // Item exists in the inventory and has a count greater than zero
            }
            else
            {
                Debug.Log($"Item '{item.Name}' exists in the inventory but has a count of zero.");
            }
        }

        return false;
    }

    public void AddItem(DiceModifier item, int quantity = 1)
    {
        if (diceModifierItems == null)
        {
            diceModifierItems = new UDictionary<DiceModifier, int>();
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

        if (diceModifierItems.ContainsKey(item))
        {
            diceModifierItems[item] += quantity; // Increase the count of the existing item
        }
        else
        {
            diceModifierItems.Add(item, quantity); // Add the new item with its quantity
        }

        Debug.Log($"Added {quantity} of '{item.Name}' to the inventory.");
    }

    public void RemoveItem(DiceModifier item, int quantity = 1)
    {
        if (diceModifierItems == null || !diceModifierItems.ContainsKey(item))
        {
            Debug.LogWarning($"Cannot remove '{item.Name}' because it does not exist in the inventory.");
            return;
        }

        if (quantity <= 0)
        {
            Debug.LogWarning("Quantity must be greater than zero.");
            return;
        }

        if (diceModifierItems[item] < quantity)
        {
            Debug.LogWarning($"Cannot remove {quantity} of '{item.Name}' because only {diceModifierItems[item]} are available.");
            return;
        }

        diceModifierItems[item] -= quantity; // Decrease the count of the item

        if (diceModifierItems[item] <= 0)
        {
            diceModifierItems.Remove(item); // Remove the item if its count is zero or less
            Debug.Log($"Removed '{item.Name}' from the inventory as its count reached zero.");
        }
        else
        {
            Debug.Log($"Removed {quantity} of '{item.Name}' from the inventory. Remaining: {diceModifierItems[item]}");
        }
    }
}
