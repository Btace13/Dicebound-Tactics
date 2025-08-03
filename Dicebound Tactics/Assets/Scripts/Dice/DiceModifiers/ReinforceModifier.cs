using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/Reinforce")]
public class ReinforceModifier : DiceModifier
{
    public float defenseBoost = 25f;
    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.ApplyTemporaryDefenseBuff(defenseBoost);
    }
}