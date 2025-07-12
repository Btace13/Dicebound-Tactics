using TacticsToolkit;
using UnityEngine;

[System.Serializable]
public class GiveItemAction : InteractionAction
{
    public CombatItem item;
    public int quantity = 1;

    public override void Execute(GameObject interactor, GameObject target)
    {
        var inventory = interactor.GetComponent<Entity>()?.inventory;
        if (inventory != null)
        {
            inventory.AddItem(item, quantity);
            Debug.Log($"Gave {item.name} x{quantity}");
        }
    }
}