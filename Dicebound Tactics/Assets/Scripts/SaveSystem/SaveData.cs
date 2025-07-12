using UnityEngine;

public abstract class SaveData
{
    public abstract void Capture(GameObject go);
    public abstract void Apply(GameObject go);
}