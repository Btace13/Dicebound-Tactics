using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TacticsToolkit;

public class EventManager : MonoBehaviour
{
  // Combat Events
  public static event Action<GameEventCombatEncounter> OnCombatEncounterStarted;
  public static event Action<GameEventCombatEncounter> OnCombatEncounterEnded;
  public static event Action OnBattleStarted;
  public static event Action OnBattleEnded;
  public static event Action OnGameOver;
  public static event Action<CharacterManager> OnCharacterTurnStarted;
  public static event Action<EnemyManager> OnEnemyTurnStarted;
  public static event Action<Entity> OnNewActiveEntity;
  public static event Action<Entity> OnTargetChanged;
  public static event Action<GameEventGameState> OnGameStateChanged;

  // UI Events
  public static event Action OnMenuButtonPressed;
  public static event Action OnSuccessButtonPressed;

  // Flag Events


  // Methods
  public static void TriggerCombatEncounterStarted(GameEventCombatEncounter encounter)
  {
    OnCombatEncounterStarted?.Invoke(encounter);
  }
  public static void TriggerCombatEncounterEnded(GameEventCombatEncounter encounter)
  {
    OnCombatEncounterEnded?.Invoke(encounter);
  }
  public static void TriggerBattleStarted() {
    OnBattleStarted?.Invoke();
  }
  public static void TriggerBattleEnded() {
    OnBattleEnded?.Invoke();
  }
  public static void TriggerGameOver() {
    OnGameOver?.Invoke();
  }
  public static void TriggerCharacterTurnStarted(CharacterManager character) {
    OnCharacterTurnStarted?.Invoke(character);
  }
  public static void TriggerEnemyTurnStarted(EnemyManager enemy) {
    OnEnemyTurnStarted?.Invoke(enemy);
  }
  public static void TriggerNewActiveEntity(Entity entity) {
    OnNewActiveEntity?.Invoke(entity);
  }
  public static void TriggerTargetChanged(Entity entity) {
    OnTargetChanged?.Invoke(entity);
  }
  public static void TriggerGameStateChanged(GameEventGameState state) {
    OnGameStateChanged?.Invoke(state);
  }
  public static void TriggerMenuButtonPressed() {
    OnMenuButtonPressed?.Invoke();
  }
  public static void TriggerSuccessButtonPressed() {
    OnSuccessButtonPressed?.Invoke();
  }
}

