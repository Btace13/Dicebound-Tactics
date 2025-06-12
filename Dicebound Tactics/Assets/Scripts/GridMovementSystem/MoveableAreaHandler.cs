using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MoveableAreaHandler : MonoBehaviour
{
    public struct Edge
	{
        public Vector3 start;
        public Vector3 end;

        public Edge(Vector3 _start, Vector3 _end)
		{
            start = _start;
            end = _end;
		}
	}


    [BoxGroup("Renderer Settings"), SerializeField] private float heightOffGround = 0.1f;

    [BoxGroup("References")]
    [SerializeField] private Material areaOutlineMaterial;
    [BoxGroup("References"), SerializeField] private Material moveableAreaMaterial;
    [BoxGroup("References"), SerializeField] private Material sprintableAreaMaterial;

    public List<Vector2> WalkablePositions = new List<Vector2>();
    public List<Vector2> SprintablePositions = new List<Vector2>();

    private MeshRenderer _gridMesh;
    private MeshRenderer _sprintGridMesh;

    private Transform _walkableOutlineRoot;
    private Transform _sprintableOutlineRoot;

    public void UpdateMoveableArea(Vector3 worldPosition, int distanceCanMove, int distanceCanSprint)
    {
        // Get walkable positions
        WalkablePositions = GridManager.Instance.GetWalkablePositions(worldPosition, distanceCanMove);

        // Get sprintable positions: within sprint distance but not in walkable positions
        SprintablePositions = GridManager.Instance.GetWalkablePositions(worldPosition, distanceCanSprint);//.Except(WalkablePositions).ToList();

        if (_walkableOutlineRoot == null)
        {
            _walkableOutlineRoot = new GameObject("Walkable Bounds").transform;
            _walkableOutlineRoot.parent = transform;
        }
        if (_sprintableOutlineRoot == null)
        {
            _sprintableOutlineRoot = new GameObject("Sprintable Bounds").transform;
            _sprintableOutlineRoot.parent = transform;
        }

        //generate moveable area mesh
        GenerateAreaMesh(WalkablePositions, 2, moveableAreaMaterial, false);
        GenerateAreaMesh(SprintablePositions, 2, sprintableAreaMaterial, true);
    }

    public void HideMoveableArea()
	{
        if (_gridMesh != null)
		{
            _gridMesh.enabled = false;
            _sprintGridMesh.enabled = false;
		}
	}

    // Function to calculate boundary vertices based on walkable positions
    public Dictionary<Vector3, Vector3> GetBoundaryEdges(List<Vector2> walkablePositions, float cellSize, List<Vector2> excludePositions = null)
    {
        HashSet<Vector2> walkableSet = new HashSet<Vector2>(walkablePositions);
        HashSet<Vector2> excludeSet = excludePositions != null ? new HashSet<Vector2>(excludePositions) : new HashSet<Vector2>();

        Dictionary<Vector3, Vector3> edges = new Dictionary<Vector3, Vector3>();

        // Directions to check neighboring cells (up, right, down, left)
        Vector2[] directions = {
            Vector2.up, Vector2.right, Vector2.down, Vector2.left
        };

        Vector3 lastPoint = walkablePositions[0];

        foreach (var position in walkablePositions)
        {
            // Check each direction to see if the neighbor is walkable
            foreach (var dir in directions)
            {
                Vector2 neighborPos = position + dir * cellSize;

                // This is a boundary if the neighbor is not in the current area or is in the exclusion set
                if (!walkableSet.Contains(neighborPos) || excludeSet.Contains(neighborPos))
                {
                    // This is a boundary, calculate the corresponding corner vertices
                    Vector3 topLeft = new Vector3(position.x - cellSize / 2, heightOffGround, position.y + cellSize / 2);
                    Vector3 topRight = new Vector3(position.x + cellSize / 2, heightOffGround, position.y + cellSize / 2);
                    Vector3 bottomLeft = new Vector3(position.x - cellSize / 2, heightOffGround, position.y - cellSize / 2);
                    Vector3 bottomRight = new Vector3(position.x + cellSize / 2, heightOffGround, position.y - cellSize / 2);

                    if (dir == Vector2.up)
                    {
                        if (!edges.ContainsKey(topLeft))
                        {
                            edges.Add(topLeft, topRight);
                            lastPoint = topRight;
                        }
                    }
                    else if (dir == Vector2.right)
                    {
                        if (!edges.ContainsKey(topRight))
						{
                            edges.Add(topRight, bottomRight);
                            lastPoint = bottomRight;
						}
                    }
                    else if (dir == Vector2.down)
                    {
                        if (!edges.ContainsKey(bottomRight))
						{
                            edges.Add(bottomRight, bottomLeft);
                            lastPoint = bottomLeft;
						}
                    }
                    else if (dir == Vector2.left)
                    {
                        if (!edges.ContainsKey(bottomLeft))
						{
                            edges.Add(bottomLeft, topLeft);
                            lastPoint = topLeft;
						}
                    }
                }
            }
        }

/*        if (!edges.ContainsKey(lastPoint))
		{
            FindNearestPointInOtherBoundaries()
		}*/

        return edges;
    }

    public List<Vector3> SortVertices(Dictionary<Vector3, Vector3> edges)
    {
        List<Vector3> sortedVertices = new List<Vector3>();

        // Make a copy of the edges so we can remove items from it
        // without destroying the original collection
        var copy = new Dictionary<Vector3, Vector3>(edges); 

        // Add the first pair before starting the loop
        var previousEdge = edges.First();

        sortedVertices.Add(previousEdge.Key);
        sortedVertices.Add(previousEdge.Value);

        print("Edges " + edges.Count);

        // While there is an edge that follows the previous one
        while (copy.ContainsKey(previousEdge.Value))
        {
            KeyValuePair<Vector3, Vector3> currentEdge = new KeyValuePair<Vector3, Vector3>(previousEdge.Value, copy[previousEdge.Value]);

            // Add the vertex to the list and continue
            sortedVertices.Add(currentEdge.Value);
            previousEdge = currentEdge;

            // Remove traversed nodes
            copy.Remove(currentEdge.Key);
        }

        print("vertex count = " + edges.Count * 2 + ", sorted vertex count = " + sortedVertices.Count);
        return sortedVertices;
    }

    private void GenerateAreaMesh(List<Vector2> points, float squareWidth, Material material, bool isSprint)
    {
        Transform transformToUse = (isSprint ? _sprintableOutlineRoot : _walkableOutlineRoot);

        // Ensure the GameObject has a MeshFilter component
        MeshFilter meshFilter = transformToUse.GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            meshFilter = transformToUse.gameObject.AddComponent<MeshFilter>();
        }

        // Ensure the GameObject has a MeshRenderer component
        _gridMesh = _walkableOutlineRoot.GetComponent<MeshRenderer>();
        _sprintGridMesh = _sprintableOutlineRoot.GetComponent<MeshRenderer>();

        if (_gridMesh == null)
        {
            _gridMesh = _walkableOutlineRoot.gameObject.AddComponent<MeshRenderer>();
            _gridMesh.renderingLayerMask = 256; //moveable area index
        }

        if (_sprintGridMesh == null)
        {
            _sprintGridMesh = _sprintableOutlineRoot.gameObject.AddComponent<MeshRenderer>();
            _sprintGridMesh.renderingLayerMask = 512; //moveable area index
        }

        MeshRenderer renderer = isSprint ? _sprintGridMesh : _gridMesh;

        //ensure grid mesh is active 
        renderer.enabled = true;

        // Set the material if one is provided
        if (material != null)
        {
            renderer.material = material;
        }

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // For each point, generate a square
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];

            // Calculate half-width for square centering
            float halfWidth = squareWidth / 2f;

            // Define the 4 vertices of the square in clockwise order
            Vector3 topLeft = new Vector3(point.x - halfWidth, heightOffGround, point.y + halfWidth);
            Vector3 topRight = new Vector3(point.x + halfWidth, heightOffGround, point.y + halfWidth);
            Vector3 bottomRight = new Vector3(point.x + halfWidth, heightOffGround, point.y - halfWidth);
            Vector3 bottomLeft = new Vector3(point.x - halfWidth, heightOffGround, point.y - halfWidth);

            // Add the vertices to the list
            vertices.Add(topLeft);
            vertices.Add(topRight);
            vertices.Add(bottomRight);
            vertices.Add(bottomLeft);

            // Add two triangles to form the square
            int vertexIndex = i * 4;
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        // Assign vertices and triangles to the mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        // Assign the mesh to the MeshFilter component
        meshFilter.mesh = mesh;
    }
}
