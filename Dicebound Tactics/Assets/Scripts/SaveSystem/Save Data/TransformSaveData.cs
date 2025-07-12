using UnityEngine;

public class TransformSaveData : SaveData
{
    // This class is used to save the position, rotation, and scale of a GameObject
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public override void Capture(GameObject go)
    {
        // Capture the transform data of the GameObject
        position = go.transform.position;
        rotation = go.transform.rotation;
        scale = go.transform.localScale;
    }

    public override void Apply(GameObject go)
    {
        // Apply the captured transform data to the GameObject
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
    }
}
