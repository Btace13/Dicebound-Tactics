using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : MonoBehaviour
{
    private class PathNode
    {
        public Vector2 Position;
        public float GCost; // Cost from start to this node
        public float HCost; // Heuristic cost to target
        public float FCost => GCost + HCost; // Total cost
        public PathNode Parent;

        public PathNode(Vector2 position)
        {
            Position = position;
            GCost = 0;
            HCost = 0;
            Parent = null;
        }
    }

    public List<Vector2> FindPath(Vector2 start, Vector2 target)
    {
        HashSet<Vector2> closedSet = new HashSet<Vector2>();
        List<PathNode> openSet = new List<PathNode>();

        PathNode startNode = new PathNode(start);
        PathNode targetNode = new PathNode(target);

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get the node with the lowest F cost
            PathNode currentNode = openSet[0];
            foreach (var node in openSet)
            {
                if (node.FCost < currentNode.FCost || (node.FCost == currentNode.FCost && node.HCost < currentNode.HCost))
                {
                    currentNode = node;
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            // Check if we've reached the target
            if (currentNode.Position == targetNode.Position)
            {
                return RetracePath(startNode, currentNode);
            }

            // Check neighbors (including diagonal movement)
            foreach (var neighborPosition in GetNeighbors(currentNode.Position))
            {
                if (closedSet.Contains(neighborPosition))
                    continue;

                PathNode neighborNode = new PathNode(neighborPosition);
                float newGCost = currentNode.GCost + (currentNode.Position.x != neighborNode.Position.x && currentNode.Position.y != neighborNode.Position.y ? 1.4f : 1f);

                // Only check neighbor's costs if it’s not already in the open set
                if (!openSet.Contains(neighborNode) || newGCost < neighborNode.GCost)
                {
                    neighborNode.GCost = newGCost;
                    neighborNode.HCost = Vector2.Distance(neighborNode.Position, targetNode.Position);
                    neighborNode.Parent = currentNode;

                    if (!openSet.Contains(neighborNode))
                    {
                        openSet.Add(neighborNode);
                    }
                }
            }
        }

        return null; // No path found
    }

    private List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
    {
        List<Vector2> path = new List<Vector2>();
        PathNode currentNode = endNode;

        while (currentNode.Position != startNode.Position)
        {
            path.Add(currentNode.Position);
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private List<Vector2> GetNeighbors(Vector2 position)
    {
        List<Vector2> neighbors = new List<Vector2>
        {
            new Vector2(position.x + GridManager.Instance.CellSize, position.y), // Right
            new Vector2(position.x - GridManager.Instance.CellSize, position.y), // Left
            new Vector2(position.x, position.y + GridManager.Instance.CellSize), // Up
            new Vector2(position.x, position.y - GridManager.Instance.CellSize), // Down
            new Vector2(position.x + GridManager.Instance.CellSize, position.y + GridManager.Instance.CellSize), // Up Right (Diagonal)
            new Vector2(position.x + GridManager.Instance.CellSize, position.y - GridManager.Instance.CellSize), // Down Right (Diagonal)
            new Vector2(position.x - GridManager.Instance.CellSize, position.y + GridManager.Instance.CellSize), // Up Left (Diagonal)
            new Vector2(position.x - GridManager.Instance.CellSize, position.y - GridManager.Instance.CellSize)  // Down Left (Diagonal)
        };
        return neighbors;
    }
}
