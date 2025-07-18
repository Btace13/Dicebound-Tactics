using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class RowUI : MonoBehaviour
{
    [SerializeField] private Transform columnContainer;
    [SerializeField] private GameObject textColumnPrefab;

    public void Initialize(RowData data)
    {
        foreach (Transform child in columnContainer)
            Destroy(child.gameObject);

        foreach (var val in data.Values)
        {
            var go = Instantiate(textColumnPrefab, columnContainer);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = val.Value;
            text.alignment = val.Alignment;

            var rt = go.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, val.Width);
        }
    }
}