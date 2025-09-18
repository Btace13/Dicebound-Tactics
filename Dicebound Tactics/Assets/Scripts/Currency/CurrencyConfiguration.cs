using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "CurrencyConfiguration", menuName = "Currency/Currency Configuration")]
public class CurrencyConfiguration : ScriptableObject
{
    [System.Serializable]
    public class CurrencyDisplayData
    {
        public CurrencyType currencyType;
        public Sprite icon;
        public Color displayColor = Color.white;
        public string displayName;
        public string shortName; // "G" for Gold, "S" for Shards
        
        [Header("Formatting")]
        public bool useCustomFormatting = false;
        public string customFormat = "{0}"; // {0} = amount
    }

    [SerializeField] private List<CurrencyDisplayData> currencyData = new List<CurrencyDisplayData>();

    private static CurrencyConfiguration instance;
    public static CurrencyConfiguration Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<CurrencyConfiguration>("CurrencyConfiguration");
                if (instance == null)
                {
                    Debug.LogWarning("CurrencyConfiguration not found in Resources folder!");
                }
            }
            return instance;
        }
    }

    public CurrencyDisplayData GetCurrencyData(CurrencyType type)
    {
        return currencyData.FirstOrDefault(data => data.currencyType == type);
    }

    public Sprite GetIcon(CurrencyType type)
    {
        var data = GetCurrencyData(type);
        return data?.icon;
    }

    public Color GetColor(CurrencyType type)
    {
        var data = GetCurrencyData(type);
        return data?.displayColor ?? Color.white;
    }

    public string GetDisplayName(CurrencyType type)
    {
        var data = GetCurrencyData(type);
        return data?.displayName ?? type.ToString();
    }

    public string GetShortName(CurrencyType type)
    {
        var data = GetCurrencyData(type);
        return data?.shortName ?? type.ToString().Substring(0, 1);
    }

    // Auto-populate with all currency types
    [ContextMenu("Auto-Setup Currency Types")]
    private void AutoSetupCurrencyTypes()
    {
        currencyData.Clear();
        
        foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
        {
            currencyData.Add(new CurrencyDisplayData
            {
                currencyType = type,
                displayName = type.ToString(),
                shortName = type.ToString().Substring(0, 1),
                displayColor = GetDefaultColor(type)
            });
        }
    }

    private Color GetDefaultColor(CurrencyType type)
    {
        switch (type)
        {
            case CurrencyType.Gold:
                return new Color(1f, 0.84f, 0f); // Gold color
            case CurrencyType.Shards:
                return new Color(0.4f, 0.8f, 1f); // Light blue
            default:
                return Color.white;
        }
    }
}