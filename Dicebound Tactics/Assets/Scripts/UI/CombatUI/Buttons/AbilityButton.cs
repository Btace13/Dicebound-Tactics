using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class AbilityButton : CombatButton
{
    public AbilitySO ability;
    [SerializeField] TextMeshProUGUI abilityCostText;

    public override void AnimateIn()
    {
        base.AnimateIn();
        // Additional animation logic specific to AbilityButton
    }

    public void SetupAbilityButton(AbilitySO ability, UnityAction onClickAction, bool canUse = false)
    {
        this.ability = ability;
        abilityCostText.SetText(ability.apCost.ToString());

        SetupButton(ability.abilityName, onClickAction);

        Button.interactable = canUse;
    }

    public override void SetupButton(string text, UnityAction onClickAction)
    {
        base.SetupButton(text, onClickAction);
        // Additional setup logic specific to AbilityButton
    }
}
