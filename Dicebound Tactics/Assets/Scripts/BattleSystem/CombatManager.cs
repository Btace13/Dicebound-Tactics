using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TacticsToolkit;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Space(10)]
    [Header("Component References")]
    [SerializeField] private SelectionController selectionController;
    [SerializeField] private TurnManager turnManager;

    [Header("Events")]
    public GameEventEntity OnTargetSelected;

    public void BasicAttackSelected()
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
        {
            Debug.LogError("No active character to use the basic attack.");
            return;
        }

        selectionController.ChangeSelectionType(true);
        selectionController.SetSelectableTargetCount(1);
        selectionController.ToggleEntitySelection(turnManager.enemyUnits[0], false);
    }

    public void AbilitySelected(Ability ability)
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
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

        bool targetsEnemy = ability.abilityType == Ability.AbilityTypes.Enemy;

        // sets whether the ability is for allies or enemies
        selectionController.ChangeSelectionType(targetsEnemy);
        // sets the number of targets the ability can hit, 
        // TODO: need to implement logic for this
        selectionController.SetSelectableTargetCount(1);
        selectionController.ToggleEntitySelection(targetsEnemy ? turnManager.enemyUnits[0] : turnManager.playerUnits[0], false);
    }

    public void ItemSelected(CombatItem item)
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
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
        selectionController.ToggleEntitySelection(turnManager.playerUnits[0], false); // Assuming items target allies
    }
}
