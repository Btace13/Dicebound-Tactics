using UnityEngine;
using DG.Tweening;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEngine.Events; // Add this if not already imported for Ease type

public class AbilityPanel : CombatPanel
{
    [SerializeField] List<AbilityButton> abilityButtons = new List<AbilityButton>();
    public UnityEvent<AbilitySO> OnAbilitySelected;

    public void PopulateAbilityPanel(CharacterManager character)
    {
        print($"Populating ability panel for {character.name} with {character.abilities.Count} abilities.");

        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= character.abilities.Count)
            {
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            AbilitySO ability = character.abilities[i];

            // Set up the button with the ability name and action
            abilityButtons[i].gameObject.SetActive(true); // Ensure the button is active
            abilityButtons[i].ability = ability;


            print("Setting up ability button: " + ability.abilityName);

            // Set the button text and action
            abilityButtons[i].SetupButton(ability.abilityName, () =>
            {
                print("Using ability: " + ability.abilityName);
                OnAbilitySelected?.Invoke(ability);
            });
            abilityButtons[i].AnimateIn();
        }
    }
}
