using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TableData", menuName = "UI/Table/TableData")]
public class TableDataSO : ScriptableObject
{
    [Required]
    public string TableName = "New Table";

    [Required]
    public ColumnDefinitionsSO ColumnDefinitions;

    [TableList]
    public List<RowData> Rows;
}