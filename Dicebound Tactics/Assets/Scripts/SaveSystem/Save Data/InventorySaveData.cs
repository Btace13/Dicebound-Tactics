using System.Linq;
using UnityEngine;

public class InventorySaveData : SaveData
{
    public CombatItem[] items; // Array of items in the inventory
    public int[] itemQuantities; // Corresponding quantities for each item ID

    public override void Apply(GameObject obj)
    {
        var inventory = obj.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("No Inventory component found on the GameObject.");
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (itemQuantities[i] > 0)
            {
                inventory.AddItem(items[i], itemQuantities[i]);
            }
        }
    }

    public override void Capture(GameObject obj)
    {
        var inventory = obj.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("No Inventory component found on the GameObject.");
            return;
        }

        items = inventory.combatItems.Keys.ToArray();
        itemQuantities = new int[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            itemQuantities[i] = inventory.combatItems[items[i]];
        }
    }
}