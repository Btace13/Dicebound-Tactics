using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurveToTexture1D : MonoBehaviour
{
    [Header("Curve Settings")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public int resolution = 256;

    [Header("Texture Settings")]
    public bool generateOnStart = true;
    public bool clamp = true;
    public bool pointFilter = true;

#if UNITY_EDITOR
    [ContextMenu("Generate and Save Texture Asset")]
    public void GenerateAndSave()
    {
        var tex = GenerateTexture();

        string path = EditorUtility.SaveFilePanelInProject(
            "Save Curve Texture", "EyelidCurve", "png", "Save curve texture", "Assets/Scripts/Character"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.wrapMode = clamp ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                importer.filterMode = pointFilter ? FilterMode.Point : FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            Debug.Log($"Saved curve texture to {path}");
        }
    }
#endif

    private void Start()
    {
        if (generateOnStart)
        {
            var tex = GenerateTexture();
            // Example use: assign to a material
            GetComponent<Renderer>().material.SetTexture("_EyelidCurve", tex);
        }
    }

    public Texture2D GenerateTexture()
    {
        Texture2D tex = new Texture2D(resolution, 1, TextureFormat.RFloat, false, true);
        tex.wrapMode = clamp ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
        tex.filterMode = pointFilter ? FilterMode.Point : FilterMode.Bilinear;

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            float value = Mathf.Clamp01(curve.Evaluate(t));
            tex.SetPixel(i, 0, new Color(value, 0, 0, 1));
        }

        tex.Apply();
        return tex;
    }
}
