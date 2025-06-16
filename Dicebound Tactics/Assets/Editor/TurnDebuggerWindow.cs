using UnityEngine;
using UnityEditor;
using TacticsToolkit;
using System.Linq;

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
        EditorGUILayout.LabelField("AP: " + entity.GetStat(Stats.ActionPoints).statValue);
        EditorGUILayout.LabelField("Rollover AP: " + entity.GetStat(Stats.CarriedOverActionPoints).statValue);
        EditorGUILayout.LabelField("Alive: " + (entity.isAlive ? "Yes" : "No"));
        EditorGUILayout.Space();
        if (entity.equippedDice != null)
        {
            EditorGUILayout.LabelField("Dice Sides:");
            for (int i = 0; i < entity.equippedDice.sides.Count; i++)
            {
                var side = entity.equippedDice.sides[i];
                string label = $"[{i + 1}] Value: {side.value}";
                if (side.modifier != null)
                    label += $" | {side.modifier.modifierName}";

                EditorGUILayout.LabelField(label);
            }

            if (GUILayout.Button("Roll Dice"))
            {
                int amount = entity.RollDice();
                entity.statsContainer.ActionPoints.statValue = amount;
                Debug.Log($"{entity.name} rolled: {amount} AP");
            }
        }

        EditorGUILayout.Space();

        if (entity.abilities != null && entity.abilities.Count > 0)
        {
            EditorGUILayout.LabelField("Abilities", EditorStyles.boldLabel);
            foreach (var ability in entity.abilities)
            {
                bool canUse = entity.GetStat(Stats.ActionPoints).statValue >= ability.apCost;
                GUI.enabled = canUse && entity.isAlive;

                if (GUILayout.Button($"Use {ability.abilityName}"))
                {
                    if (turnManager.playerTurn)
                    {
                        var targets = turnManager.enemyUnits.Where(p => p != null && p.isAlive).ToList();
                        if (targets.Count > 0)
                        {
                            ability.Execute(entity, targets[0]);
                        }
                    }
                    else
                    {
                        var targets = turnManager.playerUnits.Where(e => e != null && e.isAlive).ToList();
                        if (targets.Count > 0)
                        {
                            ability.Execute(entity, targets[0]);
                        }
                    }
                }
            }
            GUI.enabled = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health Management", EditorStyles.boldLabel);
        GUI.enabled = entity.isAlive;
        if (GUILayout.Button("Damage 10"))
        {
            entity.TakeDamage(10);
        }

        if (GUILayout.Button("Heal 10"))
        {
            entity.HealEntity(10);
        }

        if (GUILayout.Button("Kill"))
        {
            entity.TakeDamage(9999);
        }

        // Only show "End Turn" if this entity is the current unit
        if (turnManager.GetCurrentUnit() == entity && entity.isAlive)
        {
            GUI.color = Color.cyan;
            if (GUILayout.Button("End Turn"))
            {
                turnManager.AdvanceTurn();
            }
            GUI.color = Color.white;
        }

        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
}
