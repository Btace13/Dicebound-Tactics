using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ButtonPromptPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Create Button Prompt Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ButtonPromptPrefabCreator>("Button Prompt Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Button Prompt Prefab Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Button Prompt Prefab"))
        {
            CreateButtonPromptPrefab();
        }
    }

    private void CreateButtonPromptPrefab()
    {
        // Create the button prompt GameObject
        GameObject buttonPrompt = new GameObject("ButtonPromptPrefab");
        
        // Add RectTransform
        RectTransform rectTransform = buttonPrompt.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(80, 80);

        // Add background image
        GameObject background = new GameObject("Background");
        background.transform.SetParent(buttonPrompt.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Add icon image
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(buttonPrompt.transform, false);
        Image iconImage = icon.AddComponent<Image>();
        iconImage.color = Color.white;
        
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;

        // Add ButtonPrompt component
        ButtonPrompt buttonPromptComponent = buttonPrompt.AddComponent<ButtonPrompt>();
        
        // Set references using reflection
        var fieldInfo = typeof(ButtonPrompt).GetField("buttonIcon", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(buttonPromptComponent, iconImage);
        
        fieldInfo = typeof(ButtonPrompt).GetField("backgroundImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(buttonPromptComponent, bgImage);

        // Create prefab
        string prefabPath = "Assets/Prefabs/UI/ButtonPromptPrefab.prefab";
        
        // Create directories if they don't exist
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(buttonPrompt, prefabPath);
        
        // Clean up the scene instance
        DestroyImmediate(buttonPrompt);
        
        Debug.Log($"Button Prompt Prefab created at: {prefabPath}");
        
        // Select the prefab
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }
}
