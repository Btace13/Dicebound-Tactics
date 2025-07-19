using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RowUI : MonoBehaviour
{
    [SerializeField] private GameObject textColumnPrefab;

    // Accept columnWidths as a parameter
    public void Initialize(RowData data, List<float> columnWidths)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        for (int i = 0; i < data.Values.Count; i++)
        {
            var val = data.Values[i];
            var go = Instantiate(textColumnPrefab, transform);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = val.Value;
            text.alignment = val.Alignment;
            // Use columnWidths for sizing
            float width = (i < columnWidths.Count) ? columnWidths[i] : text.rectTransform.sizeDelta.x;
            text.rectTransform.sizeDelta = new Vector2(width, text.rectTransform.sizeDelta.y);

            var rt = go.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }
    }
}