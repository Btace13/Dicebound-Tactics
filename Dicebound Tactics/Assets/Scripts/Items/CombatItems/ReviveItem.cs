using UnityEngine;
using TacticsToolkit;

[CreateAssetMenu(fileName = "ReviveItem", menuName = "Items/Revive Item")]
public class ReviveItem : CombatItem
{
    [Header("Revive Properties")]
    public int reviveHealthAmount = 0; // Fixed health amount on revive
    public float reviveHealthPercentage = 25f; // Percentage of max health on revive
    
    [Header("Revive Item Settings")]
    [Tooltip("Restore mana when reviving")]
    public bool restoreMana = false;
    public int manaAmount = 0;
    public float manaPercentage = 0f;
    
    [Tooltip("Apply temporary buffs after revival")]
    public bool applyRevivalBuff = false;
    public int buffDuration = 3; // rounds
    public float damageReduction = 50f; // percentage
    
    private void OnEnable()
    {
        // Set default values for revive items
        canTargetSelf = false; // Usually can't revive yourself
        canTargetAllies = true;
        canTargetEnemies = false;
        canTargetDeadAllies = true; // This is key for revive items
    }
    
    public override bool UseItem(Entity user, Entity target)
    {
        if (!CanUseOn(user, target))
        {
            return false;
        }
        
        if (target.isAlive)
            return false;
            
        // Revive the target
        target.isAlive = true;
        target.gameObject.SetActive(true);
        
        // Set health based on heal amount/percentage
        int reviveHealth = reviveHealthAmount;
        if (reviveHealthPercentage > 0f)
        {
            reviveHealth = Mathf.RoundToInt((reviveHealthPercentage / 100f) * target.GetStat(Stats.Health).statValue);
        }
        
        // Ensure minimum health of 1 if no specific amount is set
        if (reviveHealth <= 0)
        {
            reviveHealth = 1;
        }
        
        target.GetStat(Stats.CurrentHealth).statValue = reviveHealth;
        target.InvokeCharacterStatChanged();
        
        // Restore mana if specified
        if (restoreMana)
        {
            int totalManaAmount = manaAmount;
            
            if (manaPercentage > 0f)
            {
                int percentageMana = Mathf.RoundToInt((manaPercentage / 100f) * target.GetStat(Stats.Mana).statValue);
                totalManaAmount += percentageMana;
            }
            
            if (totalManaAmount > 0)
            {
                target.GetStat(Stats.CurrentMana).statValue = Mathf.Min(
                    target.GetStat(Stats.CurrentMana).statValue + totalManaAmount,
                    target.GetStat(Stats.Mana).statValue
                );
            }
        }
        
        // Apply revival buff
        if (applyRevivalBuff)
        {
            // TODO: Add temporary buff when status effect system is implemented
            target.AddTempModifier("DamageReduction", damageReduction);
        }
        
        // Show heal damage number
        if (CombatManager.Instance?.CombatUIManager?.damageNumberUIHandler != null)
        {
            CombatManager.Instance.CombatUIManager.damageNumberUIHandler.ShowDamageNumber(
                reviveHealth, 
                target.transform.position, 
                DamageNumberType.Heal
            );
        }

        return true;
    }
}
