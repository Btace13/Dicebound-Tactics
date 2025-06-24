using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/HealOnHit")]
public class HealOnHitModifier : DiceModifier
{
    public float percent = 15f;

    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.SetHealOnNextHit(percent);
    }
}