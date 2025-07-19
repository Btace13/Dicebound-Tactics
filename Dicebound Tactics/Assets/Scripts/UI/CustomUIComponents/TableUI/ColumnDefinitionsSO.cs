using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ColumnDefinitions", menuName = "UI/Table/ColumnDefinitions")]
public class ColumnDefinitionsSO : ScriptableObject
{
    public List<ColumnDefinition> Columns;
}
