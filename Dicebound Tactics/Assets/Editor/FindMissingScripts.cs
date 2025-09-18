// Assets/Editor/FindMissingScripts.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts/Find in Open Scenes")]
    public static void FindInOpenScenes()
    {
        int missingCount = 0;
        var foundObjects = new List<Object>();

        // Get all scene GameObjects (includes inactive)
        var allGos = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allGos)
        {
            // filter out assets / prefabs: scene objects will have no asset path
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))) continue;
            // also ignore internal/editor-only objects
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave) continue;

            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    missingCount++;
                    string msg = $"Missing script on GameObject: {GetFullPath(go)} (Component index {i})";
                    Debug.LogWarning(msg, go);
                    foundObjects.Add(go);
                }
            }
        }

        if (foundObjects.Count > 0) Selection.objects = foundObjects.ToArray();
        Debug.Log($"FindMissingScripts: Found {missingCount} missing scripts in open scenes.");
    }

    [MenuItem("Tools/Find Missing Scripts/Find in Project Prefabs")]
    public static void FindInProjectPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int missingCount = 0;
        var reported = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) continue;

            var allChildren = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                var go = t.gameObject;
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null)
                    {
                        missingCount++;
                        Debug.LogWarning($"Missing script in Prefab: {path} -> {GetFullPath(go, root.transform)} (index {i})");
                        reported++;
                        // limit spam if desired:
                        if (reported > 500) { Debug.Log("Stopping early to avoid huge log spam."); break; }
                    }
                }
                if (reported > 500) break;
            }

            PrefabUtility.UnloadPrefabContents(root);
            if (reported > 500) break;
        }

        Debug.Log($"FindMissingScripts: Found {missingCount} missing scripts in project prefabs (scanned {guids.Length} prefabs).");
    }

    static string GetFullPath(GameObject go, Transform root = null)
    {
        // If root provided (prefab scan) remove the root prefix
        string path = go.name;
        var t = go.transform.parent;
        while (t != null && t != root)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        if (root != null && t == root) path = root.name + "/" + path;
        else if (t == null && root != null) ; // root not ancestor
        else
        {
            // scene object: walk to topmost parent
            t = go.transform.parent;
            while (t != null)
            {
                path = t.name + "/" + path;
                t = t.parent;
            }
        }
        return path;
    }
}
