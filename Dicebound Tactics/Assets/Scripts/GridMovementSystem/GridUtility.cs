using UnityEngine;
using System.Collections.Generic;

public class GridUtility : MonoBehaviour
{
    public static bool IncludeDiagonals = true;
    private static Plane XZPlane = new Plane(Vector3.up, Vector3.zero);

    // Define possible movements: cardinal (cost 1) and diagonal (cost 1.5)
    private static readonly (Vector2Int offset, float cost)[] directions = {
        (new Vector2Int(0, 1), 1f),    // Up
        (new Vector2Int(1, 0), 1f),    // Right
        (new Vector2Int(0, -1), 1f),   // Down
        (new Vector2Int(-1, 0), 1f),   // Left
        (new Vector2Int(1, 1), 1.5f),  // Up-Right
        (new Vector2Int(1, -1), 1.5f), // Down-Right
        (new Vector2Int(-1, -1), 1.5f), // Down-Left
        (new Vector2Int(-1, 1), 1.5f)  // Up-Left
    };

    public static Vector3 WorldToCellCenterPosition(Vector3 worldPosition)
    {
        int minX = Mathf.FloorToInt(worldPosition.x);
        int minZ = Mathf.FloorToInt(worldPosition.z);

        return new Vector3(minX + 0.5f, 0, minZ + 0.5f);
    }

    public static Vector2Int GetCoordinateFromWorld(Vector3 worldPos)
    {
        int minX = Mathf.FloorToInt(worldPos.x);
        int minZ = Mathf.FloorToInt(worldPos.z);

        return new Vector2Int(minX, minZ);
    }

    public static Vector3 HitPlaneAtPoint()
    {
        Vector3 hitPoint = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;

        if (XZPlane.Raycast(ray, out distance))
        {
            hitPoint = ray.GetPoint(distance);
            hitPoint.y = 0;
        }

        return hitPoint;
    }

    public static List<Vector2Int> GetNeighbors(Vector2Int origin, int steps)
    {
        List<Vector2Int> results = new List<Vector2Int>();

        for (int x = -steps; x <= steps; x++)
            for (int y = -steps; y <= steps; y++)
            {
                Vector2Int neighborCoord = new Vector2Int(origin.x + x, origin.y + y);

                if (neighborCoord != origin)
                    if (IncludeDiagonals || (!IncludeDiagonals && Mathf.Abs(x) != Mathf.Abs(y)))
                        results.Add(neighborCoord);
            }

        return results;
    }

    /// <summary>
    /// Calculates the distance between two Vector3 coordinates
    /// </summary>
    /// <param name="a">Vector3 cube coordinate a</param>
    /// <param name="b">Vector3 cube coordinate b</param>
    /// <returns>float distance</returns>
    public static float GetDistanceBetweenTwoPositions(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b);
    }

    public static List<Vector3> GetPositionsOnLine(Vector3 startPosition, Vector3 direction, int steps)
    {
        List<Vector3> line = new List<Vector3>();

        if (steps == 0) return line;

        for (int i = 0; i <= steps; i++)
        {
            Vector3 nextPosition = startPosition + direction.normalized * i;
            line.Add(nextPosition);
        }

        return line;
    }

    public static HashSet<Vector2Int> GetPointsInCone(Vector3 startPosition, Vector3 direction, int range, float angle = 90)
    {
        HashSet<Vector2Int> pointsInCone = new HashSet<Vector2Int>();

        float halfAngle = angle * 0.5f;
        Vector2Int startGridSpace = GridManager.Instance.WorldToGridPositionRounded(startPosition);

        for (int y = 0; y < range; y++)
		{
            Vector2Int space = GridManager.Instance.WorldToGridPositionRounded(startPosition + y * direction.normalized);
           
            if (GridManager.Instance.HasLineOfSight(startGridSpace, space))
                pointsInCone.Add(space);
            else
                break;
		}

        for (int r = 5; r <= halfAngle; r+=5)
		{
            Vector3 leftDir = Quaternion.Euler(0, -r, 0) * direction.normalized;
            Vector3 rightDir = Quaternion.Euler(0, r, 0) * direction.normalized;

            bool leftLineOfSightBlocked = false;
            bool rightLineOfSightBlocked = false;

            for (int i = 1; i <= range; i+=(int)GridManager.Instance.CellSize)
            {
                if (!leftLineOfSightBlocked)
				{
                    Vector2Int leftPos = GridManager.Instance.WorldToGridPositionRounded(startPosition + (i * leftDir));

                    if (GridManager.Instance.HasLineOfSight(startGridSpace, leftPos))
                    {
                        pointsInCone.Add(leftPos);
                    }
                    else
                        leftLineOfSightBlocked = true;
				}

                if (!rightLineOfSightBlocked)
				{
                    Vector2Int rightPos = GridManager.Instance.WorldToGridPositionRounded(startPosition + (i * rightDir));

                    if (GridManager.Instance.HasLineOfSight(startGridSpace, rightPos))
                    {
                        pointsInCone.Add(rightPos);
                    }
                    else
                        rightLineOfSightBlocked = true;
				}
            }
		}

        return pointsInCone;
    }

    public static List<Vector2> GetRadiusAroundPosition(Vector2 center, int radius, float cellSize = 1)
    {
        List<Vector2> positions = new List<Vector2>();

        // Loop through the square area around the center
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                // Check Euclidean distance instead of Manhattan distance to form a circle
                if (Mathf.Sqrt(x * x + z * z) <= radius)
                {
                    Vector2 newPos = new Vector2(Mathf.RoundToInt(center.x + x * cellSize), Mathf.RoundToInt(center.y + z * cellSize));
                    positions.Add(newPos);
                }
            }
        }

        return positions;
    }

    public static List<Vector3> GetRadiusAroundPosition(Vector3 center, int radius, float cellSize = 1)
    {
        List<Vector3> positions = new List<Vector3>();

        // Loop through the square area around the center
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                // Check Euclidean distance to form a circle
                if (Mathf.Sqrt(x * x + z * z) <= radius)
                {
                    Vector3 newPos = new Vector3(center.x + x * cellSize, center.y, center.z + z * cellSize);
                    positions.Add(newPos);
                }
            }
        }

        return positions;
    }

    public static List<Vector2> GetSpacesWithinRange(Vector2 start, float maxRange, float cellSize)
    {
        Queue<(Vector2,float)> q = new Queue<(Vector2, float)>();
        q.Enqueue((start,0));

        List<Vector2> spacesInRange = new List<Vector2> { start };
        HashSet<Vector2> visited = new HashSet<Vector2>() { start };

        while (q.Count > 0)
		{
            (Vector2, float) current = q.Dequeue();

            foreach (var (offset, cost) in directions)
            {
                Vector2 neighbor = current.Item1 + new Vector2((int)(offset.x * cellSize), (int)(offset.y * cellSize));
                float newCost = current.Item2 + cost;

                if (newCost <= maxRange && !visited.Contains(neighbor))
				{
                    spacesInRange.Add(neighbor);
                    q.Enqueue((neighbor, newCost));
                    visited.Add(neighbor);
				}
            }
        }

        return spacesInRange;
    }

        public static List<Vector2> ConvertToVector2List(List<Vector3> vector3List)
    {
        List<Vector2> vector2List = new List<Vector2>();
        foreach (var vector3 in vector3List)
        {
            // Create a new Vector2 using the x and z components of the Vector3
            Vector2 vector2 = new Vector2(vector3.x, vector3.z);
            vector2List.Add(vector2);
        }
        return vector2List;
    }
}
