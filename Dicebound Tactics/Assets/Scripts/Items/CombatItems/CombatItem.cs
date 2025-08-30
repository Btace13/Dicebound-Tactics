using UnityEngine;
using TacticsToolkit;

public abstract class CombatItem : Item
{
    [Header("Targeting")]
    public bool canTargetSelf = true;
    public bool canTargetAllies = true;
    public bool canTargetEnemies = false;
    public bool canTargetDeadAllies = false; // For revive items
    
    /// <summary>
    /// Uses the combat item on the specified target
    /// </summary>
    /// <param name="user">The entity using the item</param>
    /// <param name="target">The target entity</param>
    /// <returns>True if the item was successfully used</returns>
    public abstract bool UseItem(Entity user, Entity target);
    
    /// <summary>
    /// Checks if the item can be used on the target
    /// </summary>
    public virtual bool CanUseOn(Entity user, Entity target)
    {
        if (user == null || target == null)
            return false;
            
        // Check if user has already used an item this turn
        if (!user.CanUseItemThisTurn())
            return false;
            
        // Check targeting rules
        bool isSelf = user == target;
        bool isAlly = user.teamID == target.teamID && !isSelf;
        bool isEnemy = user.teamID != target.teamID;
        bool isTargetDead = !target.isAlive;
        
        if (isSelf && !canTargetSelf)
            return false;
            
        if (isAlly && !canTargetAllies)
            return false;
            
        if (isEnemy && !canTargetEnemies)
            return false;
            
        // Check if targeting dead allies is allowed (for revive items)
        if (isTargetDead && !canTargetDeadAllies)
            return false;
            
        // Most items can't target dead entities unless specifically allowed
        if (isTargetDead && !canTargetDeadAllies)
            return false;
        
        return true;
    }
}
