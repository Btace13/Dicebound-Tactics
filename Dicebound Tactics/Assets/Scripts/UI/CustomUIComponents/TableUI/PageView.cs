using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class PageView : MonoBehaviour
{
    [BoxGroup("Page Info")]
    public string PageName;

    [BoxGroup("Page Info")]
    [SerializeField] private TabUI tab;
    public TabUI Tab { get { return tab; } set { tab = value; } }

    [BoxGroup("Table Components")]
    [SerializeField] private Transform headerContainer;

    [BoxGroup("Table Components")]
    [SerializeField] private Transform rowContainer;

    [BoxGroup("Table Components")]
    [SerializeField] private GameObject headerTextPrefab;

    [BoxGroup("Table Components")]
    [SerializeField] private GameObject rowPrefab;

    [BoxGroup("Table")]
    [SerializeField] private RowStyle rowStyleSO;

    public UnityAction<RowData> OnRowSelected;

    public IEnumerator InitializePage(List<RowData> rowData, ColumnDefinitionsSO columnDefinitionsSO)
    {
        if (tab != null)
        {
            tab.TabName = PageName.ToUpper();
        }

        if (rowData == null || columnDefinitionsSO == null || columnDefinitionsSO.Columns == null || columnDefinitionsSO.Columns.Count == 0)
        {
            Debug.LogWarning("Row data or column definitions are missing!");
            yield break;
        }

        yield return BuildTableCoroutine(columnDefinitionsSO.Columns, rowData);
    }

    public void SetHeader(string text)
    {
        if (tab != null)
        {
            tab.TabName = text;
        }
    }

    public IEnumerator BuildTableCoroutine(List<ColumnDefinition> columns, List<RowData> data)
    {
        if (headerContainer.childCount > 0)
        {
            foreach (Transform child in headerContainer)
                DestroyImmediate(child.gameObject);

            yield return null; // Wait for the destruction to complete
        }

        while (headerContainer.childCount > 0)
            yield return null;

        float totalFlex = 0f;
        foreach (var col in columns)
            totalFlex += col.Flex;

        float totalWidth = (rowContainer as RectTransform).rect.width;
        List<float> columnWidths = new List<float>();
        for (int i = 0; i < columns.Count; i++)
        {
            ColumnDefinition def = columns[i];
            float colWidth = (def.Flex / totalFlex) * totalWidth;
            print($"Column {i} - Flex: {def.Flex}, Width: {colWidth}, Total Width: {totalWidth}");
            columnWidths.Add(colWidth);

            var go = Instantiate(headerTextPrefab, headerContainer);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = def.Header;
            text.alignment = def.Alignment;
            var rt = go.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, colWidth);
        }

        if (rowContainer.childCount > 0)
        {
            foreach (Transform child in rowContainer)
                DestroyImmediate(child.gameObject);

            yield return null; // Wait for the destruction to complete
        }

        while (rowContainer.childCount > 0)
            yield return null;

        foreach (var row in data)
        {
            var adjustedValues = new List<RowValue>();
            for (int i = 0; i < columns.Count; i++)
            {
                if (i < row.Values.Count)
                {
                    adjustedValues.Add(row.Values[i]);
                }
                else
                {
                    adjustedValues.Add(new RowValue { Value = "", Alignment = columns[i].Alignment });
                }
            }
            var adjustedRow = new RowData(columns, adjustedValues);
            var rowGO = Instantiate(rowPrefab, rowContainer);
            rowGO.GetComponent<RowUI>().Initialize(adjustedRow, columnWidths, rowStyleSO);
        }
    }
}