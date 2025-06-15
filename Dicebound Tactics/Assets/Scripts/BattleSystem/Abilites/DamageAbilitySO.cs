using Unity;
using UnityEngine;
using TacticsToolkit;


[CreateAssetMenu(menuName = "Abilities/DamageAbility")]
public class DamageAbilitySO : AbilitySO
{
    public int damageAmount;

    public override void Execute(Entity user, Entity target)
    {
        if (user.GetStat(Stats.ActionPoints).statValue >= apCost)
        {
            user.SpendAP(apCost);
            target.TakeDamage(damageAmount);
            Debug.Log($"{user.name} used {abilityName} on {target.name} for {damageAmount} damage.");
        }
        else
        {
            Debug.Log($"{user.name} does not have enough AP for {abilityName}.");
        }
    }
}
