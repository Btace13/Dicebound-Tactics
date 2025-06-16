using UnityEngine;
using TacticsToolkit;

public abstract class DiceModifier : ScriptableObject
{
    public string modifierName;

    public abstract void Apply(Entity roller, TurnManager turnManager);
}
