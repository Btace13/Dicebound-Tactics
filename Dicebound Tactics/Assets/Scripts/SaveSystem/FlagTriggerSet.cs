using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public class FlagTriggerSet
{
    [TableList]
    public UDictionary<string, bool> flags = new();

    [Button("Trigger All Flags")]
    public void Trigger()
    {
        foreach (var pair in flags)
        {
            GameStateManager.Instance.Set(pair.Key, pair.Value);
        }
    }
}