using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class ModularTableUI : MonoBehaviour
{
    [BoxGroup("Screen Header")]
    [SerializeField] private TextMeshProUGUI titleText;

    [BoxGroup("Screen Header")]
    [SerializeField] private TabController tabController;

    [BoxGroup("Pages")]
    [SerializeField] private List<PageView> allPages;

    private void Start()
    {
        tabController.OnTabSelected += ShowPage;
        ShowPage("Military"); // Default
    }

    [Button("Force Show Page: Military")]
    public void ShowPage(string pageName)
    {
        foreach (var page in allPages)
            page.gameObject.SetActive(page.PageName == pageName);

        titleText.text = "Personnel";
    }
}