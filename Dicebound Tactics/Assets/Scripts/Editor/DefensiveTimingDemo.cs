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

        GUILayout.Space(5);

        if (GUILayout.Button("Create Audio Feedback System"))
        {
            CreateAudioFeedback();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Create Visual Effects System"))
        {
            CreateVisualEffects();
        }

        GUILayout.Space(10);
        GUILayout.Label("This will create:", EditorStyles.boldLabel);
        GUILayout.Label("• UI Canvas with Defensive Timing UI");
        GUILayout.Label("• Button Prompt Prefab");
        GUILayout.Label("• Audio and Visual Feedback Systems");
        GUILayout.Label("• Test DamageAbilitySO asset");
        GUILayout.Space(10);
        GUILayout.Label("All systems use EventManager for communication!", EditorStyles.helpBox);
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

    private void CreateAudioFeedback()
    {
        GameObject audioGO = new GameObject("DefensiveTimingAudio");
        audioGO.AddComponent<AudioSource>();
        
        // Use reflection to add the component
        var audioFeedbackType = System.Type.GetType("DefensiveTimingAudioFeedback");
        if (audioFeedbackType != null)
        {
            audioGO.AddComponent(audioFeedbackType);
        }
        
        Debug.Log("Audio feedback system created! Add DefensiveTimingAudioFeedback script and assign audio clips.");
        Selection.activeGameObject = audioGO;
    }

    private void CreateVisualEffects()
    {
        GameObject vfxGO = new GameObject("DefensiveTimingVFX");
        
        // Use reflection to add the component
        var vfxType = System.Type.GetType("DefensiveTimingVisualEffects");
        if (vfxType != null)
        {
            vfxGO.AddComponent(vfxType);
        }
        
        Debug.Log("Visual effects system created! Add DefensiveTimingVisualEffects script and assign references.");
        Selection.activeGameObject = vfxGO;
    }
}
