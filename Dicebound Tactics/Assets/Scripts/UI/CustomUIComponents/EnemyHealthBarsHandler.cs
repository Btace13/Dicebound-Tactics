using System.Collections.Generic;
using UnityEngine;
using TacticsToolkit;


public class EnemyHealthBarsHandler : MonoBehaviour
{
  [SerializeField] private GameObject healthBarsContainer;
  [SerializeField] private EnemyHealthBarUI healthBarPrefab;

  private List<EnemyHealthBarUI> healthBars = new List<EnemyHealthBarUI>();

  private void Awake()
  {
    EventManager.OnCombatEncounterStarted += CreateEnemyHealthBars;
    EventManager.OnCombatEncounterEnded += DestroyEnemyHealthBars;
    EventManager.OnTargetChanged += OnlyShowSelectedHealthBar;
    EventManager.OnSelectingATarget += ShowAllHealthBars;
  }

  private void OnDestroy()
  {
    EventManager.OnCombatEncounterStarted -= CreateEnemyHealthBars;
    EventManager.OnCombatEncounterEnded -= DestroyEnemyHealthBars;
    EventManager.OnTargetChanged -= OnlyShowSelectedHealthBar;
    EventManager.OnSelectingATarget -= ShowAllHealthBars;
  }

  private void CreateEnemyHealthBars(CombatEncounter encounter)
  {
    // Create health bars for all current enemies
    foreach (var enemy in TurnManager.Instance.enemyUnits)
    {
      EnemyHealthBarUI healthBar = Instantiate(healthBarPrefab, healthBarsContainer.transform);
      healthBar.SetEnemyInfo(enemy);
    }
  }

  private void DestroyEnemyHealthBars(CombatEncounter encounter)
  {
    foreach (Transform child in healthBarsContainer.transform)
    {
      Destroy(child.gameObject);
    }
  }

  private void OnlyShowSelectedHealthBar(Entity selectedEnemy)
  {
    foreach (var healthBar in healthBars)
    {
      if (healthBar != null)
      {
        healthBar.gameObject.SetActive(healthBar.GetEnemy() == selectedEnemy);
      }
    }
  }

  private void ShowAllHealthBars(bool isSelecting)
  {
    if (isSelecting)
      return;

    foreach (var healthBar in healthBars)
    {
      if (healthBar != null)
      {
        healthBar.gameObject.SetActive(true);
      }
    }
  }
}
