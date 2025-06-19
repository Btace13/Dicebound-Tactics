using UnityEngine;
using DG.Tweening;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEngine.Events; // Add this if not already imported for Ease type

public class AbilityPanel : CombatPanel
{
    [SerializeField] List<AbilityButton> abilityButtons = new List<AbilityButton>();
    public UnityEvent<Ability> OnAbilitySelected;

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

            Ability ability = character.abilitiesForUse[i].ability;

            // Set up the button with the ability name and action
            abilityButtons[i].gameObject.SetActive(true); // Ensure the button is active
            abilityButtons[i].ability = ability;


            print("Setting up ability button: " + ability.Name);

            // Set the button text and action
            abilityButtons[i].SetupButton(ability.Name, () =>
            {
                print("Using ability: " + ability.Name);
                OnAbilitySelected?.Invoke(ability);
            });
            abilityButtons[i].AnimateIn();
        }
    }
}
