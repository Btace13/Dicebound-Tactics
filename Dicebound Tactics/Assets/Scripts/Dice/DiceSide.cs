using UnityEngine;

[System.Serializable]
public class DiceSide
{
    public int value; // 1-6
    public DiceModifier modifier;

    public DiceSide(int value, DiceModifier mod = null)
    {
        this.value = value;
        modifier = mod;
    }
}
