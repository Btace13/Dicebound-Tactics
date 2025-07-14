using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class FlagConditionSet
{
    public enum ConditionMode { All, Any, None }

    [EnumToggleButtons]
    public ConditionMode mode = ConditionMode.All;

    [Tooltip("List of flags and their required values. The conditions will be evaluated based on the mode selected.")]
    public UDictionary<string, bool> requiredFlags = new();

    public bool AreConditionsMet()
    {
        int satisfied = 0;

        foreach (var pair in requiredFlags)
        {
            bool isMet = GameStateManager.Instance.Get(pair.Key) == pair.Value;

            if (mode == ConditionMode.All && !isMet) return false;
            if (mode == ConditionMode.Any && isMet) return true;
            if (mode == ConditionMode.None && isMet) return false;

            if (isMet) satisfied++;
        }

        return mode switch
        {
            ConditionMode.All => true,
            ConditionMode.None => true,
            ConditionMode.Any => false, // If none matched
            _ => false
        };
    }
}
