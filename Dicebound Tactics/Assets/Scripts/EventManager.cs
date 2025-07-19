using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TacticsToolkit;

public class EventManager : MonoBehaviour
{
  // Combat Events
  public static event Action<CombatEncounter> OnCombatEncounterStarted;
  public static event Action<CombatEncounter> OnCombatEncounterEnded;
  public static event Action OnBattleStarted;
  public static event Action OnBattleEnded;
  public static event Action OnGameOver;
  public static event Action<CharacterManager> OnCharacterTurnStarted;
  public static event Action<EnemyManager> OnEnemyTurnStarted;
  public static event Action<Entity> OnNewActiveEntity;
  public static event Action<Entity> OnTargetChanged;
  public static event Action<GameState> OnGameStateChanged;

  // UI Events
  public static event Action OnMenuButtonPressed;
  public static event Action OnSuccessButtonPressed;

  // Flag Events


  // Methods
  public static void TriggerCombatEncounterStarted(CombatEncounter encounter)
  {
    OnCombatEncounterStarted?.Invoke(encounter);
  }
  public static void TriggerCombatEncounterEnded(CombatEncounter encounter)
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
  public static void TriggerGameStateChanged(GameState state) {
    OnGameStateChanged?.Invoke(state);
  }
  public static void TriggerMenuButtonPressed() {
    OnMenuButtonPressed?.Invoke();
  }
  public static void TriggerSuccessButtonPressed() {
    OnSuccessButtonPressed?.Invoke();
  }
}

