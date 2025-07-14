using UnityEngine;

public abstract class SaveData : ScriptableObject
{
    public abstract void Capture(GameObject go);
    public abstract void Apply(GameObject go);
}