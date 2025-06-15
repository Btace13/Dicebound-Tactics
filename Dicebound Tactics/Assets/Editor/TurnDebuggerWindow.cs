using UnityEditor;
using UnityEngine;
using TacticsToolkit;

public class TurnDebuggerWindow : EditorWindow
{
    private TurnManager turnManager;
    private Vector2 scroll;
    private bool autoAdvanceTurn = false;
    private string actionLog = "";

    [MenuItem("Tools/Turn Debugger")]
    public static void ShowWindow()
    {
        GetWindow<TurnDebuggerWindow>("Turn Debugger");
    }

    private void OnGUI()
    {
        GUILayout.Label("JRPG Turn Debugger", EditorStyles.boldLabel);

        turnManager = (TurnManager)EditorGUILayout.ObjectField("Turn Manager", turnManager, typeof(TurnManager), true);

        if (turnManager == null)
        {
            EditorGUILayout.HelpBox("Assign a TurnManager to begin tracking.", MessageType.Info);
            return;
        }

        GUILayout.Space(10);
        GUILayout.Label("Current Turn", EditorStyles.boldLabel);
        GUILayout.Label(turnManager.playerTurn ? "PLAYER TURN" : "ENEMY TURN", EditorStyles.helpBox);

        if (GUILayout.Button("Next Turn"))
        {
            turnManager.AdvanceTurn();
            actionLog += $"Advanced to {(turnManager.playerTurn ? "Player" : "Enemy")} Turn\n";
        }

        if (GUILayout.Button("Reset Battle"))
        {
            turnManager.ResetBattle();
            actionLog += "Battle Reset\n";
        }

        autoAdvanceTurn = EditorGUILayout.Toggle("Auto Advance", autoAdvanceTurn);

        GUILayout.Space(10);
        scroll = EditorGUILayout.BeginScrollView(scroll);

        GUILayout.Label("Player Units", EditorStyles.boldLabel);
        foreach (var unit in turnManager.playerUnits)
        {
            if (unit == null) continue;
            DrawEntityControls(unit);
        }

        GUILayout.Space(10);
        GUILayout.Label("Enemy Units", EditorStyles.boldLabel);
        foreach (var unit in turnManager.enemyUnits)
        {
            if (unit == null) continue;
            DrawEntityControls(unit);
        }

        GUILayout.Space(10);
        GUILayout.Label("Action Log", EditorStyles.boldLabel);
        EditorGUILayout.TextArea(actionLog, GUILayout.Height(100));

        EditorGUILayout.EndScrollView();
    }

    private void DrawEntityControls(Entity entity)
    {
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label(entity.name, EditorStyles.boldLabel);
        GUILayout.Label($"Alive: {entity.isAlive}");
        GUILayout.Label($"Stunned: {entity.isStunned}");
        GUILayout.Label($"Has Acted: {entity.hasActed}");
        GUILayout.Label($"HP: {entity.GetStat(Stats.CurrentHealth).statValue}/{entity.GetStat(Stats.Health).statValue}");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Take 10 DMG"))
        {
            entity.TakeDamage(10);
            actionLog += $"{entity.name} took 10 damage.\n";
        }
        if (GUILayout.Button("Heal 10"))
        {
            entity.HealEntity(10);
            actionLog += $"{entity.name} healed 10 HP.\n";
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Mark As Acted"))
        {
            entity.hasActed = true;
            actionLog += $"{entity.name} marked as acted.\n";
        }

        if (GUILayout.Button("Kill Unit"))
        {
            entity.TakeDamage(9999);
            actionLog += $"{entity.name} was slain.\n";
        }

        EditorGUILayout.EndVertical();
    }
}
