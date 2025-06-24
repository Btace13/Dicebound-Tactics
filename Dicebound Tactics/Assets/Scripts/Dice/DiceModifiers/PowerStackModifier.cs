using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/PowerStack")]
public class PowerStackModifier : DiceModifier
{
    public int percent = 5;
    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.AddPowerStack(percent);
    }
}