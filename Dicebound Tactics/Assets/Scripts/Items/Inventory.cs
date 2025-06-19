using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
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
}
