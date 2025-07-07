using UnityEngine;
using TacticsToolkit;

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
    public AbilityType abilityType = AbilityType.All;
    public bool requiresMovement = false;
    public float range = 4f;
    public int unlockLevel = 1;

    public abstract void Execute(Entity user, Entity target);
}
