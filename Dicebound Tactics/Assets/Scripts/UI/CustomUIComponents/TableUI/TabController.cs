using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;

public class TabController : MonoBehaviour
{
    [BoxGroup("Settings"), SerializeField] private Color selectedTabColor = Color.orangeRed;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTabColor = Color.lightGray;
    [BoxGroup("Settings"), SerializeField] private Color selectedTextColor = Color.white;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTextColor = Color.darkGray;

    public event Action<string> OnTabSelected;

    public TabUI SelectedTab { get; private set; }

    public void TabClicked(Button button)
    {
        OnTabSelected?.Invoke(button.name);

        if (SelectedTab != null)
        {
            SelectedTab.UpdateTabColor(unselectedTabColor, unselectedTextColor);
        }

        SelectedTab = button.GetComponent<TabUI>();
        SelectedTab.UpdateTabColor(selectedTabColor, selectedTextColor);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        TabUI[] tabs = GetComponentsInChildren<TabUI>(true);

        foreach (var tab in tabs)
        {
            if (tab == null) continue;

            if (tab.IsSelected)
            {
                tab.UpdateTabColor(selectedTabColor, selectedTextColor);
            }
            else
            {
                tab.UpdateTabColor(unselectedTabColor, unselectedTextColor);
            }
        }
    }
#endif
}