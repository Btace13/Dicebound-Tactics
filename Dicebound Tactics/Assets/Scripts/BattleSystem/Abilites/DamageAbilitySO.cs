using Unity;
using UnityEngine;
using TacticsToolkit;
using DG.Tweening;

[CreateAssetMenu(menuName = "Abilities/DamageAbility")]
public class DamageAbilitySO : AbilitySO
{
    public int damageAmount;

    public override void Execute(Entity user, Entity target)
    {
        if (user.SpendAP(apCost))
        {
            int amount = user.CalculateDamageWithModifiers(damageAmount);

            if (requiresMovement)
            {
                OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();
                OverworldEntityController userController = user.GetComponent<OverworldEntityController>();

                if (enemyController == null || userController == null)
                {
                    Debug.LogWarning("Either the user or target does not have an OverworldEntityController component.");
                    ApplyDamage(amount, user, target);
                    return;
                }

                Vector3 direction = (target.transform.position - user.transform.position).normalized;
                Vector3 destination = target.transform.position - direction * range; // Stop 'range' units away from the target

                userController.MoveToPosition(destination, true, () =>
                {
                    Sequence seq = DOTween.Sequence();
                    seq.Append(userController.transform.DOLookAt(target.transform.position, 0.2f));
                    seq.AppendInterval(0.1f); // Small delay to ensure the look rotation is applied before the attack
                    seq.AppendCallback(() =>
                    {
                        ApplyDamage(amount, user, target);
                    });
                    seq.AppendInterval(1); // Wait for a moment to let the player see the attack
                    seq.AppendCallback(() =>
                    {
                        // Move back to original position
                        userController.MoveToTarget(userController.AssignedEncounterSlot.slotTransform, true);
                    });
                });
            }
            else
            {
                ApplyDamage(amount, user, target);
            }
        }
        else
        {
            Debug.Log($"{user.name} does not have enough AP for {abilityName}.");
        }
    }

    public void ApplyDamage(int amount, Entity user, Entity target)
    {
        target.TakeDamage(amount);
        user.ApplyOverloadHit(amount, target);
        user.HealOnHit(amount);
        Debug.Log($"{user.name} used {abilityName} on {target.name} for {amount} damage.");
    }
}
