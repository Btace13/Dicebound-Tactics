using UnityEngine.Events;

public class ItemButton : CombatButton
{
    public CombatItem combatItem;

    public override void AnimateIn()
    {
        base.AnimateIn();
        // Additional animation logic specific to ItemButton can be added here
    }

    public override void SetupButton(string text, UnityAction onClickAction)
    {
        base.SetupButton(text, onClickAction);
        // Additional setup logic specific to ItemButton can be added here
    }
}
