using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DefensiveTimingUISetup : EditorWindow
{
    [MenuItem("Tools/Setup Defensive Timing UI")]
    public static void ShowWindow()
    {
        GetWindow<DefensiveTimingUISetup>("Defensive Timing UI Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Defensive Timing UI Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Defensive Timing UI"))
        {
            CreateDefensiveTimingUI();
        }

        GUILayout.Space(10);
        GUILayout.Label("Instructions:", EditorStyles.boldLabel);
        GUILayout.Label("1. Click 'Create Defensive Timing UI'");
        GUILayout.Label("2. Assign button icons in the inspector");
        GUILayout.Label("3. Create a button prompt prefab");
        GUILayout.Label("4. Assign the prefab reference");
    }

    private void CreateDefensiveTimingUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create main panel
        GameObject panel = new GameObject("DefensiveTimingPanel");
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Add background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(panel.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create instruction text
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(panel.transform, false);
        TextMeshProUGUI instructionText = instructionObj.AddComponent<TextMeshProUGUI>();
        instructionText.text = "Press the button sequence!";
        instructionText.fontSize = 24;
        instructionText.alignment = TextAlignmentOptions.Center;
        
        RectTransform instructionRect = instructionText.GetComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0, 0.7f);
        instructionRect.anchorMax = new Vector2(1, 0.9f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;

        // Create timer bar
        GameObject timerObj = new GameObject("TimerBar");
        timerObj.transform.SetParent(panel.transform, false);
        Image timerImage = timerObj.AddComponent<Image>();
        timerImage.type = Image.Type.Filled;
        timerImage.fillMethod = Image.FillMethod.Horizontal;
        timerImage.color = Color.green;
        
        RectTransform timerRect = timerImage.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.2f, 0.5f);
        timerRect.anchorMax = new Vector2(0.8f, 0.6f);
        timerRect.offsetMin = Vector2.zero;
        timerRect.offsetMax = Vector2.zero;

        // Create button container
        GameObject container = new GameObject("ButtonContainer");
        container.transform.SetParent(panel.transform, false);
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.2f, 0.2f);
        containerRect.anchorMax = new Vector2(0.8f, 0.4f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // Add DefensiveTimingUI component
        DefensiveTimingUI uiComponent = panel.AddComponent<DefensiveTimingUI>();
        
        // Set references using reflection to access private fields
        var fieldInfo = typeof(DefensiveTimingUI).GetField("defensivePromptPanel", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(uiComponent, panel);
        
        fieldInfo = typeof(DefensiveTimingUI).GetField("instructionText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(uiComponent, instructionText);
        
        fieldInfo = typeof(DefensiveTimingUI).GetField("timerFillBar", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(uiComponent, timerImage);
        
        fieldInfo = typeof(DefensiveTimingUI).GetField("buttonPromptContainer", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo?.SetValue(uiComponent, container.transform);

        // Initially hide the panel
        panel.SetActive(false);

        Debug.Log("Defensive Timing UI created! Don't forget to:");
        Debug.Log("1. Assign button icons in the DefensiveTimingUI inspector");
        Debug.Log("2. Create and assign a button prompt prefab");
        
        Selection.activeGameObject = panel;
    }
}
