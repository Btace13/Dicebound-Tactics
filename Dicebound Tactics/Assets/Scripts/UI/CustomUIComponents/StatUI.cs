using UnityEngine;
using TMPro;

public class StatUI : MonoBehaviour
{
    [SerializeField] protected string statName = "Stat";
    [SerializeField] protected int statValue = 0;
    [SerializeField] protected TextMeshProUGUI statNameText;
    [SerializeField] protected TextMeshProUGUI statValueText;

    public virtual int StatValue
    {
        get => statValue;
        set
        {
            statValue = value;
            UpdateUI();
        }
    }

    public virtual string StatName
    {
        get => statName;
        set
        {
            statName = value;
            UpdateUI();
        }
    }

    protected virtual void UpdateUI()
    {
        if (statNameText != null)
        {
            statNameText.text = statName;
        }
        if (statValueText != null)
        {
            statValueText.text = statValue.ToString();
        }
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        UpdateUI();
    }
#endif
}
