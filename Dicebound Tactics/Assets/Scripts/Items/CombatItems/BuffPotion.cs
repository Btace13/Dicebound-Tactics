using UnityEngine;
using TacticsToolkit;

[CreateAssetMenu(fileName = "BuffPotion", menuName = "Items/Buff Potion")]
public class BuffPotion : CombatItem
{
    [Header("Buff Properties")]
    [Tooltip("Type of buff to apply")]
    public string buffType = "Strength"; // Could be "Strength", "Defense", "Speed", etc.
    public float buffValue = 20f; // Percentage or flat value
    public int buffDuration = 3; // Number of rounds
    
    [Header("Buff Settings")]
    public bool stackable = false; // Can multiple buffs of same type stack?
    public bool showBuffIcon = true; // Show visual indicator of buff
    
    private void OnEnable()
    {
        // Set default values for buff potions
        canTargetSelf = true;
        canTargetAllies = true;
        canTargetEnemies = false;
        canTargetDeadAllies = false;
    }
    
    public override bool UseItem(Entity user, Entity target)
    {
        if (!CanUseOn(user, target))
        {
            return false;
        }
        
        if (!target.isAlive)
            return false;
        
        // Apply the buff (using the existing temp modifier system)
        if (!stackable && target.tempModifiers.ContainsKey(buffType))
        {
            Debug.Log($"{target.name} already has a {buffType} buff active!");
            return false;
        }
        
        target.AddTempModifier(buffType, buffValue);
        
        // TODO: Add buff duration tracking when status effect system is implemented
        
        // Visual feedback - could use a different damage number type for buffs
        if (CombatManager.Instance?.CombatUIManager?.damageNumberUIHandler != null)
        {
            CombatManager.Instance.CombatUIManager.damageNumberUIHandler.ShowDamageNumber(
                (int)buffValue, 
                target.transform.position, 
                DamageNumberType.Normal // You might want to add a new type for buffs
            );
        }
        
        Debug.Log($"{user.name} used {ItemName} on {target.name}, applying {buffType} buff (+{buffValue}) for {buffDuration} rounds.");
        
        return true;
    }
}
