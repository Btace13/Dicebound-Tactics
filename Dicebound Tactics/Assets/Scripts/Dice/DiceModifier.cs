using UnityEngine;
using TacticsToolkit;

public abstract class DiceModifier : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    [TextArea] public string Description;

    // Called when the modifier is triggered
    public abstract void Apply(Entity user);
}
