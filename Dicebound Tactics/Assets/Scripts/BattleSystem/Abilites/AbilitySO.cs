using UnityEngine;
using TacticsToolkit;
using System.Collections;
using Sirenix.OdinInspector;

public enum AbilityType
{
    All,
    Ally,
    Enemy,
    Self
}

public abstract class AbilitySO : ScriptableObject
{
    public string abilityName;
    public string description;
    public string notifcationMessage;
    public Sprite icon;
    public int apCost;
    public float cooldown = -1f; // -1 means no cooldown
    public AbilityType abilityType = AbilityType.All;
    public bool requiresMovement = false;
    [ShowIf("@!requiresMovement")]
    public ParticleData projectileData; // Data for projectile abilities
    public float range = 4f;
    public int unlockLevel = 1;

    public abstract IEnumerator Execute(Entity user, Entity target);
}
