using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TacticsToolkit;
using Sirenix.OdinInspector;
using System.Linq;
using System.Threading.Tasks;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<CharacterManager> playerUnits = new();
    public List<EnemyManager> enemyUnits = new();
    public bool BattlePlaying;
    [SerializeField] DiceRollManager diceRollManager;

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
        EventManager.OnEntityDied += HandleEntityDied;
    }

    void OnDisable()
    {
        EventManager.OnCharacterTurnEnded -= HandleCharacterTurnEnded;
        EventManager.OnEnemyTurnEnded -= HandleEnemyTurnEnded;
        EventManager.OnEntityDied -= HandleEntityDied;
    }

    private void Update()
    {
        if (!BattlePlaying)
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
        StartCoroutine(StartNextTurn());
    }

    private void HandleEnemyTurnEnded(EnemyManager enemy = null)
    {
        StartCoroutine(StartNextTurn());
    }

    [Button("Start Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void StartBattle()
    {
        BattlePlaying = true;
        currentTurnIndex = 0;

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

        BuildTurnOrder(true);
    }

    private void BuildTurnOrder(bool isFirstRound = false)
    {
        if (!BattlePlaying)
            return;

        turnOrder.Clear();
        var allUnits = new List<Entity>();
        allUnits.AddRange(playerUnits);
        allUnits.AddRange(enemyUnits);

        turnOrder = allUnits
            .Where(u => u != null && u.isAlive)
            .OrderByDescending(u => u.GetStat(Stats.Speed).statValue)
            .ThenByDescending(u => Random.value) // Simple tiebreaker
            .ToList();

        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
        }

        StartCoroutine(StartNextTurn(isFirstRound));
    }

    public IEnumerator StartNextTurn(bool isFirstRound = false)
    {
        if (!BattlePlaying || turnOrder.Count == 0)
            yield break;

        if (currentTurnIndex >= turnOrder.Count)
        {
            // End of round, rebuild turn order and start again
            BuildTurnOrder();
            yield break;
        }

        var unit = currentUnit ? turnOrder[currentTurnIndex] : turnOrder.FirstOrDefault();
        currentTurnIndex++;

        if (unit != null && unit.isAlive)
        {
            currentUnit = unit;
            unit.StartTurn();
            EventManager.TriggerNewActiveEntity(unit);

            // Trigger battle started FIRST on first round to ensure UI is in correct state
            if (isFirstRound)
            {
                EventManager.TriggerBattleStarted();
                // Wait a frame to ensure UI state transition completes
                yield return null;
            }

            // Switch camera to CombatMenuCamera1 before dice rolling
            Debug.Log($"[TurnManager] Switching to CombatMenuCamera1 before dice roll for {unit.name}");
            CameraManager.Instance?.TrySetActiveCamera("CombatMenuCamera1");

            diceRollManager.RollDiceForUnit(unit, () =>
            {
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
            });
        }
    }

    public void EndCharacterTurn(Entity character)
    {
        int leftover = character.GetStat(Stats.ActionPoints).statValue;
        character.statsContainer.CarriedOverActionPoints.statValue = leftover;
        character.statsContainer.ActionPoints.statValue = 0;
        character.InvokeCharacterStatChanged();
    }

    [Button("Reset Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
    public void ResetBattle()
    {
        BattlePlaying = false;
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
        BattlePlaying = false;

        if (PlayerWon)
        {
            BattleEndedDialogManager.Instance.Show(true);
        }
        else
        {
            BattleEndedDialogManager.Instance.Show(false);
        }

        EventManager.TriggerBattleEnded();
    }

    public void SetEnemies(List<EnemyManager> enemies)
    {
        enemyUnits = enemies;
        foreach (var enemy in enemyUnits)
        {
            enemy.Reset();
        }
    }
    
    private void HandleEntityDied(Entity entity)
    {
        if (entity is CharacterManager character)
        {
            playerUnits.Remove(character);
        }
        else if (entity is EnemyManager enemy)
        {
            enemyUnits.Remove(enemy);
        }

        RemoveFromTurnOrder(entity);
    }
}
