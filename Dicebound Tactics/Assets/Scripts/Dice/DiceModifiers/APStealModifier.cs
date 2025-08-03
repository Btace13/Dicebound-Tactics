using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/APSteal")]
public class APStealModifier : DiceModifier
{
    public int amount = 1;

    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        var enemies = GameObject.FindObjectsByType<Entity>(FindObjectsSortMode.None).Where(e => e.teamID != user.teamID && e.isAlive).ToList();
        if (enemies.Count > 0)
        {
            var target = enemies[Random.Range(0, enemies.Count)];
            target.SpendAP(amount);
            user.AddActionPoints(amount);
        }
    }
}