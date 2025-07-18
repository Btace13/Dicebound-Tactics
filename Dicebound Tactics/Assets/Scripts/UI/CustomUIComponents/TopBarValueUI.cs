using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class TopBarValueUI : MonoBehaviour
{
    public int Value = 0;

    [BoxGroup("Settings"), SerializeField] private int maxLength = 10;
    [BoxGroup("Settings"), SerializeField] private Color primaryColor = Color.white;
    [BoxGroup("Settings"), SerializeField] private Color secondaryColor = Color.gray;
    [BoxGroup("Settings"), SerializeField] private Color iconColor = Color.white;

    [BoxGroup("References"), SerializeField]
    private TextMeshProUGUI valueText;
    [BoxGroup("References"), SerializeField] private Image iconImage;

    public void SetValue(int value)
    {
        Value = value;

        // Remove any formatting (like commas) from input value
        string rawValue = value.ToString().Replace(",", "");
        int padLength = Mathf.Max(0, maxLength - rawValue.Length);
        string paddedZeros = new string('0', padLength);

        string secondaryHex = ColorUtility.ToHtmlStringRGBA(secondaryColor);
        string primaryHex = ColorUtility.ToHtmlStringRGBA(primaryColor);

        // Format the output with colors and add commas to the value
        string formattedValue = $"<color=#{secondaryHex}>{paddedZeros}</color><color=#{primaryHex}>{int.Parse(rawValue):N0}</color>";
        valueText.text = formattedValue;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure the icon color is set correctly in the editor
        if (iconImage != null)
        {
            iconImage.color = iconColor;
        }

        SetValue(Value);
    }
#endif
}