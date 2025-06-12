using UnityEngine;
using TMPro;

public class HighlightableNameText : MonoBehaviour
{
    private TMP_Text text;

    private void OnEnable()
    {
        //EventManager.OnHighlightableHover += OnHighlightableHover;
        //EventManager.OnHighlightableUnhover += OnHighlightableUnhover;
    }

    private void OnDisable()
    {
        //EventManager.OnHighlightableHover -= OnHighlightableHover;
        //EventManager.OnHighlightableUnhover -= OnHighlightableUnhover;
    }

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnHighlightableHover(GameObject obj)
    {
        if (text != null)
            text.text = obj.name;
    }

    private void OnHighlightableUnhover()
    {
        if (text != null)
            text.text = "";
    }
}
