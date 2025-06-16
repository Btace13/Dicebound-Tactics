using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Dice/Dice")]
public class Dice : ScriptableObject
{
    public List<DiceSide> sides = new List<DiceSide>(6); // Always 6, ordered

    public DiceSide Roll()
    {
        int index = Random.Range(0, sides.Count);
        return sides[index];
    }
}
