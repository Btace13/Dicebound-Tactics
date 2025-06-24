using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/Overload")]
public class OverloadModifier : DiceModifier
{
    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.ApplyOverload();
    }
}