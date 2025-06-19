using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Basic Item")]
public class Item : ScriptableObject
{
    public string ItemName = "New Item";
    public string Description = "Item Description";
    public Sprite Icon; // Icon for the item
    public int MaxStackSize = 99; // Maximum number of items that can be stacked
}
