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
    [SerializeField] private CombatUIManager combatUIManager;

    public SelectionController SelectionController => selectionController;
    public TurnManager TurnManager => turnManager;
    public CombatUIManager CombatUIManager => combatUIManager;
    public CombatEncounter CurrentEncounter => _currentEncounter;

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
            // The CombatUIManager will automatically show the UI when EventManager.TriggerBattleStarted() is called
            combatUIManager.ShowBigNotification("Fight!", 0.5f);
        }
        else
        {
            // The CombatUIManager will automatically hide the UI when EventManager.TriggerBattleEnded() is called
            // No need to manually call HideCombatUI() anymore
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
        Debug.Log($"[CombatManager] ExecuteAction called. Current UI state: {combatUIManager?.GetCurrentState()}");
        EventManager.TriggerSelectingATarget(false);
        Debug.Log($"[CombatManager] After TriggerSelectingATarget(false). Current UI state: {combatUIManager?.GetCurrentState()}");
        
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

        // Handle item usage separately (items end turn immediately)
        if (_selectedItem != null)
        {
            Entity target = selectedTargets[0]; // Items only affect the first selected target
            bool itemUsed = currentUnit.UseCombatItem(_selectedItem, target);

            if (!itemUsed)
            {
                Debug.LogWarning($"{currentUnit.name} failed to use {_selectedItem.ItemName} on {target.name}");
            }
            else
            {
                // Items always end the character's turn (regardless of remaining AP)
                if (currentUnit is CharacterManager character)
                {
                    EventManager.TriggerCharacterTurnEnded(character);
                }
            }
            
            // Clean up
            _selectedItem = null;
            SelectionController.Instance.ClearAllSelections();
            return; // Exit early - no need for the DOTween sequence
        }

        // Handle abilities and basic attacks with DOTween sequence
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() =>
        {
            if (_selectedAbility != null)
            {
                // Show notification for ability usage
                combatUIManager.ShowNotification($"{currentUnit.name}{_selectedAbility.notifcationMessage}", 1);
            }
            else if (_selectedItem != null)
            {
                // Show notification for item usage
                combatUIManager.ShowNotification($"{currentUnit.name} used a {_selectedItem.ItemName}", 1);
            }
            else
            {
                //Show notifcation that the action is being executed
                if (selectedTargets.Count > 1)
                    combatUIManager.ShowNotification($"{currentUnit.name} is attacking {selectedTargets.Count} targets", 1);
                else
                    combatUIManager.ShowNotification($"{currentUnit.name} is attacking {selectedTargets[0].name}", 1);
            }

            // Hide the action panel during the attack sequence
            combatUIManager.FadeOutCurrentPanel();

            // set the active camera as the AttackCamera
            CameraManager.Instance?.TrySetActiveCamera(CurrentEncounter.GetCameraControllerForSide(currentUnit).name);

            //TODO: trigger animations / effects here
        });
        //TODO: the duration of the sequence should be based on the animation length
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() =>
        {
            // Execute the action with the selected targets (for abilities and basic attacks only)
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
                                combatUIManager.damageNumberUIHandler.ShowDamageNumber(damage, target.transform.position, DamageNumberType.Normal);
                                // CameraManager.Instance?.ShakeActiveCamera();
                            });
                            attackSequence.AppendInterval(0.5f);
                            attackSequence.AppendCallback(() =>
                            {
                                // return to original position
                                cc.MoveToPosition(cc.AssignedEncounterSlot.slotTransform.position, true, () =>
                                {
                                    // After returning to position, trigger the camera transition and UI update
                                    if (currentUnit is CharacterManager character)
                                    {
                                        if (!character.CanUseMoreAbilitiesThisTurn())
                                        {
                                            // Character is out of action points - end the turn
                                            EventManager.TriggerCharacterTurnEnded(character);
                                        }
                                        else
                                        {
                                            // Character still has action points - continue the turn
                                            // Switch camera back to player's side and set them as the active character
                                            if (CurrentEncounter != null)
                                            {
                                                var cameraController = CurrentEncounter.GetCameraControllerForSide(character);
                                                if (cameraController != null)
                                                {
                                                    CameraManager.Instance?.TrySetActiveCamera(cameraController.name);
                                                }
                                                else
                                                {
                                                    Debug.LogWarning($"[CombatManager] No camera controller found for character {character.name}");
                                                }
                                            }
                                            else
                                            {
                                                Debug.LogWarning("[CombatManager] CurrentEncounter is null, cannot switch camera");
                                            }
                                            
                                            // Refocus camera on the player like their turn is starting again
                                            CameraManager.Instance?.SetActiveCombatCharacter(character.transform);
                                            
                                            // Show the action panel again and move canvas back to player
                                            combatUIManager.OpenActionPanel();
                                            combatUIManager.MoveCanvasToCharacter(character);
                                            
                                            // Use the proper event to refresh the UI without restarting the turn
                                            EventManager.TriggerShowActionPanel();
                                        }
                                    }
                                });
                            });
                        });
                    }
                    else
                    {
                        int damage = currentUnit.characterClass.Strength.baseStatValue;
                        target.TakeDamage(damage);
                        combatUIManager.damageNumberUIHandler.ShowDamageNumber(damage, target.transform.position, DamageNumberType.Normal);
                        // CameraManager.Instance?.ShakeActiveCamera();
                    }
                }
            }
        });
        sequence.AppendInterval(2f);
        sequence.AppendCallback(() =>
        {
            // Store whether this was an ability before clearing it
            bool wasAbility = _selectedAbility != null;
            
            _selectedAbility = null;
            SelectionController.Instance.ClearAllSelections();
            
            // For abilities, they handle their own turn ending/continuation logic
            // For basic attacks, the camera transition is handled after the character returns to position
            // No additional logic needed here for basic attacks
        });
    }
}
