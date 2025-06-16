using UnityEngine;
using TacticsToolkit;

[CreateAssetMenu(menuName = "Dice Modifiers/Bonus Damage")]
public class BonusDamageModifier : DiceModifier
{
  public int bonusDamage = 2;

  public override void Apply(Entity roller, TurnManager turnManager)
  {
    
  }
  
  public float GetTotalDamage(float baseDamage)
  {
    return baseDamage + bonusDamage;
  }
}
