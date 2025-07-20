using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class RowUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField] private GameObject textColumnPrefab;
    [SerializeField] private Image rowBackground;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField, ReadOnly] private List<TextMeshProUGUI> rowTexts = new List<TextMeshProUGUI>();

    private RowStyle _rowStyle;
    private Button _button;

    // Accept columnWidths as a parameter
    public void Initialize(RowData data, List<float> columnWidths, RowStyle rowStyle)
    {
        if (data == null || data.Values == null || data.Values.Count == 0)
        {
            Debug.LogWarning("RowData is null or has no values.");
            return;
        }

        // Apply row style
        if (rowStyle != null)
        {
            layoutGroup.padding.left = (int)rowStyle.HoverRowOffset.x;
            layoutGroup.padding.top = (int)rowStyle.HoverRowOffset.y;
        }

        // Clear existing children
        ClearChildren();

        // Build the row with the provided data and column widths
        BuildRow(data, columnWidths, rowStyle);

        if (_button == null)
        {
            _button = gameObject.AddComponent<Button>();
            _button.onClick.AddListener(() =>
            {
                Debug.Log($"Row {transform.GetSiblingIndex()} clicked!");
                if (data != null)
                {
                    var page = GetComponentInParent<PageView>(true);
                    page.OnRowSelected?.Invoke(data);
                }
            });
        }
    }

    private void ClearChildren()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private void BuildRow(RowData data, List<float> columnWidths, RowStyle rowStyle)
    {
        _rowStyle = rowStyle;

        if (rowBackground != null)
        {
            rowBackground.color = _rowStyle.BackgroundColor;
        }

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < data.Values.Count; i++)
        {
            var val = data.Values[i];
            var go = Instantiate(textColumnPrefab, transform);
            var rowText = go.GetComponent<TextMeshProUGUI>();

            rowText.text = val.Value;
            rowText.alignment = val.Alignment;
            rowText.color = _rowStyle.TextColor;

            // Use columnWidths for sizing
            rowText.rectTransform.sizeDelta = new Vector2(columnWidths[i], rowText.rectTransform.sizeDelta.y);

            rowTexts.Add(rowText);

            // Set the width of the RectTransform based on columnWidths
            var rt = go.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, columnWidths[i]);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rowBackground != null && _rowStyle != null)
        {
            rowBackground.color = _rowStyle.HoverColor;

            foreach (var rowText in rowTexts)
            {
                if (rowText != null)
                    rowText.color = _rowStyle.TextHoverColor; // Change text color on hover
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rowBackground != null && _rowStyle != null)
        {
            rowBackground.color = _rowStyle.BackgroundColor;

            foreach (var rowText in rowTexts)
            {
                if (rowText != null)
                    rowText.color = _rowStyle.TextColor;
            }
        }
    }
}