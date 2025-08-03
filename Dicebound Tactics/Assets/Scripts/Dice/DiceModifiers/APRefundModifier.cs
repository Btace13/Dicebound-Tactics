using UnityEngine;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/APRefund")]
public class APRefundModifier : DiceModifier
{
    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.SetNextAbilityFree();
    }
}