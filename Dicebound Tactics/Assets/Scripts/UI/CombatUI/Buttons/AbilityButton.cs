using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.InputSystem; // Added for InputActionReference
#if UNITY_EDITOR
using UnityEditor.UI;
#endif

public class AbilityButton : CombatButton, IIconUpdater
{
    public AbilitySO ability;
    public Image buttonImage;
    [SerializeField] TextMeshProUGUI abilityCostText;
    [SerializeField] TextMeshProUGUI abilityDescriptionText;
    public InputActionReference inputAction;
    private Button button;

    public void OnEnable()
    {
        button = GetComponent<Button>();

        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ActionListenerButton.");
            return;
        }

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
    }

    public void OnDisable()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ActionListenerButton.");
            return;
        }

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
    }

    private void Start()
    {
        UpdateIcon();
    }

    public override void AnimateIn()
    {
        base.AnimateIn();
        // Additional animation logic specific to AbilityButton
    }

    public void SetupAbilityButton(AbilitySO ability, UnityAction onClickAction, bool canUse = false)
    {
        this.ability = ability;
        abilityCostText.SetText(ability.apCost.ToString());
        abilityDescriptionText.SetText(ability.description);

        SetupButton(ability.abilityName, onClickAction);

        button.interactable = canUse;
    }

    public override void SetupButton(string text, UnityAction onClickAction)
    {
        base.SetupButton(text, onClickAction);
        // Additional setup logic specific to AbilityButton
    }


    public void UpdateIcon()
    {
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
            button.onClick?.Invoke();
        }
    }
}
