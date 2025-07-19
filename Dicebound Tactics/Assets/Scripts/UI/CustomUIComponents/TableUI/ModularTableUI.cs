using UnityEngine;
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

    [BoxGroup("Pages")]
    [SerializeField] private List<PageView> allPages;

    private Coroutine syncTabsCoroutine;

    private void Start()
    {
        SyncTabsWithPages();
        tabController.OnTabSelected += ShowPage;
        ShowPage("Weapons"); // Default
    }

    [Button("Sync Tabs with Pages")]
    public void SyncTabsWithPages()
    {
        if (syncTabsCoroutine != null)
        {
            StopCoroutine(syncTabsCoroutine);
        }
        syncTabsCoroutine = StartCoroutine(SyncTabsWithPagesCoroutine());
    }

    private IEnumerator SyncTabsWithPagesCoroutine()
    {
        if (tabController == null || allPages == null)
        {
            Debug.LogWarning("TabController or allPages is not set!");
            yield break;
        }

        print($"Syncing {tabController.Tabs.Count} tabs with {allPages.Count} pages.");

        yield return StartCoroutine(tabController.SyncTabsWithPages(allPages));

        print($"Final tab count: {tabController.Tabs.Count}");

        // Set tab names to match page names
        for (int i = 0; i < allPages.Count; i++)
        {
            var page = allPages[i];
            if (page != null && page.Tab != null)
            {
                page.Tab.TabName = page.PageName;
            }
        }
    }

    [Button("Force Show Page: Weapons")]
    public void ForceShowWeaponsPage()
    {
        ShowPage("Weapons");
    }

    public void ShowPage(string pageName)
    {
        foreach (var page in allPages)
        {
            bool isActive = page.PageName == pageName;
            page.gameObject.SetActive(isActive);
            if (isActive)
            {
                titleText.text = page.PageName;
                page.SetHeader(page.PageName.ToUpper());
                page.InitializePage();
            }
        }
    }
}