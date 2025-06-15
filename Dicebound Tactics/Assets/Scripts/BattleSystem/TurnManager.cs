using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TacticsToolkit;

public class TurnManager : MonoBehaviour
{
  public List<CharacterManager> playerUnits = new List<CharacterManager>();
  public List<EnemyManager> enemyUnits = new List<EnemyManager>();

  [Header("Events")]
  public GameEventGameObject startNewTurn;

  private int currentPlayerIndex = 0;
  private int currentEnemyIndex = 0;
  public bool playerTurn = true;

  public void StartBattle(List<CharacterManager> players, List<EnemyManager> enemies)
  {
    playerUnits = players;
    enemyUnits = enemies;
    currentPlayerIndex = 0;
    currentEnemyIndex = 0;
    playerTurn = true;

    StartNextTurn();
  }

  private void StartNextTurn()
  {
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
          return;
        }
      }

      // All enemies finished or dead, switch back to player turn
      currentPlayerIndex = 0;
      playerTurn = true;
      StartNextTurn();
    }
  }

  public void EndTurn()
  {
    StartNextTurn();
  }
    
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
}
