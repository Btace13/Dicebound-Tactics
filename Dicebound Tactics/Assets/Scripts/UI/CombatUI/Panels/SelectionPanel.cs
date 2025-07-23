using System.Collections.Generic;
using UnityEngine;

public class SelectionPanel : MonoBehaviour
{
  [SerializeField] private List<GameObject> selectionButtons;

  void Awake()
  {
    // Event Listeners
    EventManager.OnSelectingATarget += HandleSelectingATarget;
  }

  void OnDisable()
  {
    EventManager.OnSelectingATarget -= HandleSelectingATarget;
  }

  private void HandleSelectingATarget(bool isSelecting)
  {
    if (isSelecting)
    {
      ShowButtons();
    }
    else
    {
      HideButtons();
    }
  }

  private void ShowButtons()
  {
    foreach (var button in selectionButtons)
    {
      button.SetActive(true);
    }
  }

  private void HideButtons()
  {
    foreach (var button in selectionButtons)
    {
      button.SetActive(false);
    }
  }
}