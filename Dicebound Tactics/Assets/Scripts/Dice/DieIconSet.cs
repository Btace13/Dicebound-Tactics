using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Die Icon Set", menuName = "Dice/Die Icon Set")]
public class DieIconSet : ScriptableObject
{
    public string dieName;
    public List<Sprite> faceIcons = new List<Sprite>();

    // Get icon for a specific value (1-based)
    public Sprite GetIconForValue(int value)
    {
        if (value < 1 || value > faceIcons.Count)
        {
            Debug.LogWarning($"Value {value} out of range for die: {dieName}");
            return null;
        }
        return faceIcons[value - 1]; // index 0 = value 1
    }
}
