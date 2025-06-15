
namespace TacticsToolkit
{
    public enum Stats
    {
        ActionPoints,
        CarriedOverActionPoints,
        Health,
        Mana,
        Strength,
        Endurance,
        Speed,
        Intelligence,
        MoveRange,
        AttackRange,
        CurrentHealth,
        CurrentMana
    }

    public enum Operation
    {
        Add,
        Minus,
        Multiply,
        Divide,
        AddByPercentage,
        MinusByPercentage
    }

    public enum TileTypes
    {
        Traversable,
        NonTraversable,
        Effect
    }

    public enum MovementDirection
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }
}