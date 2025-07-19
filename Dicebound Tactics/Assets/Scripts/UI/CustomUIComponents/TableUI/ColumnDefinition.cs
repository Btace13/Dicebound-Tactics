using UnityEngine;
using TMPro;

[System.Serializable]
public class ColumnDefinition
{
    public string Header;
    public float Flex = 1f; // Flex value for proportional width
    public TextAlignmentOptions Alignment = TextAlignmentOptions.Left;
}