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
        ShowPage("Weapons"); // Default
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