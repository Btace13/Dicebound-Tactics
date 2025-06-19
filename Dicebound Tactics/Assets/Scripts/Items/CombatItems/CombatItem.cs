using UnityEngine;

[CreateAssetMenu(fileName = "NewCombatItem", menuName = "Items/Combat Item")]
public class CombatItem : Item
{
    public string ItemName => "Combat Item";
    public string Description => "A generic combat item used in battles.";
    public Sprite Icon => null; // Replace with a default icon if needed
    public int MaxStackSize => 10; // Default max stack size for combat items
}
