using UnityEngine;

public class ConfirmTargetButton : ButtonWithIconInput
{
  private void Awake()
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
    Debug.Log($"ConfirmTargetButton: HandleSelectingATarget called with isSelecting: {isSelecting}");
    button.interactable = isSelecting;
  }
}
