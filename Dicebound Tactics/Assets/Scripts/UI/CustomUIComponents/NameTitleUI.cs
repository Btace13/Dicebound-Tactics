using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class NameTitleUI : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI firstNameText;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI lastNameText;
    [BoxGroup("Settings"), SerializeField] private string firstName = "Ellenai";
    [BoxGroup("Settings"), SerializeField] private string lastName = "Kesia";
    [BoxGroup("Settings"), SerializeField] private int preferredFontSize = 85;
    [BoxGroup("Settings"), SerializeField] private int minFontSize = 12;

    private RectTransform rectTransform;

    public string Name
    {
        get => firstNameText.text;
        set => firstNameText.text = value;
    }

    public string Title
    {
        get => lastNameText.text;
        set => lastNameText.text = value;
    }

    private bool FitsCurrentWidth
    {
        get
        {
            if (firstNameText == null || lastNameText == null)
                return false;

            float totalWidth = firstNameText.preferredWidth + lastNameText.preferredWidth;

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (rectTransform == null)
            {
                Debug.LogWarning("RectTransform is not set on NameTitleUI.");
                return false;
            }

            return totalWidth <= rectTransform.rect.width;
        }
    }

    private void UpdateTextSizeToFit()
    {
        if (FitsCurrentWidth)
        {
            firstNameText.fontSize = preferredFontSize;
            lastNameText.fontSize = preferredFontSize;
        }
        else
        {
            // Reduce font size until it fits
            float firstNameWidth = firstNameText.preferredWidth;
            float lastNameWidth = lastNameText.preferredWidth;
            float totalWidth = firstNameWidth + lastNameWidth;

            while (totalWidth > rectTransform.rect.width && firstNameText.fontSize > minFontSize)
            {
                firstNameText.fontSize--;
                lastNameText.fontSize--;
                firstNameWidth = firstNameText.preferredWidth;
                lastNameWidth = lastNameText.preferredWidth;
                totalWidth = firstNameWidth + lastNameWidth;
            }

            // Ensure the font size does not go below a minimum threshold
            if (firstNameText.fontSize < minFontSize || lastNameText.fontSize < minFontSize)
            {
                firstNameText.fontSize = minFontSize;
                lastNameText.fontSize = minFontSize;
            }
        }
    }

    private void OnValidate()
    {
        if (firstNameText != null)
        {
            firstNameText.text = firstName;
        }
        if (lastNameText != null)
        {
            lastNameText.text = lastName;
        }

        UpdateTextSizeToFit();
    }
}
