using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class PageView : MonoBehaviour
{
    [BoxGroup("Page Info")]
    public string PageName;

    [BoxGroup("Page Info")]
    [SerializeField] private TabUI tab;
    public TabUI Tab { get { return tab; } set { tab = value; } }

    [BoxGroup("Table")]
    [SerializeField] private TableUI table;

    [BoxGroup("Table")]
    [SerializeField] private List<RowData> mockData;

    [BoxGroup("Table")]
    [SerializeField] private ColumnDefinitionsSO columnDefinitionsSO;

    [Button("Initialize Page With Mock Data")]
    public void InitializePage(List<RowData> rowData = null)
    {
        tab.TabName = PageName.ToUpper();
        var data = rowData ?? mockData;
        table.BuildTable(columnDefinitionsSO.Columns, data);
    }

    public void SetHeader(string text)
    {
        tab.TabName = text;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        tab.TabName = PageName.ToUpper();
    }
#endif
}