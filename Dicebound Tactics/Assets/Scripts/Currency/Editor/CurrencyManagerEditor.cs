using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(CurrencyManager))]
public class CurrencyManagerEditor : Editor
{
    private CurrencyManager currencyManager;
    private SerializedProperty startingCurrencies;
    private SerializedProperty currentCurrencies;

    // Test values
    private CurrencyType testCurrencyType = CurrencyType.Gold;
    private int testAmount = 100;

    private void OnEnable()
    {
        currencyManager = (CurrencyManager)target;
        startingCurrencies = serializedObject.FindProperty("startingCurrencies");
        currentCurrencies = serializedObject.FindProperty("currentCurrencies");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        GUILayout.Label("Currency Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

        // Current currencies display (runtime only)
        if (Application.isPlaying && currencyManager != null)
        {
            EditorGUILayout.LabelField("Current Currencies:", EditorStyles.miniLabel);
            var currencies = currencyManager.GetAllCurrencies();
            foreach (var currency in currencies)
            {
                EditorGUILayout.LabelField($"  {currency.Key}: {currency.Value}");
            }
        }

        EditorGUILayout.Space();

        // Test controls
        EditorGUILayout.LabelField("Test Controls", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        testCurrencyType = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type:", testCurrencyType);
        testAmount = EditorGUILayout.IntField("Amount:", testAmount);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("Add Currency"))
        {
            currencyManager?.AddCurrency(testCurrencyType, testAmount);
        }
        
        if (GUILayout.Button("Spend Currency"))
        {
            currencyManager?.SpendCurrency(testCurrencyType, testAmount);
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();

        // Quick actions
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = Application.isPlaying;
        
        if (GUILayout.Button("Reset All"))
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyManager?.SetCurrency(type, 0);
            }
        }
        
        if (GUILayout.Button("Max All"))
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyManager?.SetCurrency(type, 9999);
            }
        }
        
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Runtime controls are only available during play mode.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}