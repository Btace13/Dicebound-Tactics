using Unity;
using UnityEngine;
using TacticsToolkit;


[CreateAssetMenu(menuName = "Abilities/DamageAbility")]
public class DamageAbilitySO : AbilitySO
{
    public int damageAmount;

    public override void Execute(Entity user, Entity target)
    {
        if (user.SpendAP(apCost))
        {
            int amount = user.CalculateDamageWithModifiers(damageAmount);
            target.TakeDamage(amount);
            target.HealOnNextHit(amount);
            Debug.Log($"{user.name} used {abilityName} on {target.name} for {amount} damage.");
        }
        else
        {
            Debug.Log($"{user.name} does not have enough AP for {abilityName}.");
        }
    }
}
