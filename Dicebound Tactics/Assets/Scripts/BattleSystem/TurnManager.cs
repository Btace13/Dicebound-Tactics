using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TacticsToolkit;
using Sirenix.OdinInspector;

public class TurnManager : MonoBehaviour
{
  public List<CharacterManager> playerUnits = new List<CharacterManager>();
  public List<EnemyManager> enemyUnits = new List<EnemyManager>();
  public bool playerTurn = true;
  public bool GameIsPlaying;

  [Header("Events")]
  public GameEvent BattleStarted;
  public GameEventGameObject startNewTurn;
  public GameEventCharacterManager startNewCharacterTurn;
  public GameEventEnemyManager startNewEnemyTurn;

  public GameEvent GameEnded;

  private int currentPlayerIndex = 0;
  private int currentEnemyIndex = 0;
  private Entity currentUnit;

  private void Update()
  {
    if (!GameIsPlaying)
    {
      return;
    }

    bool allEnemiesDefeated = enemyUnits.TrueForAll(e => !e.isAlive);
    if (allEnemiesDefeated)
    {
      // Player wins, handle victory logic here
      Debug.Log("All enemies defeated! Player wins!");
      GameIsPlaying = false;
      GameEnded.Raise();
      return;
    }

    bool allPlayersDead = playerUnits.TrueForAll(p => !p.isAlive);
    if (allPlayersDead)
    {
      // Player loses, handle defeat logic here
      Debug.Log("All player units defeated! Player loses!");
      GameIsPlaying = false;
      GameEnded.Raise();
      return;
    }
  }

  [Button("Start Battle", ButtonSizes.Medium, ButtonStyle.CompactBox)]
  public void StartBattle()
  {
    currentPlayerIndex = 0;
    currentEnemyIndex = 0;
    playerTurn = true;
    GameIsPlaying = true;

    BattleStarted?.Raise();
    Debug.Log("Battle started!");

    BeginPlayerTurn();
    StartNextTurn();
  }

  private void StartNextTurn()
  {
    if (!GameIsPlaying)
    {
      Debug.Log("Game is not currently playing. Cannot start next turn.");
      return;
    }

    if (playerTurn)
    {
      while (currentPlayerIndex < playerUnits.Count)
      {
        var unit = playerUnits[currentPlayerIndex];
        currentPlayerIndex++;

        if (unit != null && unit.isAlive)
        {
          unit.StartTurn();
          startNewTurn.Raise(unit.gameObject);

          if (startNewCharacterTurn != null)
          {
            startNewCharacterTurn.Raise(unit);
          }

          currentUnit = unit;
          return;
        }
      }

      // All player units finished or dead, switch to enemy turn
      currentEnemyIndex = 0;
      playerTurn = false;
      StartNextTurn();
    }
    else
    {
      while (currentEnemyIndex < enemyUnits.Count)
      {
        var unit = enemyUnits[currentEnemyIndex];
        currentEnemyIndex++;

        if (unit != null && unit.isAlive)
        {
          unit.StartTurn();
          startNewTurn.Raise(unit.gameObject);

          if (startNewEnemyTurn != null)
          {
            startNewEnemyTurn.Raise(unit);
          }

          currentUnit = unit;

          if (unit != null)
          {
            unit.BeginAITurn();
          }

          return;
        }
      }

      // All enemies finished or dead, switch back to player turn
      currentPlayerIndex = 0;
      playerTurn = true;
      BeginPlayerTurn();
      StartNextTurn();
    }
  }

  private void BeginPlayerTurn()
  {
    foreach (var character in playerUnits)
    {
      if (!character.isAlive) continue;

      int diceRoll = character.RollDice();
      int carriedOver = character.GetStat(Stats.CarriedOverActionPoints).statValue;

      int totalAP = diceRoll + carriedOver;

      character.statsContainer.ActionPoints.statValue = totalAP;
      character.statsContainer.CarriedOverActionPoints.statValue = 0;

      Debug.Log($"{character.name} rolled a {diceRoll} and now has {totalAP} AP.");
    }

    foreach (var enemy in enemyUnits)
    {
      if (!enemy.isAlive) continue;

      int diceRoll = enemy.RollDice();
      int carriedOver = enemy.GetStat(Stats.CarriedOverActionPoints).statValue;

      int totalAP = diceRoll + carriedOver;

      enemy.statsContainer.ActionPoints.statValue = totalAP;
      enemy.statsContainer.CarriedOverActionPoints.statValue = 0;
    }
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
    currentPlayerIndex = 0;
    currentEnemyIndex = 0;
    playerTurn = true;

    foreach (var unit in playerUnits)
    {
      if (unit != null)
        unit.Reset();
    }

    foreach (var unit in enemyUnits)
    {
      if (unit != null)
        unit.Reset();
    }

    StartNextTurn();
  }

  public void AdvanceTurn()
  {
    if (playerTurn)
    {
      if (currentPlayerIndex < playerUnits.Count)
      {
        var unit = playerUnits[currentPlayerIndex];
        if (unit != null && unit.isAlive)
        {
          unit.endTurn.Raise();
        }
      }
    }
    else
    {
      if (currentEnemyIndex < enemyUnits.Count)
      {
        var unit = enemyUnits[currentEnemyIndex];
        if (unit != null && unit.isAlive)
        {
          unit.endTurn.Raise();
        }
      }
    }

    EndTurn();
  }

  public Entity GetCurrentUnit()
  {
    return currentUnit;
  }
}
