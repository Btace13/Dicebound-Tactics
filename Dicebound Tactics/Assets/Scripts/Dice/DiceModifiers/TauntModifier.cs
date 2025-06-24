using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/Taunt")]
public class TauntModifier : DiceModifier
{
    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.ApplyTaunt();
    }
}