using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/Reinforce")]
public class ReinforceModifier : DiceModifier
{
    public float defenseBoost = 0.25f; // 25% defense boost
    public override void Apply(Entity user)
    {
        if (user == null || !user.isAlive) return;
        user.ApplyTemporaryDefenseBuff(defenseBoost);
    }
}