 using UnityEngine;
using System.Collections.Generic;

public class DiceResult
{
  public int apRolled;
  public List<DiceModifier> modifiers;

  public DiceResult(int apRolled, List<DiceModifier> modifiers)
  {
    this.apRolled = apRolled;
    this.modifiers = modifiers;
  }
}
