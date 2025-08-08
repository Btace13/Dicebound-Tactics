#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(DiceModifierInventory))]
public class DiceModifierInventoryEditor : Editor
{
    // Temporary drag-and-drop field
    private List<DiceModifier> dragDropList = new List<DiceModifier>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiceModifierInventory inventory = (DiceModifierInventory)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

        // === Drag & Drop multiple DiceModifier assets ===
        EditorGUILayout.LabelField("Drag DiceModifier assets here:");
        int removeIndex = -1;
        for (int i = 0; i < dragDropList.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            dragDropList[i] = (DiceModifier)EditorGUILayout.ObjectField(dragDropList[i], typeof(DiceModifier), false);
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0)
            dragDropList.RemoveAt(removeIndex);

        if (GUILayout.Button("Add Dragged Modifiers to Inventory"))
        {
            foreach (var modifier in dragDropList)
            {
                if (modifier != null)
                    inventory.AddItem(modifier, 1);
            }
            dragDropList.Clear();
            EditorUtility.SetDirty(inventory);
        }

        EditorGUILayout.Space();

        // === Load from a folder ===
        if (GUILayout.Button("Load All DiceModifiers From Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Select Folder with DiceModifiers", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = "Assets" + path.Replace(Application.dataPath, "");
                string[] guids = AssetDatabase.FindAssets("t:DiceModifier", new[] { relativePath });
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    DiceModifier modifier = AssetDatabase.LoadAssetAtPath<DiceModifier>(assetPath);
                    if (modifier != null)
                        inventory.AddItem(modifier, 1);
                }
                EditorUtility.SetDirty(inventory);
            }
        }
    }
}
#endif
