using UnityEngine;
using TacticsToolkit;

public abstract class DiceModifier : ScriptableObject
{
    public string modifierName;
    [TextArea] public string description;

    // Called when the modifier is triggered
    public abstract void Apply(Entity user);
}
