using UnityEngine;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEngine.Events;

public class ItemPanel : CombatPanel
{
    [SerializeField] List<ItemButton> itemButtons = new List<ItemButton>();
    public UnityEvent<CombatItem> OnItemClicked;

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

        print($"Populating item panel for {character.name} with {character.inventory.combatItems.Count} items.");

        foreach (var button in itemButtons)
        {
            button.gameObject.SetActive(false);
        }

        for (int i = 0; i < itemButtons.Count; i++)
        {
            if (i < character.inventory.combatItems.Count)
            {
                // Create a local copy to capture in the closure
                CombatItem currentItem = character.inventory.combatItems.Keys[i];
                int itemCount = character.inventory.combatItems[currentItem];

                // Set up the button properties
                itemButtons[i].combatItem = currentItem;
                itemButtons[i].gameObject.SetActive(true);
                itemButtons[i].Button.interactable = itemCount > 0;

                itemButtons[i].SetupButton(
                    $"{currentItem.ItemName} ({itemCount}/{currentItem.MaxStackSize})",
                    () =>
                    {
                        try
                        {
                            print("Item button clicked: " + currentItem.ItemName);
                            OnItemClicked?.Invoke(currentItem);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError($"Error when using button for item {currentItem.ItemName}: {ex.Message}");
                        }
                    }
                );
            }
        }
    }
}
