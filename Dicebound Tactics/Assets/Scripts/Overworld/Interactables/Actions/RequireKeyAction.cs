using Sirenix.OdinInspector;
using TacticsToolkit;
using UnityEngine;

[System.Serializable]
public class RequireKeyAction : InteractableConditionalAction
{
    public CombatItem requiredItem;

    public override bool CheckCondition(GameObject interactor, GameObject target)
    {
        var inventory = interactor.GetComponent<Entity>().inventory;
        return inventory != null && inventory.HasItem(requiredItem);
    }
}
