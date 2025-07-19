using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class TabController : MonoBehaviour
{
    [BoxGroup("Settings"), SerializeField] private Color selectedTabColor = Color.orangeRed;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTabColor = Color.lightGray;
    [BoxGroup("Settings"), SerializeField] private Color selectedTextColor = Color.white;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTextColor = Color.darkGray;

    public event Action<string> OnTabSelected;

    public List<TabUI> Tabs { get; private set; } = new List<TabUI>();
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
        if (Tabs.Count == 0)
        {
            Tabs = new List<TabUI>(GetComponentsInChildren<TabUI>(true));
        }

        if (Tabs == null || Tabs.Count == 0)
        {
            Debug.LogWarning("No tabs found in TabController.");
            return;
        }

        foreach (var tab in Tabs)
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

        // Set the first tab as selected if none is selected
        if (SelectedTab == null && Tabs.Count > 0)
        {
            SelectedTab = Tabs[0];
            SelectedTab.UpdateTabColor(selectedTabColor, selectedTextColor);
        }
    }
#endif
}