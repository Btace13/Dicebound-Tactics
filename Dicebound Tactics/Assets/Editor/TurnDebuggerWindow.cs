using UnityEngine;
using UnityEditor;
using TacticsToolkit;
using System.Linq;
using System.Collections.Generic;

public class TurnDebuggerWindow : EditorWindow
{
    private TurnManager turnManager;
    private SelectionController selectionController;
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
        selectionController = FindAnyObjectByType<SelectionController>();
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
        EditorGUILayout.LabelField("Current Unit", turnManager.GetCurrentUnit()?.name ?? "None");
        EditorGUILayout.LabelField("Turns Remaining", turnManager.GetRemainingTurns().ToString());
        EditorGUILayout.Space();

        if (GUILayout.Button("Next Turn"))
        {
            turnManager.StartNextTurn();
        }

        if (GUILayout.Button("Reset Turn Order"))
        {
            turnManager.ResetBattle();
        }

        if (GUILayout.Button("Toggle Selection Mode"))
        {
            selectionController?.ChangeSelectionType(!selectionController.cyclingEnemies);
        }

        autoAdvance = EditorGUILayout.Toggle("Auto Advance", autoAdvance);

        EditorGUILayout.Space();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Full Turn Order", EditorStyles.boldLabel);
        foreach (var entity in turnManager.GetFullTurnOrder())
        {
            EditorGUILayout.LabelField(entity.name, EditorStyles.label);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player Characters", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        foreach (var character in turnManager.playerUnits)
        {
            DrawEntityBox(character);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
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
        EditorGUILayout.BeginVertical("box", GUILayout.Width(180));

        if (entity.statsContainer != null)
        {
            EditorGUILayout.LabelField(entity.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Speed: {entity.GetStat(Stats.Speed).statValue}");
            EditorGUILayout.LabelField($"HP: {entity.GetStat(Stats.CurrentHealth).statValue}/{entity.GetStat(Stats.Health).statValue}");
            EditorGUILayout.LabelField($"Last Roll: {entity.equippedDice?.LastRollValue ?? 0}");
            EditorGUILayout.LabelField($"AP: {entity.GetStat(Stats.ActionPoints).statValue}");
            EditorGUILayout.LabelField($"Rollover AP: {entity.GetStat(Stats.CarriedOverActionPoints).statValue}");
            EditorGUILayout.LabelField($"Alive: {(entity.isAlive ? "Yes" : "No")}");
            EditorGUILayout.LabelField($"Used Item This Turn: {(entity.hasUsedItemThisTurn ? "Yes" : "No")}");
        }

        EditorGUILayout.Space();

        if (entity.equippedDice != null)
        {
            EditorGUILayout.LabelField("Dice Sides:");
            for (int i = 0; i < entity.equippedDice.sides.Count; i++)
            {
                var side = entity.equippedDice.sides[i];
                string label = $"[{i + 1}] Value: {side.value}";
                if (side.modifier != null)
                    label += $" | {side.modifier.Name}";
                EditorGUILayout.LabelField(label);
            }

            if (GUILayout.Button("Roll Dice"))
            {
                entity.RollDice();
                Debug.Log($"{entity.name} rolled: {entity.LastRollValue} AP");
            }
        }

        EditorGUILayout.Space();

        if (entity.abilityLoadout != null && entity.abilityLoadout.Count > 0)
        {
            EditorGUILayout.LabelField("Abilities", EditorStyles.boldLabel);
            foreach (var ability in entity.abilityLoadout)
            {
                bool canUse = entity.GetStat(Stats.ActionPoints).statValue >= ability.apCost;
                GUI.enabled = canUse && entity.isAlive;

                if (GUILayout.Button($"Use {ability.abilityName}"))
                {
                    var targets = (entity is CharacterManager)
                        ? turnManager.enemyUnits.Where(p => p != null && p.isAlive).Cast<Entity>().ToList()
                        : turnManager.playerUnits.Where(p => p != null && p.isAlive).Cast<Entity>().ToList();

                    foreach (var target in selectionController.SelectedEntities)
                    {
                        if (target != null && target.isAlive)
                        {
                            ability.Execute(entity, target);
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

        if (turnManager.GetCurrentUnit() == entity && entity.isAlive)
        {
            GUI.color = Color.cyan;
            if (GUILayout.Button("End Turn"))
            {
                turnManager.StartNextTurn();
            }
            GUI.color = Color.white;
        }

        GUI.enabled = true;
        EditorGUILayout.EndVertical();
    }
}
