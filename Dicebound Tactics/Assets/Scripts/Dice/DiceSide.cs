using UnityEngine;

[System.Serializable]
public class DiceSide
{
    public int value; // 1-6
    public DiceModifier modifier;

    public DiceSide(DiceModifier mod = null)
    {
        modifier = mod;
    }
}
