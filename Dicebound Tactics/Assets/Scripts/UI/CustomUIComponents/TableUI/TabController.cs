using UnityEngine;
using UnityEngine.UI;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

public class TabController : MonoBehaviour
{
    [BoxGroup("Settings"), SerializeField] private Color selectedTabColor = Color.orangeRed;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTabColor = Color.lightGray;
    [BoxGroup("Settings"), SerializeField] private Color selectedTextColor = Color.white;
    [BoxGroup("Settings"), SerializeField] private Color unselectedTextColor = Color.darkGray;

    [BoxGroup("References"), SerializeField] private TabUI tabPrefab;

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

    public IEnumerator SyncTabsWithPages(List<PageView> allPages)
    {
        if (allPages == null || tabPrefab == null)
        {
            Debug.LogWarning("allPages or tabPrefab is not set!");
            yield break;
        }

        // Remove excess tabs
        while (Tabs.Count > allPages.Count)
        {
            var tabToRemove = Tabs[Tabs.Count - 1];
            Tabs.RemoveAt(Tabs.Count - 1);
            if (tabToRemove != null)
                DestroyImmediate(tabToRemove.gameObject);
            yield return null;
        }

        // Clear all tabs before re-adding to ensure correct order and no duplicates
        if (Tabs.Count != 0)
            Tabs.Clear();

        // Add tabs for each page
        for (int i = 0; i < allPages.Count; i++)
        {
            var page = allPages[i];
            if (page != null)
            {
                if (page.Tab == null && tabPrefab != null)
                {
                    var newTab = Instantiate(tabPrefab, transform);
                    page.Tab = newTab;
                }
                if (page.Tab != null)
                {
                    Tabs.Add(page.Tab);
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
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