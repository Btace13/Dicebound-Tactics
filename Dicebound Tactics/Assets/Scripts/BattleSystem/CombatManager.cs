using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TacticsToolkit;
using UnityEngine;
using DG.Tweening;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Space(10)]
    [Header("Component References")]
    [SerializeField] private SelectionController selectionController;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private CombatUIHandler combatUIHandler;

    public SelectionController SelectionController => selectionController;
    public TurnManager TurnManager => turnManager;
    public CombatUIHandler CombatUIHandler => combatUIHandler;

    private CombatItem _selectedItem;
    private AbilitySO _selectedAbility;
    private CombatEncounter _currentEncounter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // Event Listeners
        EventManager.OnGameStateChanged += OnGameStateChanged;
        EventManager.OnCombatEncounterStarted += encounter => _currentEncounter = encounter;
        EventManager.OnCombatEncounterEnded += encounter => _currentEncounter = null;
    }

    void OnDisable()
    {
        EventManager.OnGameStateChanged -= OnGameStateChanged;
        EventManager.OnCombatEncounterStarted -= encounter => _currentEncounter = encounter;
        EventManager.OnCombatEncounterEnded -= encounter => _currentEncounter = null;
    }

    public void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Combat)
        {
            turnManager.StartBattle();
            combatUIHandler.ShowCombatUI();
            combatUIHandler.ShowBigNotification("Fight!", 0.5f);
        }
        else
        {
            // Hide the combat UI when not in combat
            combatUIHandler.HideCombatUI();
        }
    }

    public void BasicAttackSelected()
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
        {
            return;
        }

        selectionController.ChangeSelectionType(true);
        selectionController.SetSelectableTargetCount(1);
        selectionController.ToggleEntitySelection(turnManager.enemyUnits[0], false);
    }

    public void AbilitySelected(AbilitySO ability)
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
        {
            return;
        }

        if (selectionController == null)
        {
            return;
        }

        if (ability == null)
        {
            return;
        }

        bool targetsEnemy = ability.abilityType == AbilityType.Enemy;

        // sets whether the ability is for allies or enemies
        selectionController.ChangeSelectionType(targetsEnemy);
        // sets the number of targets the ability can hit, 
        // TODO: need to implement logic for this
        selectionController.SetSelectableTargetCount(1);
        selectionController.ToggleEntitySelection(targetsEnemy ? turnManager.enemyUnits[0] : turnManager.playerUnits[0], false);
        EventManager.TriggerSelectingATarget(true);

        _selectedAbility = ability;
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

        _selectedItem = item;
        selectionController.SetSelectableTargetCount(1); // Items typically target one entity at a time

        // Determine targeting based on item properties
        bool targetAllies = item.canTargetAllies || item.canTargetSelf;
        bool targetEnemies = item.canTargetEnemies;
        
        // For revive items, check if they can target dead allies
        if (item.canTargetDeadAllies)
        {
            targetAllies = true;
            targetEnemies = false;
        }

        // Set up selection controller based on item targeting
        if (targetAllies && targetEnemies)
        {
            // Can target both allies and enemies
            selectionController.ChangeSelectionType(true); // Allow both
            selectionController.ToggleEntitySelection(turnManager.playerUnits[0], false);
        }
        else if (targetEnemies)
        {
            // Target enemies only
            selectionController.ChangeSelectionType(true); // Target enemies
            selectionController.ToggleEntitySelection(turnManager.enemyUnits[0], false);
        }
        else
        {
            // Target allies only (default)
            selectionController.ChangeSelectionType(false); // Target allies
            selectionController.ToggleEntitySelection(turnManager.playerUnits[0], false);
        }
        
        // Start target selection
        EventManager.TriggerSelectingATarget(true);
        
        Debug.Log($"Item {item.ItemName} selected. Targeting mode: Allies={targetAllies}, Enemies={targetEnemies}");
    }

    public void ExecuteAction()
    {
        EventManager.TriggerSelectingATarget(false);
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
        {
            return;
        }

        if (selectionController == null)
        {
            return;
        }

        List<Entity> selectedTargets = selectionController.SelectedEntities;

        if (selectedTargets.Count == 0)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            if (_selectedAbility != null)
            {
                // Show notification for ability usage
                combatUIHandler.ShowNotification($"{currentUnit.name}{_selectedAbility.notifcationMessage}", 1);
            }
            else if (_selectedItem != null)
            {
                // Show notification for item usage
                combatUIHandler.ShowNotification($"{currentUnit.name} used a {_selectedItem.ItemName}", 1);
            }
            else
            {
                //Show notifcation that the action is being executed
                if (selectedTargets.Count > 1)
                    combatUIHandler.ShowNotification($"{currentUnit.name} is attacking {selectedTargets.Count} targets", 1);
                else
                    combatUIHandler.ShowNotification($"{currentUnit.name} is attacking {selectedTargets[0].name}", 1);
            }

            // set the active camera as the AttackCamera
            CameraManager.Instance?.TrySetActiveCamera(_currentEncounter.GetCameraControllerForSide(currentUnit).name);

            //TODO: trigger animations / effects here
        });
        //TODO: the duration of the sequence should be based on the animation length
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() =>
        {

            // Execute the action with the selected targets
            foreach (var target in selectedTargets)
            {
                // if should use ability
                if (_selectedAbility != null)
                {
                    DamageAbilitySO damageAbility = _selectedAbility as DamageAbilitySO;

                    if (damageAbility == null)
                    {
                        continue;
                    }

                    StartCoroutine(damageAbility.Execute(currentUnit, target));
                }
                else if (_selectedItem != null)
                {
                    // Use the new centralized item usage method
                    bool itemUsed = currentUnit.UseCombatItem(_selectedItem, target);

                    if (!itemUsed)
                    {
                        Debug.LogWarning($"{currentUnit.name} failed to use {_selectedItem.ItemName} on {target.name}");
                    }
                    else
                    { 
                        TurnManager.Instance.StartNextTurn();
                    }
                }
                else // basic attack
                {
                    Vector3 dir = (target.transform.position - currentUnit.transform.position).normalized;
                    dir.y = 0;

                    OverworldCharacterController cc = currentUnit.GetComponent<OverworldCharacterController>();

                    if (cc != null)
                    {
                        cc.MoveToPosition(target.transform.position - dir * 3f, true, () =>
                        {
                            Sequence attackSequence = DOTween.Sequence();
                            attackSequence.AppendCallback(() =>
                            {
                                int damage = currentUnit.characterClass.Strength.baseStatValue;
                                target.TakeDamage(damage);
                                CombatUIHandler.damageNumberUIHandler.ShowDamageNumber(damage, target.transform.position, DamageNumberType.Normal);
                                // CameraManager.Instance?.ShakeActiveCamera();
                            });
                            attackSequence.AppendInterval(0.5f);
                            attackSequence.AppendCallback(() =>
                            {
                                // return to original position
                                cc.MoveToPosition(cc.AssignedEncounterSlot.slotTransform.position, true);
                            });
                        });
                    }
                    else
                    {
                        int damage = currentUnit.characterClass.Strength.baseStatValue;
                        target.TakeDamage(damage);
                        CombatUIHandler.damageNumberUIHandler.ShowDamageNumber(damage, target.transform.position, DamageNumberType.Normal);
                        // CameraManager.Instance?.ShakeActiveCamera();
                    }
                }
            }
        });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            _selectedAbility = null;
            _selectedItem = null;
            SelectionController.Instance.ClearAllSelections();
        });
    }
}
