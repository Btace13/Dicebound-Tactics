using Unity;
using UnityEngine;
using TacticsToolkit;
using DG.Tweening;
using UnityEngine.Events;

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
                    TriggerAbilityAnimationSequence(user, target, () =>
                    {
                        // This callback is called when the ability hits the target
                        ApplyDamage(amount, user, target);
                    }, () =>
                    {
                        // Animation complete
                        // Move the user back to their assigned encounter slot
                        userController.MoveToTarget(userController.AssignedEncounterSlot.slotTransform, true);
                    });
                });
            }
            else
            {
                TriggerAbilityAnimationSequence(user, target, () =>
                {
                    // This callback is called when the ability hits the target
                    ApplyDamage(amount, user, target);
                });
            }
        }
        else
        {
            Debug.Log($"{user.name} does not have enough AP for {abilityName}.");
        }
    }

    public void TriggerAbilityAnimationSequence(Entity user, Entity target, UnityAction OnHitTarget, UnityAction OnAbilityAnimationComplete = null)
    {
        OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();
        OverworldEntityController userController = user.GetComponent<OverworldEntityController>();

        UnitAnimationHandler animationHandler = userController.GetComponentInChildren<UnitAnimationHandler>(true);
        float clipTime = 0;

        if (animationHandler != null && animationHandler.AnimationData != null)
        {
            if (user.TryGetComponent(out CharacterManager characterManager))
            {
                clipTime = animationHandler.AnimationData.combatAnimations[animationHandler.EquippedWeapon].abilities[this].length;
            }
        }
        else
        {
            Debug.LogWarning("No combat animations found for the equipped weapon or UnitAnimationHandler is missing.");
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(userController.transform.DOLookAt(target.transform.position, 0.2f));
        seq.AppendInterval(0.2f); // Small delay to ensure the look rotation is applied before the attack
        seq.AppendCallback(() =>
        {
            // Play attack animation
            if (animationHandler == null)
            {
                Debug.LogWarning("No UnitAnimationHandler found on the user.");
                OnHitTarget?.Invoke();
                return;
            }
            else
            {
                animationHandler.UseAbility(this, time =>
                {
                    OnHitTarget?.Invoke();
                }, () =>
                {
                    Debug.Log($"{user.name} finished {this.abilityName} animation.");
                    OnAbilityAnimationComplete?.Invoke();
                });
            }
        });
    }

    public void ApplyDamage(int amount, Entity user, Entity target)
    {
        target.TakeDamage(amount);
        user.ApplyOverloadHit(amount, target);
        user.HealOnHit(amount);
        Debug.Log($"{user.name} used {abilityName} on {target.name} for {amount} damage.");
    }
}
