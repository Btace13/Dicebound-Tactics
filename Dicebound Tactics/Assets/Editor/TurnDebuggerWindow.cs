using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TacticsToolkit;

public class TurnDebuggerWindow : EditorWindow
{
    private TurnManager turnManager;
    private Vector2 scrollPosition;
    private bool autoAdvance = false;

    [MenuItem("Tools/Turn Debugger")]
    public static void ShowWindow()
    {
        GetWindow<TurnDebuggerWindow>("Turn Debugger");
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        FindTurnManager();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            FindTurnManager();
        }
    }

    private void FindTurnManager()
    {
        turnManager = FindAnyObjectByType<TurnManager>();
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to debug turns.", MessageType.Info);
            return;
        }

        if (turnManager == null)
        {
            EditorGUILayout.HelpBox("No TurnManager found in scene.", MessageType.Warning);
            if (GUILayout.Button("Find TurnManager"))
            {
                FindTurnManager();
            }
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Current Turn Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Turn Side", turnManager.playerTurn ? "Player" : "Enemies");
        EditorGUILayout.LabelField("Auto Advance", autoAdvance.ToString());
        EditorGUILayout.Space();

        if (GUILayout.Button("Next Turn"))
        {
            turnManager.AdvanceTurn();
        }

        if (GUILayout.Button("Reset Turn Order"))
        {
            turnManager.ResetBattle();
        }

        autoAdvance = EditorGUILayout.Toggle("Auto Advance", autoAdvance);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Player Characters Row
        EditorGUILayout.LabelField("Player Characters", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var character in turnManager.playerUnits)
        {
            DrawEntityBox(character);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // Enemies Row
        EditorGUILayout.LabelField("Enemies", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var enemy in turnManager.enemyUnits)
        {
            DrawEntityBox(enemy);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void DrawEntityBox(Entity entity)
    {
        EditorGUILayout.BeginVertical("box", GUILayout.Width(150));

        EditorGUILayout.LabelField(entity.name, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("HP: " + entity.GetStat(Stats.CurrentHealth).statValue + "/" + entity.GetStat(Stats.Health).statValue);
        EditorGUILayout.LabelField("Mana: " + entity.GetStat(Stats.CurrentMana).statValue);
        EditorGUILayout.LabelField("Status: " + (entity.isAlive ? (entity.hasActed ? "Acted" : "Ready") : "Dead"));

        GUI.enabled = entity.isAlive;
        if (GUILayout.Button("Damage 10"))
        {
            entity.TakeDamage(10);
        }

        if (GUILayout.Button("Heal 10"))
        {
            entity.HealEntity(10);
        }

        if (GUILayout.Button("Set Acted"))
        {
            entity.hasActed = true;
        }

        if (GUILayout.Button("Reset Acted"))
        {
            entity.hasActed = false;
        }

        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
}