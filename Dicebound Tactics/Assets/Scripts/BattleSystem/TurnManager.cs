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

        // Event Listeners
        EventManager.OnCharacterTurnEnded += HandleCharacterTurnEnded;
        EventManager.OnEnemyTurnEnded += HandleEnemyTurnEnded;
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

    private void HandleCharacterTurnEnded(CharacterManager character = null)
    {
        StartNextTurn();
    }

    private void HandleEnemyTurnEnded(EnemyManager enemy = null)
    {
        StartNextTurn();
    }

    [Button("Start Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void StartBattle()
    {
        GameIsPlaying = true;
        EventManager.TriggerBattleStarted();

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

        turnOrder.ForEach(unit => unit.RollDice());
    }

    public void StartNextTurn()
    {
        if (!GameIsPlaying || turnOrder.Count == 0)
            return;

        if (currentTurnIndex >= turnOrder.Count)
        {
            // End of round, rebuild turn order and start again
            BuildTurnOrder();
            currentTurnIndex = 0;
        }

        var unit = currentUnit ? turnOrder[currentTurnIndex] : turnOrder.FirstOrDefault();
        currentTurnIndex++;

        if (unit != null && unit.isAlive)
        {
            currentUnit = unit;
            Debug.Log("Starting next turn for " + (currentUnit != null ? currentUnit.name : "null"));
            unit.StartTurn();
            EventManager.TriggerNewActiveEntity(unit);

            if (unit is CharacterManager character)
            {
                EventManager.TriggerCharacterTurnStarted(character);
                // Wait for EventManager.OnCharacterTurnEnded to advance turn
            }
            else if (unit is EnemyManager enemy)
            {
                EventManager.TriggerEnemyTurnStarted(enemy);
                // Wait for EventManager.OnEnemyTurnEnded to advance turn
            }
        }
    }

    public void EndCharacterTurn(Entity character)
    {
        int leftover = character.GetStat(Stats.ActionPoints).statValue;
        character.statsContainer.CarriedOverActionPoints.statValue = leftover;
        character.statsContainer.ActionPoints.statValue = 0;
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

        EventManager.TriggerGameOver();
    }
}
