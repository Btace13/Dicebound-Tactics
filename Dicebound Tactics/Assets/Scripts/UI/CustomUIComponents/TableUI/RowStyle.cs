using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "RowStyle", menuName = "UI/Table/RowStyle", order = 1)]
public class RowStyle : ScriptableObject
{
    [BoxGroup("General"), SerializeField, Tooltip("Offset applied to the row when hovered")] private Vector2 hoverRowOffset = new Vector2(0, 0);
    [BoxGroup("Background"), SerializeField] private Color backgroundColor = Color.white;
    [BoxGroup("Background"), SerializeField] private Color hoverColor = Color.gray;

    [BoxGroup("Borders"), SerializeField] private float borderWidth = 1f;
    [BoxGroup("Borders"), SerializeField] private Color borderColor = Color.black;

    [BoxGroup("Text"), SerializeField] private Color textColor = Color.black;
    [BoxGroup("Text"), SerializeField] private Color textHoverColor = Color.white;

    // Getters for the style properties
    public Vector2 HoverRowOffset => hoverRowOffset;
    public Color BackgroundColor => backgroundColor;
    public Color HoverColor => hoverColor;
    public float BorderWidth => borderWidth;
    public Color BorderColor => borderColor;
    public Color TextColor => textColor;
    public Color TextHoverColor => textHoverColor;
}
