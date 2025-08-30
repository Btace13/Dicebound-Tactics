using UnityEngine;
using TacticsToolkit;

[CreateAssetMenu(fileName = "HealPotion", menuName = "Items/Heal Potion")]
public class HealPotion : CombatItem
{
    [Header("Healing Properties")]
    public int healAmount = 25; // Fixed heal amount
    public float healPercentage = 0f; // Percentage of max health to heal
    
    [Header("Heal Potion Settings")]
    [Tooltip("Additional healing effects or special properties")]
    public bool removeDebuffs = false;
    public bool healOverTime = false;
    public int healOverTimeDuration = 3; // rounds
    public int healOverTimeAmount = 5; // per round
    
    private void OnEnable()
    {
        // Set default values for heal potions
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
            
        int totalHealAmount = healAmount;
        
        // Add percentage-based healing
        if (healPercentage > 0f)
        {
            int percentageHeal = Mathf.RoundToInt((healPercentage / 100f) * target.GetStat(Stats.Health).statValue);
            totalHealAmount += percentageHeal;
        }
        
        if (totalHealAmount > 0)
        {
            target.HealEntity(totalHealAmount);
            
            // Show heal damage number
            if (CombatManager.Instance?.CombatUIHandler?.damageNumberUIHandler != null)
            {
                CombatManager.Instance.CombatUIHandler.damageNumberUIHandler.ShowDamageNumber(
                    totalHealAmount, 
                    target.transform.position, 
                    DamageNumberType.Heal
                );
            }
            
            // Apply special effects
            if (removeDebuffs)
            {
                // TODO: Add debuff removal logic when debuff system is implemented
                Debug.Log($"Removed debuffs from {target.name}");
            }
            
            if (healOverTime)
            {
                // TODO: Add heal over time effect when status effect system is implemented
                Debug.Log($"Applied heal over time to {target.name}: {healOverTimeAmount} HP for {healOverTimeDuration} rounds");
            }
            
            Debug.Log($"{user.name} used {ItemName} on {target.name}, healing for {totalHealAmount} HP.");
            return true;
        }
        
        return false;
    }
}
