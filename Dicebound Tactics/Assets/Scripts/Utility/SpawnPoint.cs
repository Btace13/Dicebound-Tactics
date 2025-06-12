using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public GameObject SpawnedObject;
    [SerializeField] public bool ShouldSpawn = true;

    private void OnDrawGizmos()
    {
        // Draw a sphere to visualize the spawn point in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}