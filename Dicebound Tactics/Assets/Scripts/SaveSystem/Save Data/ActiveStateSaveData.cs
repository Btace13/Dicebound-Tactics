using UnityEngine;

[CreateAssetMenu(menuName = "Save System/Active State Save Data")]
public class ActiveStateSaveData : SaveData
{
    // This class is used to save the active state of a GameObject
    public bool isActive;

    public override void Capture(GameObject go)
    {
        // Capture the active state of the GameObject
        isActive = go.activeSelf;
    }

    public override void Apply(GameObject go)
    {
        // Apply the captured active state to the GameObject
        go.SetActive(isActive);
    }
}
