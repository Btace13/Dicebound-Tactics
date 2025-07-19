using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

[System.Serializable]
public class RowData
{
    [ValidateInput("ValidateRowValues", "Row values count does not match column count!", InfoMessageType.Error)]
    public List<RowValue> Values = new List<RowValue>();
    public Sprite Icon; // Optional, for visuals like items or portraits

    private List<ColumnDefinition> _columns;

    public RowData(List<ColumnDefinition> columns)
    {
        _columns = columns;
    }

    public RowData(List<ColumnDefinition> columns, List<RowValue> values, Sprite icon = null)
    {
        _columns = columns;
        Values = values;
        Icon = icon;
    }

    // This method will be called by Odin Inspector for validation
    private bool ValidateRowValues()
    {
        if (_columns == null)
        {
            return true;
        }

        if (Values == null)
        {
            Debug.LogError("Row values are null!");
            return false;
        }

        return Values.Count == _columns.Count;
    }
}