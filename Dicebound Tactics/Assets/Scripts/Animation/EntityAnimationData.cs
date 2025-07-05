using UnityEngine;
using Sirenix.OdinInspector;
using Animancer;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EntityAnimationData", menuName = "Dicebound Tactics/Animation/Entity Animation Data", order = 1)]
public class EntityAnimationData : ScriptableObject
{
    [BoxGroup("Movement Animations")] public MixerTransition2D movementAnimations;
    [BoxGroup("Movement Animations")] public List<AnimationClip> idleAnimations = new List<AnimationClip>();
    [BoxGroup("Movement Animations")] public bool CanSwim = false;
    [BoxGroup("Movement Animations"), ShowIf("CanSwim")] public MixerTransition2D swimmingAnimations;
    [BoxGroup("Movement Animations")] public bool CanFly = false;
    [BoxGroup("Movement Animations"), ShowIf("CanFly")] public MixerTransition2D flyingAnimations;
    [BoxGroup("Movement Animations")] public bool CanJump = false;
    [BoxGroup("Movement Animations"), ShowIf("CanJump")] public AnimationClip jumpAnimation;
    [BoxGroup("Movement Animations"), ShowIf("CanJump")] public AnimationClip fallingAnimation;
    [BoxGroup("Movement Animations"), ShowIf("CanJump")] public AnimationClip landingAnimation;

    [BoxGroup("Combat Animations")] public bool CanFight = true;
    [BoxGroup("Combat Animations"), ShowIf("CanFight")] public UDictionary<WeaponData, CombatAnimationData> combatAnimations = new UDictionary<WeaponData, CombatAnimationData>();
    [BoxGroup("Combat Animations"), ShowIf("CanFight")] public AnimationClip hitAnimation;
    [BoxGroup("Combat Animations"), ShowIf("CanFight")] public AnimationClip deathAnimation;
}