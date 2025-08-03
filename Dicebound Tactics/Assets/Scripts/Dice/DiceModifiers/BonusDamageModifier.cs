using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/BonusDamage")]
public class BonusDamageModifier : DiceModifier
{
    public float bonusPercent = 10f;

    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.AddTempModifier("BonusDamage", bonusPercent);
    }
}