using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class InputManager : MonoBehaviour
{
    public enum ActionMap
    {
        NONE = 0,
        PLAYER = 1,
        UI = 2
    }

    [SerializeField, ReadOnly] public ActionMap CurrentActionMap = ActionMap.NONE;

    //private InputSystem_Actions inputActions;

    private void Awake()
    {
        //inputActions = new InputSystem_Actions();

        // inputActions.Player.Move.performed += Move;
        // inputActions.Player.Look.performed += Look;
        // inputActions.Player.Jump.performed += Jump;
        // inputActions.Player.CycleUnit.performed += Cycle;
        // inputActions.Player.OpenInventory.performed += OpenInventory;
        // inputActions.Player.Pause.performed += PauseGame;
        // inputActions.Player.NumberKey.performed += NumberKeyPressed;
        // inputActions.Player.StopTime.performed += StopTime;

        // inputActions.UI.RotateObjectLeft.performed += RotateItemLeft;
        // inputActions.UI.RotateObjectRight.performed += RotateItemRight;
        // inputActions.UI.Confirm.performed += Confirm;
        // inputActions.UI.CloseInventory.performed += CloseInventory;
        // inputActions.UI.Click.performed += UIClick;
        // inputActions.UI.Click.canceled += UIClickReleased;
        // inputActions.UI.CycleUI.performed += CycleUI;
        // inputActions.UI.ScrollWheel.performed += ScrollUI;
        // inputActions.UI.Close.performed += PauseGame;

        SetActionMap(ActionMap.PLAYER);
    }

    public void SetActionMap(ActionMap actionMap)
    {
        switch (actionMap)
        {
            case ActionMap.NONE:
                //inputActions.UI.Disable();
                //inputActions.Player.Disable();
                break;
            case ActionMap.UI:
                //inputActions.UI.Enable();
                //inputActions.Player.Disable();
                break;
            case ActionMap.PLAYER:
                //inputActions.UI.Disable();
                //inputActions.Player.Enable();
                break;
        }

        CurrentActionMap = actionMap;
    }
}
