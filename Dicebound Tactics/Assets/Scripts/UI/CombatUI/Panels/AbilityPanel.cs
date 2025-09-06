using UnityEngine;
using DG.Tweening;
using TacticsToolkit;
using System.Collections.Generic;
using UnityEngine.Events; // Add this if not already imported for Ease type

public class AbilityPanel : CombatPanel
{
    [SerializeField] List<AbilityButton> abilityButtons = new List<AbilityButton>();
    public UnityEvent<AbilitySO> OnAbilitySelected;
    public string cameraName = "ConfirmTargetCamera";

    public void PopulateAbilityPanel(CharacterManager character)
    {
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            if (i >= character.abilityLoadout.Count)
            {
                abilityButtons[i].gameObject.SetActive(false);
                continue;
            }

            AbilitySO ability = character.abilityLoadout[i];

            // Set up the button with the ability name and action
            abilityButtons[i].gameObject.SetActive(true); // Ensure the button is active
            abilityButtons[i].ability = ability;

            // Set the button text and action
            abilityButtons[i].SetupAbilityButton(ability, () =>
            {
                OnAbilitySelected?.Invoke(ability);
                EventManager.TriggerSelectingATarget(ability.abilityType == AbilityType.Enemy);
                CameraManager.Instance.TrySetActiveCamera(cameraName);
                CombatManager.Instance.AbilitySelected(ability);
            }, character.HasEnoughApToUseAbility(ability));
            abilityButtons[i].AnimateIn();
        }
    }
}
