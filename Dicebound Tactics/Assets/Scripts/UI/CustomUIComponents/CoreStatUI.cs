using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class CoreStatUI : MonoBehaviour
{
    [BoxGroup("Values"), SerializeField] private string coreStatName = "Core Stat"; // Default name
    [BoxGroup("Values"), SerializeField] private int coreStatValue = 0; // Default value
    [BoxGroup("Values"), SerializeField] private Sprite coreStatIconSprite;

    public int CoreStatValue
    {
        get => coreStatValue;
        set
        {
            coreStatValue = value;
            UpdateUI();
        }
    }
    public string CoreStatName
    {
        get => coreStatName;
        set
        {
            coreStatName = value;
            UpdateUI();
        }
    }

    [BoxGroup("References"), SerializeField] private Image coreStatIcon;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI coreStatNameText;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI coreStatValueText;

    private void UpdateUI()
    {
        if (coreStatNameText != null)
        {
            coreStatNameText.text = coreStatName;
        }
        if (coreStatValueText != null)
        {
            coreStatValueText.text = coreStatValue.ToString();
        }
        if (coreStatIcon != null && coreStatIconSprite != null)
        {
            // Assuming you have a method to get the icon based on the core stat name
            coreStatIcon.sprite = coreStatIconSprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Update the UI when values are changed in the inspector
        UpdateUI();
    }
#endif
}
