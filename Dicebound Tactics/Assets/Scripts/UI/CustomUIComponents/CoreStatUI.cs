using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class CoreStatUI : StatUI
{
    [BoxGroup("Values"), SerializeField] private Sprite coreStatIconSprite;
    [BoxGroup("References"), SerializeField] private Image coreStatIcon;

    public int CoreStatValue
    {
        get => StatValue;
        set => StatValue = value;
    }
    public string CoreStatName
    {
        get => StatName;
        set => StatName = value;
    }

    protected override void UpdateUI()
    {
        base.UpdateUI();
        if (coreStatIcon != null && coreStatIconSprite != null)
        {
            coreStatIcon.sprite = coreStatIconSprite;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
    }
#endif
}
