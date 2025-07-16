using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class ResourceBarUI : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI resourceAmountText;
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI resourceNameText;
    [BoxGroup("References"), SerializeField] private Slider slider;
    [BoxGroup("Settings"), SerializeField] private string resourceName = "HP";
    [BoxGroup("Settings"), SerializeField] private int maxResource = 100;
    [BoxGroup("Settings"), SerializeField] private int currentResource = 100;
    [BoxGroup("Settings"), SerializeField, ColorUsage(false)] private Color barColor = Color.green;

    public int MaxResource
    {
        get => maxResource;
        set
        {
            maxResource = Mathf.Max(0, value);
            UpdateBar();
        }
    }

    public int CurrentResource
    {
        get => currentResource;
        set
        {
            currentResource = Mathf.Clamp(value, 0, maxResource);
            UpdateBar();
        }
    }

    public string ResourceName
    {
        get => resourceName;
        set
        {
            resourceName = value;
            resourceNameText.text = value + ":";
        }
    }

    private Image fillImage;

    public void SetResource(int resource)
    {
        currentResource = Mathf.Clamp(resource, 0, maxResource);
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (slider != null)
        {
            slider.maxValue = maxResource;
            slider.value = currentResource;
            if (fillImage == null)
            {
                fillImage = slider.fillRect.GetComponent<Image>();
            }
            fillImage.color = barColor;
            resourceAmountText.text = $"{currentResource}/{maxResource}";
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Ensure the resource name is set
        if (resourceNameText != null)
        {
            resourceNameText.text = resourceName + ":";
        }

        // Update the resource amount
        UpdateBar();
    }
#endif
}
