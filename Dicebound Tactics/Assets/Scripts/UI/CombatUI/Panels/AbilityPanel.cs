using UnityEngine;
using DG.Tweening;
using TacticsToolkit;
using System.Collections.Generic; // Add this if not already imported for Ease type

public class AbilityPanel : CombatPanel
{
    [SerializeField] List<AbilityButton> abilityButtons = new List<AbilityButton>();

    public void PopulateAbilityPanel(CharacterManager character)
    {
        print($"Populating ability panel for {character.name} with {character.abilitiesForUse.Count} abilities.");

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= character.abilitiesForUse.Count)
            {
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            // Set up the button with the ability name and action
            abilityButtons[i].gameObject.SetActive(true); // Ensure the button is active
            abilityButtons[i].ability = character.abilitiesForUse[i].ability;

            print("Setting up ability button: " + abilityButtons[i].ability.Name);


            abilityButtons[i].SetupButton(abilityButtons[i].ability.Name, () =>
            {
                print("Using ability: " + abilityButtons[i].ability.Name);
            });
            abilityButtons[i].AnimateIn();
        }
    }
}
