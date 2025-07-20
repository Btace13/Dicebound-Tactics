using UnityEngine;
using Animancer;
using System.Collections;
using System;
using FIMSpace.FLook;
using Sirenix.OdinInspector;

public class UnitAnimationHandler : MonoBehaviour
{
	public AnimancerComponent _Animancer;
	public EntityAnimationData AnimationData;

	[BoxGroup("References"), SerializeField] Transform rightHandTransform;
	[BoxGroup("References"), SerializeField] FLookAnimator lookAnimator;

	[BoxGroup("Settings"), SerializeField] bool debug = false;

	public bool IsInCover { get; private set; } = false;
	public bool IsAiming { get; private set; } = false;

	private WeaponData _equippedWeapon = null;
	public WeaponData EquippedWeapon
	{
		get { return _equippedWeapon; }
		set
		{
			if (_equippedWeapon != value)
			{
				ToggleEquipWeapon(value);
			}
		}
	}

	private GameObject _weaponObject = null;
	private Transform _lookAt = null;

	private void Awake()
	{
		if (AnimationData != null)
		{
			AnimationData = ScriptableObject.Instantiate(AnimationData);
		}
	}

	private void Start()
	{
		AnimationData.movementAnimations.CreateState();

		//need to create weapon data states
		foreach (CombatAnimationData data in AnimationData.combatAnimations.Values)
		{
			data.equippedMovement.CreateState();
		}

		_lookAt = new GameObject($"{transform.parent.name}_LookAt").transform;

		if (lookAnimator)
		{
			lookAnimator.SetLookTarget(_lookAt);
			lookAnimator.enabled = false;
		}

		OnUnitVelocityChange(Vector2.zero); // for starting in idle at least
	}

	public void OnUnitVelocityChange(Vector2 velocity)
	{
		if (debug)
			print($"OnVelocityChanged: {velocity}");

		if (_equippedWeapon == null && AnimationData.movementAnimations.State != null)
		{
			// If no weapon is equipped, play the movement animation
			AnimationData.movementAnimations.State.Parameter = velocity;
			_Animancer.Play(AnimationData.movementAnimations, 0.1f, FadeMode.FixedDuration);
		}
		else if (AnimationData.combatAnimations.ContainsKey(_equippedWeapon))
		{
			MixerTransition2D mixer = AnimationData.combatAnimations[_equippedWeapon].equippedMovement;

			mixer.State.Parameter = velocity;
			_Animancer.Play(mixer, 0.1f, FadeMode.FixedDuration);
		}
	}

	public void OnCoverStatusChanged(bool enteredCover)
	{
		IsInCover = enteredCover;

		_Animancer.TryPlay(IsInCover ? "EnterCover" : "ExitCover", 0.25f, FadeMode.FixedDuration);
	}

	public void ShouldReadyWeapon(bool readyWeapon)
	{
		IsAiming = readyWeapon;

		if (AnimationData.combatAnimations.ContainsKey(_equippedWeapon))
		{
			AnimancerState state = _Animancer.Play(AnimationData.combatAnimations[_equippedWeapon].aimWeapon, 0.2f);
			state.NormalizedTime = readyWeapon ? 1 : 0;
			state.NormalizedEndTime = readyWeapon ? 0 : 1;
			state.Speed = readyWeapon ? 1 : -1;
		}
	}

	public void SetLookAtTarget(Transform target, float xOffset = 0, float yOffset = 0, float zOffset = 0)
	{
		if (_lookAt == null) return;

		_lookAt.position = target.position + new Vector3(xOffset, yOffset, zOffset);

		if (lookAnimator)
		{
			lookAnimator.SetLookTarget(_lookAt);
			lookAnimator.enabled = true;
		}
	}

	public void CancelLookAt()
	{
		lookAnimator.SetLookTarget(null);
		lookAnimator.enabled = false;
	}

