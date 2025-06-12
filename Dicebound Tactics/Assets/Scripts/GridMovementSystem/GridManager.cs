using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Pathfinding;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [BoxGroup("Grid Settings")]
    [Tooltip("The origin point of the grid.")]
    [SerializeField] private Vector3 gridOrigin;

    [BoxGroup("Grid Settings")]
    [Tooltip("The size of each grid cell.")]
    [SerializeField] private float cellSize = 2f;
    public float CellSize { get { return cellSize; } }

    [BoxGroup("Grid Settings")]
    [Tooltip("Specify the layer(s) that define walkable ground.")]
    [SerializeField] private LayerMask groundLayer;

    [BoxGroup("References")]
    [SerializeField] public GridShapeGenerator ShapeGenerator;
    [SerializeField] public CoverChecker CoverChecker;
    [SerializeField] public AstarPath aStar;
    [SerializeField] public MoveableAreaHandler moveableAreaHandler;
    [SerializeField] Material _lineMaterial;

    private LineRenderer linePath;

    [Header("Debugging")]
    [SerializeField] bool useDebugs = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public List<Vector2> GetWalkablePositions(Vector3 position, int radius)
    {
        Vector2 gridPos = WorldToGridPosition(position);
        List<Vector2> possiblePoints = GridUtility.GetSpacesWithinRange(gridPos, radius, cellSize);

        return possiblePoints.Where(point => IsPositionWalkable(point)).ToList();
    }

    private bool IsPositionWalkable(Vector2 position)
    {
        // Perform a raycast from above the position downwards to check for any hit
        if (Physics.Raycast(new Vector3(position.x, 10, position.y), Vector3.down, out RaycastHit hit, 11f))
        {
            // If the hit object is not on the ground layer, return false (not walkable)
            if ((groundLayer.value & (1 << hit.collider.gameObject.layer)) == 0)
            {
                if (hit.collider.TryGetComponent(out GridCharacterController cc))
                {
                    //walkable if over currently selected unit
                    return cc.IsSelected;
                }
                // else if (hit.collider.TryGetComponent(out GridInteractable i))
                // {
                //     return i.UsableDirections == GridInteractable.Direction.None;
                // }
                else
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    // Check if a position is walkable based on the layer mask
    public bool IsPositionWalkable(Vector3 position)
    {
        return IsPositionWalkable(new Vector2(position.x, position.z));
    }

    // Get the nearest walkable position for a given world position
    public Vector2 GetNearestWalkablePosition(Vector3 worldPosition)
    {
        Vector2 nearestPosition = WorldToGridPosition(worldPosition);

        // Check if the nearest position is walkable; if not, find the closest one
        if (!IsPositionWalkable(new Vector3(nearestPosition.x, 0, nearestPosition.y)))
        {
            nearestPosition = FindClosestWalkablePosition(nearestPosition);
        }

        return nearestPosition;
    }

    // Convert world position to grid position
    public Vector2 WorldToGridPosition(Vector3 position)
    {
        // Shift by 1 to align with grid
        float xShifted = position.x + 1;
        float zShifted = position.z + 1;

        // Round to nearest 2x2 grid center
        float roundedX = Mathf.Round(xShifted / CellSize) * CellSize - 1;
        float roundedZ = Mathf.Round(zShifted / CellSize) * CellSize - 1;

        // Return the new position with y unchanged
        return new Vector2(roundedX, roundedZ);
    }

    public Vector2Int WorldToGridPositionRounded(Vector3 position)
    {
        Vector2 gridPos = WorldToGridPosition(position);

        return new Vector2Int(Mathf.RoundToInt(gridPos.x), Mathf.RoundToInt(gridPos.y));
    }

    // Find the closest walkable position
    private Vector2 FindClosestWalkablePosition(Vector2 startPosition)
    {
        int searchRadius = 1; // Start with an initial search radius
        int maxSearchRadius = 10; // Set a maximum search radius to prevent infinite loops
        Vector3 worldPos;

        while (searchRadius <= maxSearchRadius)
        {
            // Iterate through all the positions in a square area around the starting position
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    Vector2 searchPosition = new Vector2(startPosition.x + x * cellSize, startPosition.y + y * cellSize);

                    // Convert grid position back to world position for the walkability check
                    worldPos = new Vector3(searchPosition.x, 0, searchPosition.y);

                    if (IsPositionWalkable(worldPos))
                    {
                        return searchPosition; // Return the first walkable position found
                    }
                }
            }

            // Expand the search area for the next iteration
            searchRadius++;
        }

        // If no walkable position was found within the max radius, return the original position
        Debug.LogWarning("No walkable position found within the search radius.");
        return startPosition;
    }

    //line pathing 
    public void HidePathVisual()
    {
        if (linePath && linePath.enabled)
            linePath.enabled = false;
    }

    public void DrawLine(List<Vector3> boundaryVertices, Color color, float lineWidth = 0.1f, bool isClosed = false)
    {
        if (linePath == null)
        {
            linePath = gameObject.AddComponent<LineRenderer>();
        }

        //copy material the first time
        if (linePath.material == null || linePath.material.shader != _lineMaterial.shader)
            linePath.material = new Material(_lineMaterial);

        // Setup line renderer
        linePath.material.color = color;
        linePath.generateLightingData = true;
        linePath.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        linePath.receiveShadows = false;
        linePath.startWidth = linePath.endWidth = lineWidth;
        linePath.positionCount = boundaryVertices.Count;
        linePath.numCornerVertices = 5;
        linePath.SetPositions(boundaryVertices.ToArray());
        linePath.loop = isClosed;
        linePath.enabled = true;
    }

    //Moveable Area 

    public void UpdateMoveableArea(Vector3 position, int distanceCanMove, int distanceCanSprint)
    {
        moveableAreaHandler.UpdateMoveableArea(position, distanceCanMove, distanceCanSprint);
    }

    public void HideMoveableArea()
    {
        moveableAreaHandler.HideMoveableArea();
    }

    public bool CanMoveToTargetPosition(Vector2 position)
    {
        if (moveableAreaHandler.WalkablePositions != null)
        {
            if (useDebugs)
                Debug.Log("Walkable Positions = " + moveableAreaHandler.WalkablePositions.Count);
        }
        else
            Debug.LogError("Walkable Positions is null");

        if (moveableAreaHandler == null || moveableAreaHandler.WalkablePositions == null || moveableAreaHandler.WalkablePositions.Count == 0)
            return false;

        if (useDebugs)
            Debug.Log("Position To Check = " + position);

        return moveableAreaHandler.WalkablePositions.Contains(position);
    }

    public bool CanSprintToTargetPosition(Vector2 position)
    {
        if (moveableAreaHandler.SprintablePositions != null)
        {
            if (useDebugs)
                Debug.Log("Sprintable Positions = " + moveableAreaHandler.SprintablePositions.Count);
        }
        else
            Debug.LogError("Sprintable Positions is null");

        if (moveableAreaHandler == null || moveableAreaHandler.SprintablePositions == null || moveableAreaHandler.SprintablePositions.Count == 0)
            return false;

        if (useDebugs)
            Debug.Log("Position To Check = " + position);

        return moveableAreaHandler.SprintablePositions.Contains(position);
    }

    public void CalculateDistanceToTarget(Vector3 start, Vector3 destination, System.Action<int> OnDistanceCalculated = null)
    {
        ABPath.Construct(start, destination, p =>
        {
            OnDistanceCalculated?.Invoke(p.path.Count);
        });
    }

    public bool HasLineOfSight(Vector2Int start, Vector2Int end)
    {
        // Get the difference between the start and end positions
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);
        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;

        int err = dx - dy;

        int currentX = start.x;
        int currentY = start.y;

        // Step through the grid using Bresenham's line algorithm
        while (currentX != end.x || currentY != end.y)
        {
            // Check if the current grid position is an obstacle
            if (!IsPositionWalkable(new Vector2Int(currentX, currentY)))
            {
                return false; // Line of sight is blocked
            }

            // Update error and position
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                currentX += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                currentY += sy;
            }
        }

        // No obstacles found along the line
        return true;
    }
}
