using UnityEngine;
using TacticsToolkit;

public abstract class DiceModifier : ScriptableObject
{
    public string Name;
    [TextArea] public string Description;

    // Called when the modifier is triggered
    public abstract void Apply(Entity user);
}
