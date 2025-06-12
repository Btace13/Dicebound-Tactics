using UnityEngine;
using System.Collections.Generic;

public class NoiseMaker : MonoBehaviour
{
    [ContextMenu("Emit Noise")]
    public void EmitNoise()
    {
        EmitNoise();
    }

    public void EmitNoise(float initialNoiseLevel = 10f, float decayRate = 1f, NoiseFalloffType noiseFalloffType = NoiseFalloffType.LINEAR)
    {
        if (GridManager.Instance == null) return;

        Dictionary<Vector2, float> visited = new Dictionary<Vector2, float>();
        Queue<Vector2> nodes = new Queue<Vector2>();

        Vector2 startNode = GridManager.Instance.WorldToGridPosition(transform.position);

        visited.Add(startNode, initialNoiseLevel);
        nodes.Enqueue(startNode);

        while (nodes.Count > 0)
        {
            Vector2 node = nodes.Dequeue();

            //calculate noise level at node (linearly)
            float volume = 0;

            switch (noiseFalloffType)
            {
                case NoiseFalloffType.QUADRATIC:
                    volume = initialNoiseLevel - Mathf.Pow(Vector2.Distance(node, startNode), 2) * decayRate;
                    break;
                case NoiseFalloffType.CUBIC:
                    volume = initialNoiseLevel - Mathf.Pow(Vector2.Distance(node, startNode), 3) * decayRate;
                    break;
                case NoiseFalloffType.LINEAR:
                default:
                    volume = initialNoiseLevel - Vector2.Distance(node, startNode) * decayRate;
                    break;
            }

            //skip if volume is 0
            if (volume <= 0)
            {
                continue;
            }

            //get neighbors
            List<Vector2> neighbors = GetNeighbors(node);

            foreach (Vector2 neighbor in neighbors)
            {
                //add it if it doesn't exist
                if (!visited.ContainsKey(neighbor))
                {
                    visited.Add(neighbor, volume);
                    nodes.Enqueue(neighbor);
                    print($"{neighbor} is at {volume}");
                }
                //update it if the new volume is louder
                else if (visited[neighbor] < volume)
                    visited[neighbor] = volume;
            }
        }

        foreach (KeyValuePair<Vector2, float> node in visited)
        {
            NoiseManager.Instance.CheckForListeners(node.Key, node.Value);
        }
    }

    private List<Vector2> GetNeighbors(Vector2 node, float cellSize = 2)
    {
        List<Vector2> neighbors = new List<Vector2>();

        Vector2[] directions = new Vector2[4]
        {
            new Vector2(0,1), //up
            new Vector2(1, 0), //right
            new Vector2(0, -1), //down
            new Vector2(-1, 0) //left
        };

        foreach (Vector2 dir in directions)
        {
            neighbors.Add(node + dir * cellSize);
        }

        return neighbors;
    }
}
