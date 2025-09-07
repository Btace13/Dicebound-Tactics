using UnityEngine;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class ItemPanel : CombatPanel
{
    [SerializeField] List<ItemButton> itemButtons = new List<ItemButton>();
    public UnityEvent<CombatItem> OnItemClicked;
    public string cameraName = "ConfirmTargetCamera";
    private CombatEncounter _currentEncounter;

    private void Awake() {
        EventManager.OnCombatEncounterStarted += encounter => _currentEncounter = encounter;
        EventManager.OnCombatEncounterEnded += encounter => _currentEncounter = null;
    }

    public void PopulateItemPanel(CharacterManager character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterManager is null. Cannot populate Item Panel.");
            return;
        }

        if (character.inventory == null)
        {
            Debug.LogError("Character's inventory is null. Cannot populate Item Panel.");
            return;
        }

        foreach (var button in itemButtons)
        {
            button.gameObject.SetActive(false);
        }

        // Get valid items (excluding null entries)
        var validItems = character.inventory.GetValidItems();
        var itemList = validItems.ToList();

        for (int i = 0; i < itemButtons.Count && i < itemList.Count; i++)
        {
            var itemEntry = itemList[i];
            CombatItem currentItem = itemEntry.Key;
            int itemCount = itemEntry.Value;

            // Additional null check
            if (currentItem == null)
            {
                Debug.LogWarning($"Skipping null item at index {i} in item panel.");
                continue;
            }

            // Set up the button properties
            itemButtons[i].gameObject.SetActive(true);

            // Check if character can use this item
            bool canUse = character.CanUseItemThisTurn() &&
                         currentItem.CanUseOn(character, character) &&
                         itemCount > 0;

            itemButtons[i].SetupItemButton(
                currentItem,
                itemCount,
                () =>
                {
                    try
                    {
                        // SelectionController.Instance.ChangeSelectionType(!currentItem.canTargetAllies || !currentItem.canTargetSelf);

                        // // Safe camera switching with null checks
                        // if (_currentEncounter != null && TurnManager.Instance.enemyUnits != null && TurnManager.Instance.enemyUnits.Count > 0)
                        // {
                        //     var cameraController = _currentEncounter.GetCameraControllerForSide(TurnManager.Instance.enemyUnits[0]);
                        //     if (cameraController != null)
                        //     {
                        //         CameraManager.Instance.TrySetActiveCamera(cameraController.name);
                        //     }
                        //     else
                        //     {
                        //         Debug.LogWarning("No camera controller found for the enemy side. Skipping camera switch.");
                        //     }
                        // }
                        // else
                        // {
                        //     Debug.LogWarning("Current encounter or enemy units are null/empty. Skipping camera switch.");
                        // }
                        
                        CameraManager.Instance.TrySetActiveCamera(cameraName);
                        CombatManager.Instance.ItemSelected(currentItem);
                        OnItemClicked?.Invoke(currentItem);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error when using button for item {currentItem.ItemName}: {ex.Message}");
                        Debug.LogError($"Stack trace: {ex.StackTrace}");
                    }
                },
                canUse
            );

            itemButtons[i].AnimateIn();
        }
    }

    /// <summary>
    /// Update all item button states for the current character
    /// </summary>
    public void UpdateItemButtonStates(Entity currentEntity)
    {
        foreach (var button in itemButtons)
        {
            if (button.gameObject.activeInHierarchy)
            {
                button.UpdateButtonState(currentEntity);
            }
        }
    }
}
