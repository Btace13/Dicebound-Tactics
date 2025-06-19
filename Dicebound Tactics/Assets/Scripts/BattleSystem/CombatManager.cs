using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TacticsToolkit;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Combat State")]
    [ShowInInspector, ReadOnly] public CharacterManager CurrentActiveCharacter;
    [ShowInInspector, ReadOnly] public List<Entity> CurrentTargets = new List<Entity>();

    [Space(10)]
    [Header("Component References")]
    [SerializeField] private SelectionController selectionController;

    [Header("Events")]
    public GameEventEntity OnTargetSelected;

    public void SetActiveCharacterGameObject(GameObject character)
    {
        if (character == null)
        {
            Debug.LogError("Character GameObject is null.");
            return;
        }

        CharacterManager characterManager = character.GetComponent<CharacterManager>();
        if (characterManager == null)
        {
            Debug.LogError("Character GameObject does not have a CharacterManager component.");
            return;
        }

        SetActiveCharacter(characterManager);
    }

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

    public void BasicAttackSelected()
    {
        if (CurrentActiveCharacter == null)
        {
            Debug.LogError("No active character to use the basic attack.");
            return;
        }

        selectionController.ChangeSelectionType(true);
        selectionController.SetSelectableTargetCount(Math.Max(1, CurrentTargets.Count));
    }

    public void AbilitySelected(Ability ability)
    {
        if (CurrentActiveCharacter == null)
        {
            Debug.LogError("No active character to use the ability.");
            return;
        }

        if (selectionController == null)
        {
            Debug.LogError("SelectionController is not assigned.");
            return;
        }

        if (ability == null)
        {
            Debug.LogError("Ability is null.");
            return;
        }

        // sets whether the ability is for allies or enemies
        selectionController.ChangeSelectionType(ability.abilityType == Ability.AbilityTypes.Enemy);
        // sets the number of targets the ability can hit, 
        // TODO: need to implement logic for this
        selectionController.SetSelectableTargetCount(1);
    }

    public void ItemSelected(CombatItem item)
    {
        if (CurrentActiveCharacter == null)
        {
            Debug.LogError("No active character to use the item.");
            return;
        }

        if (selectionController == null)
        {
            Debug.LogError("SelectionController is not assigned.");
            return;
        }

        if (item == null)
        {
            Debug.LogError("Item is null.");
            return;
        }

        selectionController.ChangeSelectionType(false); // Assuming items target allies
        selectionController.SetSelectableTargetCount(1); // Assuming items can target one entity at a time
    }
}
