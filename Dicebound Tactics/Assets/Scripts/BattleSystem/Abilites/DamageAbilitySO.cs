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
    private Vector3 failSafePosition = Vector3.zero;

    public override IEnumerator Execute(Entity user, Entity target)
    {
        if (!user.SpendAP(apCost))
        {
            yield break; // Not enough AP, exit early
        }

        user.InvokeCharacterStatChanged();

        int amount = user.CalculateDamageWithModifiers(damageAmount);

        OverworldEntityController userController = user.GetComponent<OverworldEntityController>();
        OverworldEntityController enemyController = target.GetComponent<OverworldEntityController>();

        if (requiresMovement)
        {
            if (userController == null || enemyController == null)
            {
                ApplyDamage(amount, user, target);
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // Move toward target
            Vector3 direction = (target.transform.position - user.transform.position).normalized;
            Vector3 destination = target.transform.position - direction * range;

            bool moveDone = false;
            failSafePosition = user.transform.position;
            userController.MoveToPosition(destination, true, () => moveDone = true);
            while (!moveDone) yield return null;

            // Play attack animation and apply damage
            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, () =>
            {
                ApplyDamage(amount, user, target);
            });

            // Return to origin
            bool returnDone = false;
            userController.MoveToPosition(
                userController?.AssignedEncounterSlot?.slotTransform != null
                    ? userController.AssignedEncounterSlot.slotTransform.position
                    : failSafePosition,
                true, () =>
            {
                returnDone = true;
                if (user is CharacterManager character)
                {
                    EventManager.TriggerCharacterTurnStarted(character);
                }
            });
            while (!returnDone) yield return new WaitForSeconds(1.6f);
        }
        else
        {

            yield return TriggerAbilityAnimationSequenceCoroutine(user, target, () =>
            {
                ApplyDamage(amount, user, target);
            });
            yield return new WaitForSeconds(1.6f);
            if (user is CharacterManager character)
            {
                EventManager.TriggerCharacterTurnStarted(character);
            }
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

        // Look at target
        userController.transform.DOLookAt(target.transform.position, 0.2f);
        yield return new WaitForSeconds(0.2f);

        // Play attack animation and wait for completion
        bool animDone = false;
        bool hitTriggered = false;

        if (animationHandler == null)
        {
            OnHitTarget?.Invoke();
            hitTriggered = true;
            animDone = true;
        }
        else
        {
            animationHandler.UseAbility(this, time =>
            {
                if (!hitTriggered)
                {
                    OnHitTarget?.Invoke();
                    hitTriggered = true;
                }
            }, () => { animDone = true; });

            if (ProjectileManager.Instance != null && projectileData != null)
            {
                Transform projectileSpawnPoint = user.transform;

                if (user.vfxSpawnPoints.ContainsKey(projectileData.projectileSpawnPoint))
                {
                    projectileSpawnPoint = user.vfxSpawnPoints[projectileData.projectileSpawnPoint];
                }

                Vector3 direction = (target.transform.position - projectileSpawnPoint.position).normalized;

                if (projectileData.castVFXObject != null)
                {
                    GameObject castVFX = Instantiate(projectileData.castVFXObject, projectileSpawnPoint.position, Quaternion.LookRotation(direction));
                    castVFX.transform.localScale = projectileData.castVFXScale; // Adjust scale as needed
                }

                yield return new WaitForSeconds(projectileData.castTime);

                ProjectileManager.CreateProjectile(
                    projectileSpawnPoint.position,
                    direction,
                    Vector3.one * 0.4f,
                    projectileData,
                    projectileData.projectilePath,
                    projectileData.projectileSpeed,
                    damageAmount,
                    -1,
                    null,
                    projectileData.particleObject,
                    "",
                    target.transform
                );
            }
        }
        // Failsafe: if animation never completes, force exit after a timeout
        float timeout = Mathf.Max(clipTime, 2f);
        float timer = 0f;
        while (!animDone && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (!animDone)
        {
            if (!hitTriggered) OnHitTarget?.Invoke();
        }
    }

    public void ApplyDamage(int amount, Entity user, Entity target)
    {
        target.TakeDamage(amount);
        user.ApplyOverloadHit(amount, target);
        user.HealOnHit(amount);
    }
}
