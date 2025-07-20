using System.Collections;
using Unity;
using UnityEngine;
using TacticsToolkit;
using DG.Tweening;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Abilities/DamageAbility")]
public class DamageAbilitySO : AbilitySO
{
    public int damageAmount;

    public override IEnumerator Execute(Entity user, Entity target)
    {
        int amount = user.CalculateDamageWithModifiers(damageAmount);

        OverworldEntityController userController = user.GetComponent<OverworldEntityController>();
        OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();

        if (requiresMovement)
        {
            if (userController == null || enemyController == null)
            {
                Debug.LogWarning("Missing controller — applying damage without animation.");
                ApplyDamage(amount, user, target);
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // Move toward target
            Vector3 direction = (target.transform.position - user.transform.position).normalized;
            Vector3 destination = target.transform.position - direction * range;

            bool moveDone = false;
            userController.MoveToPosition(destination, true, () => moveDone = true);
            while (!moveDone) yield return null;

            // Play attack animation and apply damage
            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, () =>
            {
                ApplyDamage(amount, user, target);
            });

            // Return to origin
            bool returnDone = false;
            userController.MoveToTarget(userController.AssignedEncounterSlot.slotTransform, true, () => returnDone = true);
            while (!returnDone) yield return null;
        }
        else
        {
            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, () =>
            {
                ApplyDamage(amount, user, target);
            });
        }
    }

    private IEnumerator TriggerAbilityAnimationSequenceCoroutine(Entity user, Entity target, UnityAction OnHitTarget)
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

        // Look at target
        userController.transform.DOLookAt(target.transform.position, 0.2f);
        yield return new WaitForSeconds(0.2f);

        // Play attack animation and wait for completion
        bool animDone = false;
        bool hitTriggered = false;
        if (animationHandler == null)
        {
            Debug.LogWarning("No UnitAnimationHandler found on the user.");
            OnHitTarget?.Invoke();
            hitTriggered = true;
            animDone = true;
        }
        else
        {
            animationHandler.UseAbility(this, time => {
                if (!hitTriggered) {
                    OnHitTarget?.Invoke();
                    hitTriggered = true;
                }
            }, () => { animDone = true; });
        }
        // Failsafe: if animation never completes, force exit after a timeout
        float timeout = Mathf.Max(clipTime, 2f);
        float timer = 0f;
        while (!animDone && timer < timeout) {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!animDone) {
            Debug.LogWarning($"Animation did not complete in time for {user.name} using {this.abilityName}. Forcing completion.");
            if (!hitTriggered) OnHitTarget?.Invoke();
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
