using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TacticsToolkit;

public class EventManager : MonoBehaviour
{
  // Combat Events
  public static event Action<CombatEncounter> OnCombatEncounterStarted;
  public static event Action<CombatEncounter, bool> OnCombatEncounterEnded;
  public static event Action OnBattleStarted;
  public static event Action OnBattleEnded;
  public static event Action<CharacterManager> OnCharacterTurnStarted;
  public static event Action<CharacterManager> OnCharacterTurnEnded;
  public static event Action<EnemyManager> OnEnemyTurnStarted;
  public static event Action<EnemyManager> OnEnemyTurnEnded;
  public static event Action<Entity> OnNewActiveEntity;
  public static event Action<Entity> OnTargetChanged;
  public static event Action<bool> OnSelectingATarget;
  public static event Action<DiceModifier, Entity> OnModifierApplied;
  public static event Action<Entity> OnEntityDied;
  public static event Action OnAttackBlocked;
  public static event Action OnPassTurn;

  // Ability Events
  public static event Action<Entity, Entity> OnAbilityStarted; // user, target
  public static event Action<Entity, Entity> OnAbilityEnded; // user, target

  // Defensive Timing Events
  public static event Action<Entity, string[], float, System.Action<bool>> OnDefensivePromptRequested;
  public static event Action OnDefensivePromptHidden;
  public static event Action<string> OnDefensiveButtonPressed;
  public static event Action OnDefensiveSequenceCompleted;
  public static event Action OnDefensiveSequenceFailed;

  // UI Events
  public static event Action OnMenuButtonPressed;
  public static event Action OnSuccessButtonPressed;
  public static event Action OnCharacterMenuOpened;
  public static event Action OnCharacterMenuClosed;
  public static event Action OnBackButtonPressed;
  public static event Action OnShowActionPanel;
  public static event Action OnShowAbilityPanel;
  public static event Action OnShowItemPanel;

  // Game Events
  public static event Action OnGameOver;
  public static event Action<GameState> OnGameStateChanged;

  // Flag Events


  // Methods
  public static void TriggerCombatEncounterStarted(CombatEncounter encounter)
  {
    OnCombatEncounterStarted?.Invoke(encounter);
  }
  public static void TriggerCombatEncounterEnded(CombatEncounter encounter, bool playerWon = true)
  {
    OnCombatEncounterEnded?.Invoke(encounter, playerWon);
  }
  public static void TriggerBattleStarted()
  {
    OnBattleStarted?.Invoke();
  }
  public static void TriggerBattleEnded()
  {
    OnBattleEnded?.Invoke();
  }
  public static void TriggerGameOver()
  {
    OnGameOver?.Invoke();
  }
  public static void TriggerCharacterTurnStarted(CharacterManager character)
  {
    OnCharacterTurnStarted?.Invoke(character);
  }
  public static void TriggerCharacterTurnEnded(CharacterManager character)
  {
    OnCharacterTurnEnded?.Invoke(character);
  }
  public static void TriggerEnemyTurnStarted(EnemyManager enemy)
  {
    OnEnemyTurnStarted?.Invoke(enemy);
  }
  public static void TriggerEnemyTurnEnded(EnemyManager enemy)
  {
    OnEnemyTurnEnded?.Invoke(enemy);
  }
  public static void TriggerNewActiveEntity(Entity entity)
  {
    OnNewActiveEntity?.Invoke(entity);
  }
  public static void TriggerTargetChanged(Entity entity)
  {
    OnTargetChanged?.Invoke(entity);
  }
  public static void TriggerGameStateChanged(GameState state)
  {
    OnGameStateChanged?.Invoke(state);
  }
  public static void TriggerMenuButtonPressed()
  {
    OnMenuButtonPressed?.Invoke();
  }
  public static void TriggerSuccessButtonPressed()
  {
    OnSuccessButtonPressed?.Invoke();
  }
  public static void TriggerSelectingATarget(bool isSelecting)
  {
    OnSelectingATarget?.Invoke(isSelecting);
  }
  public static void TriggerModifierApplied(DiceModifier modifier, Entity user)
  {
    OnModifierApplied?.Invoke(modifier, user);
  }
  public static void TriggerEntityDied(Entity entity)
  {
    OnEntityDied?.Invoke(entity);
  }
  public static void TriggerCharacterMenuOpened()
  {
    OnCharacterMenuOpened?.Invoke();
  }
  public static void TriggerCharacterMenuClosed()
  {
    OnCharacterMenuClosed?.Invoke();
  }
  public static void TriggerAttackBlocked()
  {
    OnAttackBlocked?.Invoke();
  }
  public static void TriggerDefensivePromptRequested(Entity target, string[] buttonSequence, float timeLimit, System.Action<bool> onComplete)
  {
    OnDefensivePromptRequested?.Invoke(target, buttonSequence, timeLimit, onComplete);
  }
  public static void TriggerDefensivePromptHidden()
  {
    OnDefensivePromptHidden?.Invoke();
  }
  public static void TriggerDefensiveButtonPressed(string buttonName)
  {
    OnDefensiveButtonPressed?.Invoke(buttonName);
  }
  public static void TriggerDefensiveSequenceCompleted()
  {
    OnDefensiveSequenceCompleted?.Invoke();
  }
  public static void TriggerDefensiveSequenceFailed()
  {
    OnDefensiveSequenceFailed?.Invoke();
  }
  public static void TriggerBackButtonPressed()
  {
    OnBackButtonPressed?.Invoke();
  }
  public static void TriggerShowActionPanel()
  {
    OnShowActionPanel?.Invoke();
  }
  public static void TriggerShowAbilityPanel()
  {
    OnShowAbilityPanel?.Invoke();
  }
  public static void TriggerShowItemPanel()
  {
    OnShowItemPanel?.Invoke();
  }
  public static void TriggerPassTurn()
  {
    OnPassTurn?.Invoke();
  }
  public static void TriggerAbilityStarted(Entity user, Entity target)
  {
    OnAbilityStarted?.Invoke(user, target);
  }
  public static void TriggerAbilityEnded(Entity user, Entity target)
  {
    OnAbilityEnded?.Invoke(user, target);
  }
}

