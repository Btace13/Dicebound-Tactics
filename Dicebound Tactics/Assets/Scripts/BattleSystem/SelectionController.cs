using UnityEngine;
using System.Collections.Generic;
using TacticsToolkit;

public class SelectionController : MonoBehaviour
{
  public GameObject selectionIndicatorPrefab;
  public GameObject highlightIndicatorPrefab;

  public bool canUseMouseSelection = false;
  public GameEventEntity onEntitySelected;
  public List<Entity> SelectedEntities = new();

  [Header("Selection Settings")]
  public int numberOfSelectableTargets = 1;

  private Dictionary<Entity, GameObject> indicators = new Dictionary<Entity, GameObject>();

  public bool cyclingEnemies = false;
  private int currentIndex = -1;

  private TurnManager turnManager;
  private Camera mainCamera;

  private Entity lastHoveredEntity;
  private Entity highlightedEntity;
  private GameObject highlightIndicator;


  private void Start()
  {
    turnManager = FindFirstObjectByType<TurnManager>();
    mainCamera = Camera.main;
  }

  private void Update()
  {
    if (turnManager == null || !turnManager.GameIsPlaying)
    {
      ClearAllSelections();
      return;
    }

    UpdateIndicators();
    HandleMouseHover();

    if (Input.GetKeyDown(KeyCode.Tab))
    {
      CycleHighlight();
    }

    if (Input.GetKeyDown(KeyCode.Space))
    {
      if (numberOfSelectableTargets > 1)
      {
        Entity toSelect = lastHoveredEntity != null ? lastHoveredEntity : highlightedEntity;

        if (toSelect != null && toSelect.isAlive)
        {
          // bool multiSelect = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift); 
          ToggleEntitySelection(toSelect);
        }
      }
    }

    if (Input.GetKeyDown(KeyCode.LeftShift))
    {
      cyclingEnemies = !cyclingEnemies;
      currentIndex = -1;
    }

    if (Input.GetKeyDown(KeyCode.Escape))
    {
      ClearAllSelections();
    }
  }

  private void UpdateIndicators()
  {
    foreach (var pair in indicators)
    {
      if (pair.Key != null && pair.Value != null)
      {
        pair.Value.transform.position = pair.Key.transform.position + Vector3.up * 1.5f;
      }
    }
  }

  private void HandleMouseHover()
  {
    if (!canUseMouseSelection || mainCamera == null) return;

    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit hit))
    {
      var entity = hit.collider.GetComponent<Entity>();

      if (entity != null && entity.isAlive)
      {
        lastHoveredEntity = entity;

        if (numberOfSelectableTargets == 1 && SelectedEntities.Count == 0 && !SelectedEntities.Contains(entity))
        {
          ToggleEntitySelection(entity, false);
        }
        else
        {
          SetHighlightedEntity(entity);
        }

        if (Input.GetMouseButtonDown(0))
        {
          bool multiSelect = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftShift);
          ToggleEntitySelection(entity, multiSelect);
        }

        return;
      }
    }

    lastHoveredEntity = null;
  }

  public void ToggleEntitySelection(Entity entity, bool additive = true)
  {
    if (entity == null) return;

    var list = cyclingEnemies
        ? turnManager.enemyUnits.ConvertAll(e => (Entity)e)
        : turnManager.playerUnits.ConvertAll(p => (Entity)p);

    if (!list.Contains(entity)) return;

    if (!additive)
    {
      ClearAllSelections();
    }

    if (!SelectedEntities.Contains(entity))
    {
      if (SelectedEntities.Count >= numberOfSelectableTargets)
      {
        RemoveEntitySelection(SelectedEntities[0]);
      }

      SelectedEntities.Add(entity);
      if (selectionIndicatorPrefab != null)
      {
        var indicator = Instantiate(selectionIndicatorPrefab, entity.transform.position + Vector3.up * 1.5f, Quaternion.identity);
        indicator.transform.SetParent(entity.transform);
        indicators[entity] = indicator;
      }
      onEntitySelected?.Raise(entity);
    }
    else if (additive && SelectedEntities.Contains(entity))
    {
      RemoveEntitySelection(entity);
    }
  }

  public void RemoveEntitySelection(Entity entity)
  {
    if (SelectedEntities.Contains(entity))
    {
      SelectedEntities.Remove(entity);
      if (indicators.ContainsKey(entity))
      {
        Destroy(indicators[entity]);
        indicators.Remove(entity);
      }
    }
  }

  public void ClearAllSelections()
  {
    foreach (var indicator in indicators.Values)
      Destroy(indicator);

    indicators.Clear();
    SelectedEntities.Clear();

    ClearHighlight();
  }

  public void SetSelectableTargetCount(int count)
  {
    numberOfSelectableTargets = Mathf.Max(1, count);

    while (SelectedEntities.Count > numberOfSelectableTargets)
    {
      RemoveEntitySelection(SelectedEntities[0]);
    }
  }

  public void CycleHighlight()
  {
    if (turnManager == null) return;

    List<Entity> list = cyclingEnemies
        ? turnManager.enemyUnits.ConvertAll(e => (Entity)e)
        : turnManager.playerUnits.ConvertAll(p => (Entity)p);

    if (list == null || list.Count == 0) return;

    int max = list.Count;
    int attempts = 0;

    do
    {
      currentIndex = (currentIndex + 1) % max;
      attempts++;
    }
    while (!list[currentIndex].isAlive && attempts < max);

    Entity entityToHighlight = list[currentIndex];

    if (numberOfSelectableTargets == 1)
    {
      ToggleEntitySelection(entityToHighlight, false);
    }
    else
    {
      SetHighlightedEntity(entityToHighlight);
    }
  }

  public void ChangeSelectionType(bool cycleEnemies)
  {
    cyclingEnemies = cycleEnemies;
    currentIndex = 0;

    List<Entity> list = cyclingEnemies
        ? turnManager.enemyUnits.ConvertAll(e => (Entity)e)
        : turnManager.playerUnits.ConvertAll(p => (Entity)p);

    if (list.Count > 0)
    {
      ClearAllSelections();

      if (numberOfSelectableTargets == 1)
      {
        ToggleEntitySelection(list[0], false);
      }
      else
      {
        highlightedEntity = list[0];
      }
    }
  }

  private void SetHighlightedEntity(Entity entity)
  {
    if (highlightedEntity == entity) return;

    ClearHighlight();

    if (numberOfSelectableTargets <= 1 || entity == null || !entity.isAlive)
      return;

    highlightedEntity = entity;

    if (highlightIndicatorPrefab != null)
    {
      highlightIndicator = Instantiate(
          highlightIndicatorPrefab,
          entity.transform.position + Vector3.up * 1.5f,
          Quaternion.identity
      );
      highlightIndicator.transform.SetParent(entity.transform);
    }
  }

  private void ClearHighlight()
  {
    if (highlightIndicator != null)
    {
      Destroy(highlightIndicator);
      highlightIndicator = null;
    }

    highlightedEntity = null;
  }
}
