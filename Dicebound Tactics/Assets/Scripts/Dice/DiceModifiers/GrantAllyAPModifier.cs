using UnityEngine;
using System.Linq;
using TacticsToolkit;

[CreateAssetMenu(menuName = "DiceModifiers/GrantAllyAP")]
public class GrantAllyAPModifier : DiceModifier
{
    public int amount = 1;

    public override void Apply(Entity user)
    {
        base.Apply(user);

        if (user == null || !user.isAlive) return;
        var allies = GameObject.FindObjectsByType<Entity>(FindObjectsSortMode.None).Where(e => e.teamID == user.teamID && e != user && e.isAlive).ToList();
        if (allies.Count > 0)
        {
            var target = allies[Random.Range(0, allies.Count)];
            target.AddActionPoints(amount);
            Debug.Log($"{user.name} grants {amount} AP to {target.name}");
        }
    }
}