using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ConfirmTargetButton : ButtonWithIconInput
{
  private bool isSelectingTarget;

  private void Awake() {
    // Event Listeners
    EventManager.OnSelectingATarget += HandleSelectingATarget;
  }

  void OnDisable()
  {
    EventManager.OnSelectingATarget -= HandleSelectingATarget;
  }

  private void HandleSelectingATarget(bool isSelecting)
  {
    isSelectingTarget = isSelecting;

    button.interactable = isSelectingTarget;
  }
}