using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/FocusBoost")]
public class FocusBoostModifier : DiceModifier
{
    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.ApplyFocusBoost();
    }
}