	public void ToggleEquipWeapon(WeaponData weaponData = null)
	{
		WeaponData previousWeapon = _equippedWeapon;

		_equippedWeapon = weaponData;

		print("Weapon Type: " + _equippedWeapon);

		if (_equippedWeapon != null && AnimationData.combatAnimations.ContainsKey(_equippedWeapon))
		{
			_Animancer.TryPlay(AnimationData.combatAnimations[weaponData].equipWeapon, 0.25f, FadeMode.FixedDuration);
			StartCoroutine(ToggleWeaponParented(weaponData != null, AnimationData.combatAnimations[weaponData].normalizedEquipTime, weaponData));
		}
		else if (previousWeapon != null && AnimationData.combatAnimations.ContainsKey(previousWeapon))
		{
			_Animancer.TryPlay(AnimationData.combatAnimations[previousWeapon].unequipWeapon, 0.25f, FadeMode.FixedDuration);
			StartCoroutine(ToggleWeaponParented(false, AnimationData.combatAnimations[previousWeapon].normalizedEquipTime));
		}
	}

	private IEnumerator ForceCompleteAfterDelay(float delay, Action callback)
	{
			yield return new WaitForSeconds(delay);
			callback?.Invoke();
	}

	public void UseAbility(AbilitySO ability, Action<float> OnAttackAnimationPlayed = null, Action OnAttackAnimationComplete = null)
	{
			if (_equippedWeapon == null)
			{
					return;
			}

			if (AnimationData.combatAnimations.ContainsKey(_equippedWeapon) && AnimationData.combatAnimations[_equippedWeapon].abilities.ContainsKey(ability))
			{
					var clip = AnimationData.combatAnimations[_equippedWeapon].abilities[ability];
					float duration = clip.length / _Animancer.Play(clip).Speed;

					AnimancerEvent.Sequence events = new AnimancerEvent.Sequence(
							1 + AnimationData.combatAnimations[_equippedWeapon].normalizedAttackTriggerTimes.Count
					);

					foreach (float t in AnimationData.combatAnimations[_equippedWeapon].normalizedAttackTriggerTimes)
					{
							events.Add(t, () => OnAttackAnimationPlayed?.Invoke(t));
					}

					bool finished = false;
					events.Add(1, () => {
							if (!finished)
							{
									finished = true;
									OnAttackAnimationComplete?.Invoke();
							}
					});

					AnimancerState state = _Animancer.Layers[2].Play(clip, 0.1f, FadeMode.FixedDuration);
					state.Events = events;
					state.NormalizedTime = 0;
					state.NormalizedEndTime = 1;

					// fallback timeout in case OnAttackAnimationComplete doesn't fire
					StartCoroutine(ForceCompleteAfterDelay(duration + 0.2f, () =>
					{
							if (!finished)
							{
									finished = true;
									OnAttackAnimationComplete?.Invoke();
							}
					}));
			}
			else
			{
					OnAttackAnimationPlayed?.Invoke(1);
					OnAttackAnimationComplete?.Invoke();
			}
	}

	public void Damage()
	{
		if (!AnimationData.CanFight) return;
		if (AnimationData.hitAnimation == null)
		{
			return;
		}

		AnimancerState state = _Animancer.Play(AnimationData.hitAnimation, 0.1f, FadeMode.FixedDuration);
		state.NormalizedTime = 0;
		state.NormalizedEndTime = 1;
	}

	IEnumerator ToggleWeaponParented(bool equipped, float equipTime, WeaponData weaponData = null)
	{
		if (rightHandTransform == null) yield break;

		yield return new WaitForSeconds(equipTime);

		if (equipped)
		{
			if (weaponData == null)
			{
				yield break;
			}

			if (weaponData.ItemPrefab)
			{ 
				_weaponObject = Instantiate(weaponData.ItemPrefab, rightHandTransform);
				_weaponObject.transform.localPosition = weaponData.PositionOffset;
				_weaponObject.transform.localEulerAngles = weaponData.RotationOffset;
			}
		}
		else
		{
			if (_weaponObject)
				Destroy(_weaponObject);
		}
	}
}
