using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dicebound Tactics/Animation/Combat Animation Data")]
public class CombatAnimationData : ScriptableObject
{
	[BoxGroup("References"), Tooltip("Animation to play when equipping a weapon")]
	public AnimationClip equipWeapon;
	[BoxGroup("References"), Tooltip("Animation to play when unequipping a weapon")]
	public AnimationClip unequipWeapon;
	[BoxGroup("References"), Tooltip("Animation to play when aiming or readying your weapon")]
	public AnimationClip aimWeapon;
	[BoxGroup("References"), Tooltip("Animation to play when reloading your weapon")]
	public AnimationClip reloadWeapon;
	[BoxGroup("References"), Tooltip("Animation to play when an attack is triggered")]
	public AnimationClip attack;
	[BoxGroup("References"), Tooltip("Transition group to play while moving when this weapon is equipped")]
	public MixerTransition2D equippedMovement;
	[BoxGroup("Settings"), Tooltip("Normalized time which weapon is equipped to player during the animation")] public float normalizedEquipTime = 0.4f;
	[BoxGroup("Settings"), Tooltip("List of normalized times which weapon deals damage to target")] public List<float> normalizedAttackTriggerTimes = new List<float>();
}
