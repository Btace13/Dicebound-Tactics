using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PageView : MonoBehaviour
{
    [BoxGroup("Page Info")]
    public string PageName;

    [BoxGroup("Page Info")]
    [SerializeField] private TextMeshProUGUI headerText;

    [BoxGroup("Table")]
    [SerializeField] private TableUI table;

    [BoxGroup("Table")]
    [SerializeField] private List<RowData> mockData;

    [Button("Initialize Page With Mock Data")]
    public void InitializePage()
    {
        headerText.text = PageName.ToUpper();
        table.BuildTable(mockData);
    }
}