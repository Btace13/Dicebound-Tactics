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

        // InputActions.Player.Move.performed += Move;
        // InputActions.Player.Look.performed += Look;
        // InputActions.Player.Jump.performed += Jump;
        // InputActions.Player.CycleUnit.performed += Cycle;
        // InputActions.Player.OpenInventory.performed += OpenInventory;
        // InputActions.Player.Pause.performed += PauseGame;
        // InputActions.Player.NumberKey.performed += NumberKeyPressed;
        // InputActions.Player.StopTime.performed += StopTime;

        // InputActions.UI.RotateObjectLeft.performed += RotateItemLeft;
        // InputActions.UI.RotateObjectRight.performed += RotateItemRight;
        // InputActions.UI.Confirm.performed += Confirm;
        // InputActions.UI.CloseInventory.performed += CloseInventory;
        // InputActions.UI.Click.performed += UIClick;
        // InputActions.UI.Click.canceled += UIClickReleased;
        // InputActions.UI.CycleUI.performed += CycleUI;
        // InputActions.UI.ScrollWheel.performed += ScrollUI;
        // InputActions.UI.Close.performed += PauseGame;

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
            // Debug.Log("Current Device: Keyboard and Mouse");
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
                InputActions.UI.Disable();
                InputActions.Player.Disable();
                break;
            case ActionMap.UI:
                InputActions.UI.Enable();
                InputActions.Player.Disable();
                break;
            case ActionMap.PLAYER:
                InputActions.UI.Disable();
                InputActions.Player.Enable();
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
