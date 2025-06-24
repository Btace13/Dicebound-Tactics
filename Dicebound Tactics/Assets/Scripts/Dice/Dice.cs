using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Dice")]
public class Dice : ScriptableObject
{
    public List<DiceSide> sides = new(6);
    public int LastRollValue { get; private set; }

    public DiceSide Roll()
    {
        int index = Random.Range(1, sides.Count);
        LastRollValue = sides[index].value;
        return sides[index];
    }
}
