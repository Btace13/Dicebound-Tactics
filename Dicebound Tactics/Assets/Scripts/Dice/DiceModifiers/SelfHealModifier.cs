using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/SelfHeal")]
public class SelfHealModifier : DiceModifier
{
    public float percent = 10f;

    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.HealEntityByPercentage(percent);
    }
}