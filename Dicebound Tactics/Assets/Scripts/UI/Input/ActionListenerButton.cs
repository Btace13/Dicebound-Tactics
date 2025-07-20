using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor.UI;
#endif

public class ActionListenerButton : Button, IIconUpdater
{
    public InputActionReference inputAction;

    [Header("UI Components")]
    public Image buttonImage;

    protected bool isBeingInvoked = false;

    private CanvasGroup canvasGroup;

    public new void OnEnable()
    {
        base.OnEnable();

        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ActionListenerButton.");
            return;
        }

        // Subscribe to the click event
        onClick.AddListener(OnButtonClick);

        (this as IIconUpdater).RegisterIconUpdater(this);

        if (inputAction == null)
        {
            Debug.LogWarning($"Input action for button {name} is not assigned. Please assign an action in the inspector.");
            return;
        }

        // Ensure the input action is enabled
        if (InputManager.Instance.InputActions != null)
        {
            inputAction.action.performed += UpdateButton;
            inputAction.action.canceled += UpdateButton;

            UpdateIcon();
        }
        else
        {
            Debug.LogWarning("InputActions is not initialized in InputManager. Please ensure it is set up correctly.");
        }

        canvasGroup = GetComponentInParent<CanvasGroup>(true);
    }

    public new void OnDisable()
    {
        base.OnDisable();

        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ActionListenerButton.");
            return;
        }

        // Unsubscribe from the click event
        onClick.RemoveListener(OnButtonClick);
        isBeingInvoked = false;

        (this as IIconUpdater).UnregisterIconUpdater(this);

        if (inputAction != null)
        {
            inputAction.action.performed -= UpdateButton;
            inputAction.action.canceled -= UpdateButton;
        }
        else
        {
            Debug.LogWarning($"Input action for button {name} is not assigned. Please assign an action in the inspector.");
        }

        canvasGroup = null;
    }

    public void UpdateIcon()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager is not initialized. Cannot update button icon.");
            return;
        }

        if (buttonImage == null)
        {
            Debug.LogWarning($"Button image for {name} is not assigned. Please assign an Image component in the inspector.");
            return;
        }

        Sprite icon = InputManager.Instance.TryGetIconForAction(inputAction);

        if (icon != null)
        {
            buttonImage.sprite = icon;
            buttonImage.enabled = true;
        }
        else
        {
            Debug.LogWarning($"No icon found for input action {inputAction.name}. Button image will not be updated.");
            buttonImage.enabled = false;
        }
    }

    private void UpdateButton(InputAction.CallbackContext context)
    {
        if (canvasGroup != null && !canvasGroup.interactable)
        {
            Debug.LogWarning($"Button {name} is not interactable due to CanvasGroup settings. Ignoring input action.");
            return;
        }

        if (context.action != inputAction.action)
        {
            Debug.LogWarning($"Input action {context.action.name} does not match the assigned action {inputAction.action.name} for button {name}. Ignoring input.");
            return;
        }

        if (gameObject.activeInHierarchy == false)
        {
            Debug.LogWarning($"Button {name} is not active in the hierarchy. Ignoring input action.");
            return;
        }

        if (context.performed)
        {
            DoStateTransition(SelectionState.Pressed, false);
            OnButtonClick();
            onClick?.Invoke();
        }
        else if (context.canceled)
        {
            DoStateTransition(SelectionState.Normal, false);
            isBeingInvoked = false;
        }
    }

    public void OnButtonClick()
    {
        if (isBeingInvoked)
        {
            Debug.LogWarning($"Button {name} is already being invoked. Ignoring duplicate click.");
            return;
        }

        isBeingInvoked = true;

        // Check if the button is currently being hovered over
        if (IsOverButton())
        {
            // Handle the button click action
            Debug.Log($"Button {name} clicked while hovered.");
        }
        else
        {
            Debug.Log($"Button {name} clicked but not hovered.");
        }
    }

    public bool IsOverButton()
    {
        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.LogWarning("EventSystem is not found in the scene.");
            return false;
        }

        if (eventSystem.IsPointerOverGameObject())
        {
            // Check if the pointer is over this button
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            var results = new System.Collections.Generic.List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                if (result.gameObject == gameObject)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ActionListenerButton))]
[CanEditMultipleObjects]
public class ActionListenerButtonEditor : ButtonEditor
{
    SerializedProperty inputActionProperty;

    protected override void OnEnable()
    {
        base.OnEnable();
        inputActionProperty = serializedObject.FindProperty("inputAction");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ActionListenerButton button = (ActionListenerButton)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Action Listener Button Settings", EditorStyles.boldLabel);

        button.buttonImage = (Image)EditorGUILayout.ObjectField("Button Image", button.buttonImage, typeof(Image), true);

        EditorGUILayout.PropertyField(inputActionProperty, new GUIContent("Input Action"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
