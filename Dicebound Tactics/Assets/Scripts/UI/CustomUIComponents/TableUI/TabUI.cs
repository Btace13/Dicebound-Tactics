using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

public class TabUI : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] private TextMeshProUGUI tabText;
    [BoxGroup("References"), SerializeField] private Image tabIcon;
    [BoxGroup("References"), SerializeField] private Image tabBackground;

    public string TabName { get; private set; }
    public bool IsSelected
    {
        get
        {
            return _tabController != null && _tabController.SelectedTab == this;
        }
    }

    private TabController _tabController;

    public void Initialize(string tabName, Sprite icon)
    {
        TabName = tabName;
        tabText.text = tabName;
        if (icon != null)
        {
            tabIcon.sprite = icon;
            tabIcon.gameObject.SetActive(true);
        }
        else
        {
            tabIcon.gameObject.SetActive(false);
        }

        if (_tabController == null)
        {
            _tabController = GetComponentInParent<TabController>(true);
            if (_tabController == null)
            {
                Debug.LogError("TabController not found in parent hierarchy.");
            }
        }
    }

    public void UpdateTabColor(Color backgroundColor, Color textColor)
    {
        if (tabBackground != null)
        {
            tabBackground.color = backgroundColor;
        }

        if (tabText != null)
        {
            tabText.color = textColor;
        }

        if (tabIcon != null)
        {
            tabIcon.color = textColor; // Assuming icon color should match text color

            //turn off? 
            tabIcon.gameObject.SetActive(IsSelected);
        }
    }
}
