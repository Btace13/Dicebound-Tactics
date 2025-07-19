using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ModularTableUI : MonoBehaviour
{
    [BoxGroup("References")]
    [SerializeField] private TextMeshProUGUI titleText;

    [BoxGroup("References")]
    [SerializeField] private TabController tabController;

    [BoxGroup("References")]
    [SerializeField] private GameObject pageViewPrefab;

    [BoxGroup("References")]
    [SerializeField] private Transform pageContainer;

    [BoxGroup("Table Data")]
    [SerializeField] private List<TableDataSO> tableDataList;

    [BoxGroup("Pages")]
    private List<PageView> allPages = new List<PageView>();


    private IEnumerator Start()
    {
        // Instantiate PageViews for each TableDataSO
        foreach (var tableData in tableDataList)
        {
            var pageGO = Instantiate(pageViewPrefab, pageContainer);
            var pageView = pageGO.GetComponent<PageView>();
            pageView.PageName = tableData.name;

            LayoutRebuilder.ForceRebuildLayoutImmediate(pageGO.GetComponent<RectTransform>());
            yield return new WaitForEndOfFrame(); // Ensure UI updates before initializing

            yield return StartCoroutine(pageView.InitializePage(tableData.Rows, tableData.ColumnDefinitions));

            pageGO.SetActive(false);
            allPages.Add(pageView);
        }

        yield return StartCoroutine(SyncTabsWithPagesCoroutine());
        tabController.OnTabSelected += ShowPage;

        ShowPage(0); // Default
    }

    private IEnumerator SyncTabsWithPagesCoroutine()
    {
        if (tabController == null || allPages == null)
        {
            Debug.LogWarning("TabController or allPages is not set!");
            yield break;
        }

        yield return StartCoroutine(tabController.SyncTabsWithPages(allPages));

        print($"Final tab count: {tabController.Tabs.Count}");

        // Set tab names to match page names
        for (int i = 0; i < allPages.Count; i++)
        {
            var page = allPages[i];
            page.Tab = tabController.Tabs[i];

            if (page != null && page.Tab != null)
            {
                page.Tab.TabName = page.PageName;
            }
        }
    }

    public void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= allPages.Count)
        {
            Debug.LogWarning($"Page index {pageIndex} is out of range. Total pages: {allPages.Count}");
            return;
        }

        ShowPage(allPages[pageIndex].PageName);
    }

    public void ShowPage(string pageName)
    {
        foreach (var page in allPages)
        {
            bool isActive = page.PageName.ToLower() == pageName.ToLower();
            page.gameObject.SetActive(isActive);

            if (isActive)
            {
                titleText.text = page.PageName;
                page.SetHeader(page.PageName.ToUpper());
            }
        }

        tabController.UpdateTabColors();
    }
}