using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TacticsToolkit;
using Sirenix.OdinInspector;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<CharacterManager> playerUnits = new();
    public List<EnemyManager> enemyUnits = new();
    public bool GameIsPlaying;

    [Header("Events")]
    public GameEvent BattleStarted;
    public GameEventGameObject startNewTurn;
    public GameEventCharacterManager startNewCharacterTurn;
    public GameEventEnemyManager startNewEnemyTurn;
    public GameEvent GameEnded;

    private List<Entity> turnOrder = new();
    private int currentTurnIndex = 0;
    private Entity currentUnit;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (!GameIsPlaying)
            return;

        if (enemyUnits.All(e => !e.isAlive))
        {
            ShowBattleEndedDialog(true);
            return;
        }

        if (playerUnits.All(p => !p.isAlive))
        {
            ShowBattleEndedDialog(false);
            return;
        }
    }

    [Button("Start Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void StartBattle()
    {
        GameIsPlaying = true;
        BattleStarted?.Raise();

        foreach (var character in playerUnits)
        {
            if (character.isAlive)
            {
                character.ResetActionPoints();
                character.ResetTempModifiers();
            }
        }

        foreach (var enemy in enemyUnits)
        {
            if (enemy.isAlive)
            {
                enemy.ResetActionPoints();
                enemy.ResetTempModifiers();
            }
        }

        BuildTurnOrder();
        StartNextTurn();
    }

    private void BuildTurnOrder()
    {
        turnOrder.Clear();
        var allUnits = new List<Entity>();
        allUnits.AddRange(playerUnits);
        allUnits.AddRange(enemyUnits);

        turnOrder = allUnits
            .Where(u => u != null && u.isAlive)
            .OrderByDescending(u => u.GetStat(Stats.Speed).statValue)
            .ThenByDescending(u => Random.value) // Simple tiebreaker
            .ToList();

        currentTurnIndex = 0;

        turnOrder.ForEach(unit => unit.RollDice());
    }

    private void StartNextTurn()
    {
        if (!GameIsPlaying || turnOrder.Count == 0)
            return;

        while (currentTurnIndex < turnOrder.Count)
        {
            var unit = turnOrder[currentTurnIndex];
            currentTurnIndex++;

            if (unit != null && unit.isAlive)
            {
                currentUnit = unit;
                unit.StartTurn();
                startNewTurn.Raise(unit.gameObject);

                if (unit is CharacterManager character)
                    startNewCharacterTurn?.Raise(character);
                else if (unit is EnemyManager enemy)
                {
                    startNewEnemyTurn?.Raise(enemy);
                    enemy.BeginAITurn();
                }
                return;
            }
        }

        BuildTurnOrder();
        StartNextTurn();
    }

    public void EndCharacterTurn(Entity character)
    {
        int leftover = character.GetStat(Stats.ActionPoints).statValue;
        character.statsContainer.CarriedOverActionPoints.statValue = leftover;
        character.statsContainer.ActionPoints.statValue = 0;
    }

    public void EndTurn()
    {
        StartNextTurn();
    }

    [Button("Reset Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void ResetBattle()
    {
        GameIsPlaying = false;
        currentTurnIndex = 0;
        turnOrder.Clear();

        foreach (var unit in playerUnits)
        {
            unit?.Reset();
        }

        foreach (var unit in enemyUnits)
        {
            unit?.Reset();
        }

        StartBattle();
    }

    public void AdvanceTurn()
    {
        if (currentUnit != null && currentUnit.isAlive)
        {
            currentUnit.endTurn?.Raise();
        }

        EndTurn();
    }

    public Entity GetCurrentUnit() => currentUnit;

    public List<Entity> GetFullTurnOrder() => new List<Entity>(turnOrder);

    public int GetRemainingTurns() => turnOrder.Count - currentTurnIndex;

    public List<Entity> GetRemainingEntitiesThisRound()
    {
        return turnOrder
            .Skip(currentTurnIndex)
            .Where(e => e != null && e.isAlive)
            .ToList();
    }

    public bool IsThisPlayersTurn(string characterId)
    {
        if (currentUnit is CharacterManager character)
        {
            return character.characterId == characterId;
        }
        return false;
    }


    public void DelayEntity(Entity entity, int positions)
    {
        if (!turnOrder.Contains(entity)) return;
        int currentIndex = turnOrder.IndexOf(entity);
        int newIndex = Mathf.Min(currentIndex + positions, turnOrder.Count - 1);
        turnOrder.RemoveAt(currentIndex);
        turnOrder.Insert(newIndex, entity);
    }

    public void HasteEntity(Entity entity, int positions)
    {
        if (!turnOrder.Contains(entity)) return;
        int currentIndex = turnOrder.IndexOf(entity);
        int newIndex = Mathf.Max(currentIndex - positions, currentTurnIndex);
        turnOrder.RemoveAt(currentIndex);
        turnOrder.Insert(newIndex, entity);
    }

    public void RemoveFromTurnOrder(Entity entity)
    {
        if (turnOrder.Contains(entity))
        {
            turnOrder.Remove(entity);
        }
    }

    public void ShowBattleEndedDialog(bool PlayerWon = false)
    {
        GameIsPlaying = false;

        if (PlayerWon)
        {
            Debug.Log("All enemies defeated! Player wins!");
            BattleEndedDialogManager.Instance.Show(true);
        }
        else
        {
            Debug.Log("All player units defeated! Player loses!");
            BattleEndedDialogManager.Instance.Show(false);
        }

        GameEnded?.Raise();
    }
} 
