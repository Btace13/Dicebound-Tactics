using System.Collections.Generic;
using TacticsToolkit;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public CharacterManager CurrentActiveCharacter { get; private set; }
    public List<Entity> CurrentTargets { get; private set; } = new List<Entity>();

    public void SetTarget(Entity target)
    {
        if (target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        if (CurrentTargets.Contains(target))
        {
            Debug.LogWarning("Target already selected.");
            return;
        }

        CurrentTargets.Add(target);
    }

    public void SetTargets(List<Entity> targets)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogError("Targets list is null or empty.");
            return;
        }

        CurrentTargets = targets;
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

        if (CurrentTargets == null || CurrentTargets.Count == 0)
        {
            Debug.LogError("No targets selected for the ability.");
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

        if (CurrentTargets == null || CurrentTargets.Count == 0)
        {
            Debug.LogError("No targets selected for the item.");
            return;
        }

        // Implement item usage logic here
    }
}
