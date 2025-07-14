using TacticsToolkit;
using UnityEngine;

[CreateAssetMenu(menuName = "Save System/Transform Save Data")]
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
        if (go.TryGetComponent(out CustomRichAI ai))
        {
            // If the GameObject has a CustomRichAI component, we need to set the position directly
            // to avoid issues with pathfinding or AI behavior
            ai.Teleport(position);
            bool tmpRotation = ai.enableRotation;
            ai.enableRotation = false;
            ai.transform.rotation = rotation;
            ai.transform.localScale = scale;
            ai.enableRotation = tmpRotation;
            return;
        }

        // Apply the captured transform data to the GameObject
        go.transform.position = position;
        go.transform.rotation = rotation;
        go.transform.localScale = scale;
    }
}
