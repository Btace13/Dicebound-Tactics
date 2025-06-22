using TacticsToolkit;
using UnityEngine.Events;

public class AbilityButton : CombatButton
{
    public AbilitySO ability;

    public override void AnimateIn()
    {
        base.AnimateIn();
        // Additional animation logic specific to AbilityButton
    }

    public override void SetupButton(string text, UnityAction onClickAction)
    {
        base.SetupButton(text, onClickAction);
        // Additional setup logic specific to AbilityButton
    }
}
