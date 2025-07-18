using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;

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

    [BoxGroup("Config")]
    [SerializeField] private List<ColumnDefinition> columnDefinitions;

    [Button("Build Table With Mock Data")]
    public void BuildTable(List<RowData> data)
    {
        foreach (Transform child in headerContainer)
            DestroyImmediate(child.gameObject);

        for (int i = 0; i < columnDefinitions.Count; i++)
        {
            var def = columnDefinitions[i];
            float width = (data.Count > 0 && i < data[0].Values.Count) ? data[0].Values[i].Width : 100f;

            var go = Instantiate(headerTextPrefab, headerContainer);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = def.Header;
            text.alignment = def.Alignment;
            var rt = go.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        foreach (Transform child in rowContainer)
            DestroyImmediate(child.gameObject);

        foreach (var row in data)
        {
            var rowGO = Instantiate(rowPrefab, rowContainer);
            rowGO.GetComponent<RowUI>().Initialize(row);
        }
    }
}