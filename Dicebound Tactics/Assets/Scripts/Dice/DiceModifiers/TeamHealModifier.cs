using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/TeamHeal")]
public class TeamHealModifier : DiceModifier
{
    public float percent = 10f;

    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        user.HealTeamByPercentage(user.teamID, percent);
    }
}