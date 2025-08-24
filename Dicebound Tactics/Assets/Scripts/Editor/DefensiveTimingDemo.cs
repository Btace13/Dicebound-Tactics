using UnityEngine;
using UnityEditor;

public class DefensiveTimingDemo : EditorWindow
{
    [MenuItem("Tools/Setup Defensive Timing Demo")]
    public static void ShowWindow()
    {
        GetWindow<DefensiveTimingDemo>("Demo Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Defensive Timing Demo Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Complete Demo Setup"))
        {
            CreateDemoSetup();
        }

        GUILayout.Space(10);
        GUILayout.Label("This will create:", EditorStyles.boldLabel);
        GUILayout.Label("• UI Canvas with Defensive Timing UI");
        GUILayout.Label("• Button Prompt Prefab");
        GUILayout.Label("• Test DamageAbilitySO asset");
    }

    private void CreateDemoSetup()
    {
        // Create UI first
        DefensiveTimingUISetup.ShowWindow();
        
        // Create button prompt prefab
        ButtonPromptPrefabCreator.ShowWindow();
        
        Debug.Log("Demo setup started!");
        Debug.Log("Next steps:");
        Debug.Log("1. Use the opened windows to create UI and prefab");
        Debug.Log("2. Assign button icons and prefab reference");
        Debug.Log("3. Test with an enemy attacking a player character");
    }
}
