using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Dice")]
public class Dice : ScriptableObject
{
    public List<DiceSide> sides = new(6);
    public int LastRollValue { get; private set; }
    public DiceModifier LastRollModifier { get; private set; }

    public Dice(List<DiceSide> generatedSides)
    {
        sides = generatedSides;
    }

    public DiceSide Roll()
    {
        int index = Random.Range(0, sides.Count - 1);
        LastRollValue = sides[index].value;
        LastRollModifier = sides[index].modifier;
        return sides[index];
    }
}
