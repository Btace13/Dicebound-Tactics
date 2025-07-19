using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;

public class TableUI : MonoBehaviour
{
    [BoxGroup("Table Components")]
    [SerializeField] private Transform headerContainer;

    [BoxGroup("Table Components")]
    [SerializeField] private Transform rowContainer;

    [BoxGroup("Table Components")]
    [SerializeField] private GameObject headerTextPrefab;

    [BoxGroup("Table Components")]
    [SerializeField] private GameObject rowPrefab;


    [BoxGroup("Debug"), SerializeField] private TableDataSO testTableDataSO;

    [Button("Build Table From SO")]
    public void BuildTableFromSO()
    {
        if (testTableDataSO == null)
        {
            Debug.LogWarning("Test TableDataSO is null!");
            return;
        }
        BuildTableFromSO(testTableDataSO);
    }

    public void BuildTableFromSO(TableDataSO tableDataSO)
    {
        if (tableDataSO == null || tableDataSO.ColumnDefinitions == null)
        {
            Debug.LogWarning("TableDataSO or ColumnDefinitionsSO is null!");
            return;
        }
        StartCoroutine(BuildTableCoroutine(tableDataSO.ColumnDefinitions.Columns, tableDataSO.Rows));
    }

    [Button("Build Table With Mock Data")]
    public void BuildTable(List<ColumnDefinition> columns, List<RowData> data)
    {
        StartCoroutine(BuildTableCoroutine(columns, data));
    }

    private IEnumerator BuildTableCoroutine(List<ColumnDefinition> columns, List<RowData> data)
    {
        if (headerContainer.childCount > 0)
        {
            // If headerContainer is not empty, we clear it
            foreach (Transform child in headerContainer)
                DestroyImmediate(child.gameObject);
        }

        while (headerContainer.childCount > 0)
            yield return null;

        // Calculate flex sum and actual widths
        float totalFlex = 0f;
        foreach (var col in columns)
            totalFlex += col.Flex;

        float totalWidth = (rowContainer as RectTransform).rect.width;
        List<float> columnWidths = new List<float>();
        for (int i = 0; i < columns.Count; i++)
        {
            var def = columns[i];
            float colWidth = (totalFlex > 0) ? (def.Flex / totalFlex) * totalWidth : 0f;
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
            // If rowContainer is not empty, we clear it
            foreach (Transform child in rowContainer)
                DestroyImmediate(child.gameObject);
        }

        while (rowContainer.childCount > 0)
            yield return null;

        foreach (var row in data)
        {
            // Ensure row.Values matches columns.Count
            var adjustedValues = new List<RowValue>();
            for (int i = 0; i < columns.Count; i++)
            {
                if (i < row.Values.Count)
                {
                    adjustedValues.Add(row.Values[i]);
                }
                else
                {
                    // Pad with empty/default value
                    adjustedValues.Add(new RowValue { Value = "", Alignment = columns[i].Alignment });
                }
            }

            var adjustedRow = new RowData(columns, adjustedValues);
            var rowGO = Instantiate(rowPrefab, rowContainer);
            rowGO.GetComponent<RowUI>().Initialize(adjustedRow, columnWidths);
        }
    }
}