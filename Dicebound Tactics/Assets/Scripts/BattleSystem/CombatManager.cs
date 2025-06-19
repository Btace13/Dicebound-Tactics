using TacticsToolkit;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Entity CurrentTarget { get; private set; }
    public CharacterManager CurrentActiveCharacter { get; private set; }

    public void SetTarget(Entity target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        CurrentTarget = target;
        Debug.Log($"Current target set to: {CurrentTarget.name}");
    }

    public void SetActiveCharacter(CharacterManager character)
    {
        if (character == null)
        {
            Debug.LogError("Active character is null.");
            return;
        }

        CurrentActiveCharacter = character;
        Debug.Log($"Current active character set to: {CurrentActiveCharacter.name}");
    }

    public void AbilitySelected(Ability ability)
    {
        if (CurrentActiveCharacter == null)
        {
            Debug.LogError("No active character to use the ability.");
            return;
        }

        if (CurrentTarget == null)
        {
            Debug.LogError("No target selected for the ability.");
            return;
        }


    }

    public void ItemSelected(CombatItem item)
    {
        if (CurrentActiveCharacter == null)
        {
            Debug.LogError("No active character to use the item.");
            return;
        }

        if (CurrentTarget == null)
        {
            Debug.LogError("No target selected for the item.");
            return;
        }

        // Implement item usage logic here
    }
}
