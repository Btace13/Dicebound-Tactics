using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using DiceboundTactics.UI;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum ActionMap
    {
        NONE = 0,
        PLAYER = 1,
        UI = 2
    }

    [SerializeField, ReadOnly] public ActionMap CurrentActionMap = ActionMap.NONE;

    public InputSystem_Actions InputActions;
    [SerializeField] InputIconManager inputIconManager;

    private List<IIconUpdater> _iconUpdaters = new List<IIconUpdater>();
    private InputDevice currentDevice;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InputActions = new InputSystem_Actions();

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

    private void OnEnable()
    {
        InputSystem.onActionChange += OnInputAction;
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnInputAction;
    }

    private void OnInputAction(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            var inputAction = (InputAction)obj;
            if (inputAction.activeControl != null)
            {
                OnInputReceived(inputAction.activeControl);
            }
        }
    }

    private void OnInputReceived(InputControl control)
    {
        currentDevice = control.device;

        if (currentDevice is Keyboard || currentDevice is Mouse)
        {
            Debug.Log("Current Device: Keyboard and Mouse");
            // Update icons for keyboard and mouse
        }
        else if (currentDevice is Gamepad)
        {
            var gamepad = currentDevice as Gamepad;
            if (gamepad != null)
            {
                if (gamepad.name.Contains("Xbox"))
                {
                    Debug.Log("Current Device: Xbox Controller");
                    // Update icons for Xbox controller
                }
                else if (gamepad.name.Contains("PlayStation"))
                {
                    Debug.Log("Current Device: PlayStation Controller");
                    // Update icons for PlayStation controller
                }
                else
                {
                    Debug.Log("Current Device: Generic Gamepad");
                    // Update icons for generic gamepad
                }
            }
        }
        else
        {
            Debug.Log("Current Device: Unknown");
            // Handle other input devices
        }
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

    public void UpdateAllIcons()
    {
        // Use currentDevice to determine which icons to update
        if (currentDevice != null)
        {
            Debug.Log($"Updating icons for: {currentDevice.displayName}");

            foreach (var updater in _iconUpdaters)
            {
                updater.UpdateIcon();
            }
        }
        else
        {
            Debug.LogWarning("No current device detected. Cannot update icons.");
        }
    }

    public void RegisterIconUpdater(IIconUpdater updater)
    {
        if (!_iconUpdaters.Contains(updater))
        {
            _iconUpdaters.Add(updater);
        }
    }

    public void UnregisterIconUpdater(IIconUpdater updater)
    {
        if (_iconUpdaters.Contains(updater))
        {
            _iconUpdaters.Remove(updater);
        }
    }

    public Sprite TryGetIconForAction(InputActionReference action)
    {
        if (inputIconManager == null)
        {
            Debug.LogWarning("InputIconManager is not assigned. Please assign it in the inspector.");
            return null;
        }

        return inputIconManager.GetIconForAction(action.name);
    }
}
