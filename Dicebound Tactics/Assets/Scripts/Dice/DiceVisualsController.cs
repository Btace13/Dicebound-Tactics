using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;

public class DiceVisualsController : MonoBehaviour
{
    public float radius = 0.5f;
    public float faceSize = 1.0f; // Size of each face text
    public Color faceColor = Color.white;
    [HideInInspector] public Vector3 forward = Vector3.forward;
    [SerializeField] Vector3 positionOffset = Vector3.zero; // Offset for positioning dice faces
    [SerializeField] Vector3 rotationOffset = Vector3.zero; // Offset for face rotation
    [SerializeField, Required] Transform parent;

    public TMP_FontAsset fontAsset;
    public enum DiceType { D4, D6, D8, D10, D12, D20 }
    public DiceType diceType = DiceType.D6;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        forward = transform.forward;
        GenerateDiceNumbers();
    }

    [Button("Generate Dice Numbers")]
    public void GenerateDiceNumbers()
    {
        switch (diceType)
        {
            case DiceType.D6:
                GenerateD6Numbers();
                break;
            case DiceType.D20:
                GenerateD20Numbers();
                break;
                // Add cases for other dice types (D4, D8, D10, D12)
        }
    }

    [Button("Clear Dice Numbers")]
    public async void ClearDiceNumbers()
    {
        // Clear all child objects (dice faces)
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
            await System.Threading.Tasks.Task.Yield();
        }
    }

    void GenerateD6Numbers()
    {
        // D6 face normals (relative to forward/up/right)
        Vector3[] normals = new Vector3[]
        {
            Vector3.up,    // 1 (opposite 6)
            Vector3.down,  // 6 (opposite 1)
            Vector3.forward, // 2 (opposite 5)
            Vector3.back,    // 5 (opposite 2)
            Vector3.right,   // 3 (opposite 4)
            Vector3.left     // 4 (opposite 3)
        };
        int[] faceNumbers = { 1, 6, 2, 5, 3, 4 };
        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = transform.position + normals[i] * radius;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            go.transform.localPosition = normals[i] * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(normals[i], Vector3.up) * Quaternion.Euler(rotationOffset) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            tmp.color = faceColor;
        }
    }

    void GenerateD20Numbers()
    {
        // Golden ratio
        float phi = (1 + Mathf.Sqrt(5)) / 2;
        float a = 1f;
        float b = 1f / phi;
        // Vertices of an icosahedron
        Vector3[] verts = new Vector3[]
        {
            new Vector3( 0,  b, -a), new Vector3( b,  a, 0), new Vector3(-b,  a, 0), new Vector3( 0,  b,  a),
            new Vector3( 0, -b,  a), new Vector3(-a, 0,  b), new Vector3( 0, -b, -a), new Vector3( a, 0, -b),
            new Vector3( a, 0,  b), new Vector3(-a, 0, -b), new Vector3( b, -a, 0), new Vector3(-b, -a, 0)
        };
        // Each face is a triangle of 3 vertices (indices into verts)
        int[][] faces = new int[][]
        {
            new int[]{0,1,2}, new int[]{3,2,1}, new int[]{3,4,5}, new int[]{3,8,4}, new int[]{0,6,7},
            new int[]{0,9,6}, new int[]{4,10,11}, new int[]{6,11,10}, new int[]{2,5,9}, new int[]{11,9,5},
            new int[]{1,7,8}, new int[]{10,8,7}, new int[]{3,5,2}, new int[]{3,1,8}, new int[]{0,2,9},
            new int[]{0,7,1}, new int[]{6,9,11}, new int[]{6,10,7}, new int[]{4,11,5}, new int[]{4,8,10}
        };
        // Standard D20 numbering (opposite faces sum to 21)
        int[] faceNumbers = { 1, 20, 14, 7, 11, 10, 8, 13, 18, 3, 17, 4, 16, 5, 19, 2, 15, 6, 12, 9 };
        // Find rotation that aligns vertex 1 to Vector3.up
        Vector3 topVertex = verts[1].normalized;
        Quaternion alignToUp = Quaternion.FromToRotation(topVertex, Vector3.up);
        for (int i = 0; i < 20; i++)
        {
            Vector3 v0 = alignToUp * verts[faces[i][0]].normalized;
            Vector3 v1 = alignToUp * verts[faces[i][1]].normalized;
            Vector3 v2 = alignToUp * verts[faces[i][2]].normalized;
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
            Vector3 center = (v0 + v1 + v2) / 3f;
            Vector3 pos = transform.position + center * radius;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            go.transform.localPosition = center * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(center, Vector3.up) * Quaternion.Euler(rotationOffset) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            tmp.color = faceColor;
        }
    }

    // TODO: Implement GenerateD4Numbers, GenerateD8Numbers, etc.
    // For each, define face normals and number mapping, then instantiate TextMeshPro as above.
}
