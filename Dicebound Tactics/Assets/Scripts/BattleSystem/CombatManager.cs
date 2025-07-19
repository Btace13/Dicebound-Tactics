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

        if (selectionController == null)
            Debug.LogError("SelectionController is not assigned in CombatManager.");

        if (turnManager == null)
            Debug.LogError("TurnManager is not assigned in CombatManager.");

        if (combatUIHandler == null)
            Debug.LogError("CombatUIHandler is not assigned in CombatManager.");

        // Event Listeners
        EventManager.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDisable()
    {
        EventManager.OnGameStateChanged -= OnGameStateChanged;
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
            Debug.LogError("No active character to use the basic attack.");
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

        bool targetsEnemy = ability.abilityType == AbilityType.Enemy;

        // sets whether the ability is for allies or enemies
        selectionController.ChangeSelectionType(targetsEnemy);
        // sets the number of targets the ability can hit, 
        // TODO: need to implement logic for this
        selectionController.SetSelectableTargetCount(1);
        selectionController.ToggleEntitySelection(targetsEnemy ? turnManager.enemyUnits[0] : turnManager.playerUnits[0], false);

        _selectedAbility = ability;
        Debug.Log($"Selected ability: {_selectedAbility.abilityName}");
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

    public void ExecuteAction()
    {
        Entity currentUnit = turnManager.GetCurrentUnit();

        if (currentUnit == null)
        {
            Debug.LogError("No active character to execute action.");
            return;
        }

        if (selectionController == null)
        {
            Debug.LogError("SelectionController is not assigned.");
            return;
        }

        List<Entity> selectedTargets = selectionController.SelectedEntities;

        if (selectedTargets.Count == 0)
        {
            Debug.LogError("No targets selected.");
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
            CameraManager.Instance?.TrySetActiveCamera("AttackCamera");

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
                        Debug.LogError("Selected ability is not a DamageAbilitySO.");
                        continue;
                    }

                    StartCoroutine(damageAbility.Execute(currentUnit, target));

                    Debug.Log($"{currentUnit.name} uses {_selectedAbility.abilityName} on {target.name}");
                }
                else if (_selectedItem != null)
                {
                    // Use the item on the target
                    if (target is CharacterManager character)
                    {
                        //character.UseItem(_selectedItem);
                        Debug.Log($"{currentUnit.name} uses {_selectedItem.ItemName} on {character.name}.");
                    }
                    else
                    {
                        Debug.LogError("Selected target is not a character.");
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
                                CameraManager.Instance?.ShakeActiveCamera();
                                Debug.Log($"{currentUnit.name} attacks {target.name} for {damage} damage.");
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
                        CameraManager.Instance?.ShakeActiveCamera();
                        Debug.Log($"{currentUnit.name} attacks {target.name} for {damage} damage.");
                    }
                }
            }
        });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            _selectedAbility = null;
            _selectedItem = null;

            EventManager.TriggerCharacterTurnStarted(currentUnit as CharacterManager);
        });
    }
}
