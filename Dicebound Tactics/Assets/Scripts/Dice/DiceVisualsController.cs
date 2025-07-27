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
            case DiceType.D4:
                GenerateD4Numbers();
                break;
            case DiceType.D6:
                GenerateD6Numbers();
                break;
            case DiceType.D8:
                GenerateD8Numbers();
                break;
            case DiceType.D10:
                GenerateD10Numbers();
                break;
            case DiceType.D12:
                GenerateD12Numbers();
                break;
            case DiceType.D20:
                GenerateD20Numbers();
                break;
        }
    }

    [Button("Clear Dice Numbers")]
    public async void ClearDiceNumbers()
    {
        // Get all children of parent that start with "Face_"

        if (parent == null)
        {
            Debug.LogWarning("Parent transform is not set. Cannot clear dice numbers.");
            return;
        }

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.StartsWith("Face_"))
            {
                DestroyImmediate(children[i].gameObject);
                await System.Threading.Tasks.Task.Yield();
            }
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
            Vector3 rotatedNormal = Quaternion.Euler(rotationOffset) * normals[i];
            go.transform.localPosition = rotatedNormal * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedNormal, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
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
            Vector3 rotatedCenter = Quaternion.Euler(rotationOffset) * center;
            go.transform.localPosition = rotatedCenter * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedCenter, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
        }
    }

    void GenerateD4Numbers()
    {
        // Vertices of a regular tetrahedron
        float sqrt2over3 = Mathf.Sqrt(2f / 3f);
        float sqrt6over3 = Mathf.Sqrt(6f) / 3f;
        Vector3[] verts = new Vector3[]
        {
            new Vector3(0, 1, 0),
            new Vector3(2 * Mathf.Sqrt(2f) / 3f, -1f / 3f, 0),
            new Vector3(-Mathf.Sqrt(2f) / 3f, -1f / 3f, Mathf.Sqrt(6f) / 3f),
            new Vector3(-Mathf.Sqrt(2f) / 3f, -1f / 3f, -Mathf.Sqrt(6f) / 3f)
        };
        int[][] faces = new int[][]
        {
            new int[]{0,1,2}, new int[]{0,3,1}, new int[]{0,2,3}, new int[]{1,3,2}
        };
        int[] faceNumbers = { 1, 2, 3, 4 }; // Standard D4 numbering (can be customized)
        for (int i = 0; i < 4; i++)
        {
            Vector3 v0 = verts[faces[i][0]];
            Vector3 v1 = verts[faces[i][1]];
            Vector3 v2 = verts[faces[i][2]];
            Vector3 center = (v0 + v1 + v2) / 3f;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            Vector3 rotatedCenter = Quaternion.Euler(rotationOffset) * center;
            go.transform.localPosition = rotatedCenter * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedCenter, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
        }
    }

    void GenerateD8Numbers()
    {
        // Vertices of a regular octahedron
        Vector3[] verts = new Vector3[]
        {
            new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,1,0), new Vector3(0,-1,0), new Vector3(0,0,1), new Vector3(0,0,-1)
        };
        int[][] faces = new int[][]
        {
            new int[]{0,2,4}, new int[]{2,1,4}, new int[]{1,3,4}, new int[]{3,0,4},
            new int[]{0,5,2}, new int[]{2,5,1}, new int[]{1,5,3}, new int[]{3,5,0}
        };
        int[] faceNumbers = { 1, 2, 3, 4, 5, 6, 7, 8 };
        for (int i = 0; i < 8; i++)
        {
            Vector3 v0 = verts[faces[i][0]];
            Vector3 v1 = verts[faces[i][1]];
            Vector3 v2 = verts[faces[i][2]];
            Vector3 center = (v0 + v1 + v2) / 3f;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            Vector3 rotatedCenter = Quaternion.Euler(rotationOffset) * center;
            go.transform.localPosition = rotatedCenter * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedCenter, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
        }
    }

    void GenerateD10Numbers()
    {
        // Vertices of a pentagonal trapezohedron (D10)
        float angleStep = Mathf.PI * 2f / 10f;
        float h = Mathf.Cos(angleStep / 2f);
        float r = Mathf.Sin(angleStep / 2f);
        Vector3[] verts = new Vector3[10];
        for (int i = 0; i < 10; i++)
        {
            float angle = i * angleStep;
            verts[i] = new Vector3(Mathf.Cos(angle) * r, (i % 2 == 0 ? h : -h), Mathf.Sin(angle) * r);
        }
        // Each face is a kite between two adjacent top and bottom points
        int[][] faces = new int[10][];
        for (int i = 0; i < 10; i++)
        {
            int next = (i + 1) % 10;
            faces[i] = new int[] { i, next, (i + 2) % 10 };
        }
        int[] faceNumbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; // Standard D10 numbering (can be customized)
        for (int i = 0; i < 10; i++)
        {
            Vector3 v0 = verts[faces[i][0]];
            Vector3 v1 = verts[faces[i][1]];
            Vector3 v2 = verts[faces[i][2]];
            Vector3 center = (v0 + v1 + v2) / 3f;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            Vector3 rotatedCenter = Quaternion.Euler(rotationOffset) * center;
            go.transform.localPosition = rotatedCenter * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedCenter, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
        }
    }

    void GenerateD12Numbers()
    {
        // Regular dodecahedron vertices
        float phi = (1 + Mathf.Sqrt(5)) / 2f; // Golden ratio
        float a = 1f;
        float b = 1f / phi;
        float c = 2f - phi;
        Vector3[] verts = new Vector3[]
        {
            new Vector3( a,  a,  a), new Vector3( a,  a, -a), new Vector3( a, -a,  a), new Vector3( a, -a, -a),
            new Vector3(-a,  a,  a), new Vector3(-a,  a, -a), new Vector3(-a, -a,  a), new Vector3(-a, -a, -a),
            new Vector3( 0,  b,  c), new Vector3( 0,  b, -c), new Vector3( 0, -b,  c), new Vector3( 0, -b, -c),
            new Vector3( b,  c,  0), new Vector3( b, -c,  0), new Vector3(-b,  c,  0), new Vector3(-b, -c,  0),
            new Vector3( c,  0,  b), new Vector3(-c,  0,  b), new Vector3( c,  0, -b), new Vector3(-c,  0, -b)
        };
        // Each face is a pentagon (5 indices into verts)
        int[][] faces = new int[][]
        {
            new int[]{0,8,10,2,16}, new int[]{0,16,18,1,12}, new int[]{0,12,14,4,8}, new int[]{8,4,17,6,10}, new int[]{10,6,13,2,10},
            new int[]{2,13,3,18,16}, new int[]{1,18,3,19,9}, new int[]{1,9,5,14,12}, new int[]{4,14,5,17,8}, new int[]{6,17,5,9,7},
            new int[]{6,7,15,13,6}, new int[]{3,13,15,7,19},
        };
        int[] faceNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        // Find normal of first face and rotate so it points forward
        Vector3 v0 = verts[faces[0][0]];
        Vector3 v1 = verts[faces[0][1]];
        Vector3 v2 = verts[faces[0][2]];
        Vector3 faceNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        Quaternion alignToForward = Quaternion.FromToRotation(faceNormal, Vector3.forward);
        for (int i = 0; i < 12; i++)
        {
            Vector3 vv0 = alignToForward * verts[faces[i][0]];
            Vector3 vv1 = alignToForward * verts[faces[i][1]];
            Vector3 vv2 = alignToForward * verts[faces[i][2]];
            Vector3 vv3 = alignToForward * verts[faces[i][3]];
            Vector3 vv4 = alignToForward * verts[faces[i][4]];
            Vector3 center = (vv0 + vv1 + vv2 + vv3 + vv4) / 5f;
            GameObject go = new GameObject($"Face_{faceNumbers[i]}");
            go.transform.SetParent(parent);
            Vector3 rotatedCenter = Quaternion.Euler(rotationOffset) * center.normalized;
            go.transform.localPosition = rotatedCenter * radius + positionOffset;
            go.transform.localRotation = Quaternion.LookRotation(rotatedCenter, Vector3.up) * Quaternion.Euler(0, 180, 0);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = faceNumbers[i].ToString();
            tmp.fontSize = faceSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.font = fontAsset;
            SetColorValue(tmp, faceColor);
        }
    }

    private void SetColorValue(TextMeshPro tmp, Color color)
    {
        float dotProd = Vector3.Dot(transform.forward, tmp.transform.forward);

        color.a = dotProd > -0.7f ? 1 : 0;

        tmp.color = color;
    }
}
