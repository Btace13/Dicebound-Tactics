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
    if (encounter.GetEnemyEncounterSide() == null || encounter.GetEnemyEncounterSide().combatSlots.Count == 0)
    {
      Debug.Log("No enemy combat slots found in the encounter.");
      return;
    }

    List<Entity> enemies = encounter.GetEnemyEncounterSide().combatSlots.FindAll(slot => slot.isOccupied && slot.entity is EnemyManager)
      .ConvertAll(slot => slot.entity);

    foreach (var enemy in enemies)
    {
      EnemyHealthBarUI healthBar = Instantiate(healthBarPrefab, healthBarsContainer.transform);
      healthBar.SetEnemyInfo(enemy);
      healthBars.Add(healthBar);
    }
  }


  private void DestroyEnemyHealthBars(CombatEncounter encounter)
  {
    healthBars.Clear();
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
