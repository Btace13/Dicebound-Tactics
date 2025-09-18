using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurrencyDisplay))]
public class CurrencyDisplayEditor : Editor
{
    private CurrencyDisplay currencyDisplay;
    private SerializedProperty currencyTypeProperty;

    private void OnEnable()
    {
        currencyDisplay = (CurrencyDisplay)target;
        currencyTypeProperty = serializedObject.FindProperty("currencyType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
        EditorGUILayout.LabelField("Currency Display", headerStyle);
        EditorGUILayout.Space();

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Currency Configuration Info
        if (CurrencyConfiguration.Instance != null)
        {
            var currencyType = (CurrencyType)currencyTypeProperty.enumValueIndex;
            var configData = CurrencyConfiguration.Instance.GetCurrencyData(currencyType);
            
            if (configData != null)
            {
                EditorGUILayout.LabelField("Configuration Preview:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Icon:", GUILayout.Width(50));
                if (configData.icon != null)
                {
                    var iconTexture = AssetPreview.GetAssetPreview(configData.icon);
                    if (iconTexture != null)
                    {
                        GUILayout.Label(iconTexture, GUILayout.Width(32), GUILayout.Height(32));
                    }
                    EditorGUILayout.LabelField(configData.icon.name);
                }
                else
                {
                    EditorGUILayout.LabelField("No icon assigned", EditorStyles.miniLabel);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Color:", GUILayout.Width(50));
                EditorGUILayout.ColorField(configData.displayColor, GUILayout.Width(50));
                EditorGUILayout.LabelField($"RGB({configData.displayColor.r:F2}, {configData.displayColor.g:F2}, {configData.displayColor.b:F2})");
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField($"Display Name: {configData.displayName}");
                EditorGUILayout.LabelField($"Short Name: {configData.shortName}");
                
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox($"No configuration found for {currencyType}. Create a CurrencyConfiguration asset.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("CurrencyConfiguration not found! Create one in Resources folder.", MessageType.Error);
            
            if (GUILayout.Button("Create Currency Configuration"))
            {
                CreateCurrencyConfiguration();
            }
        }

        EditorGUILayout.Space();

        // Quick setup buttons
        EditorGUILayout.LabelField("Quick Setup:", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Apply Configuration to This Display"))
        {
            if (Application.isPlaying)
            {
                currencyDisplay.SetCurrencyType((CurrencyType)currencyTypeProperty.enumValueIndex);
            }
            else
            {
                // In edit mode, we can't call the method, but we can show a message
                EditorUtility.DisplayDialog("Apply Configuration", 
                    "Configuration will be applied automatically when the game starts, or you can test it in Play Mode.", "OK");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void CreateCurrencyConfiguration()
    {
        // Create Resources folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        // Create the configuration asset
        var config = ScriptableObject.CreateInstance<CurrencyConfiguration>();
        
        // Auto-setup currency types
        var setupMethod = typeof(CurrencyConfiguration).GetMethod("AutoSetupCurrencyTypes", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        setupMethod?.Invoke(config, null);

        AssetDatabase.CreateAsset(config, "Assets/Resources/CurrencyConfiguration.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = config;
        
        EditorUtility.DisplayDialog("Currency Configuration Created", 
            "CurrencyConfiguration asset created in Resources folder. You can now assign icons and colors.", "OK");
    }
}