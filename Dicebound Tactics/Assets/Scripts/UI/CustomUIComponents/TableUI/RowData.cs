using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RowData
{
    public List<RowValue> Values = new List<RowValue>();
    public Sprite Icon; // Optional, for visuals like items or portraits
}