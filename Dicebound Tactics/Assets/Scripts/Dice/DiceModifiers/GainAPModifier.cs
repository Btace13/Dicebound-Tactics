using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/Gain1AP")]
public class GainAPModifier : DiceModifier
{
    public int modifierValue = 1;
    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.AddActionPoints(modifierValue);
    }
}