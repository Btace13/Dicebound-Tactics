using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TacticsToolkit;

public class ItemButton : CombatButton, IIconUpdater
{
    public CombatItem combatItem;
    public Image buttonImage;
    [SerializeField] TextMeshProUGUI itemCostText;
    [SerializeField] TextMeshProUGUI itemDescriptionText;
    [SerializeField] TextMeshProUGUI itemQuantityText;
    public InputActionReference inputAction;
    private bool isSelectingTarget;

    public void OnEnable()
    {
        button = GetComponent<Button>();

        // Event Listeners
        EventManager.OnSelectingATarget += HandleSelectingATarget;

        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ItemButton.");
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
        EventManager.OnSelectingATarget -= HandleSelectingATarget;
        if (InputManager.Instance == null)
        {
            Debug.LogWarning("InputManager instance is not found. Please ensure it is initialized before using ItemButton.");
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
        // Additional animation logic specific to ItemButton
    }

    public void SetupItemButton(CombatItem item, int quantity, UnityAction onClickAction, bool canUse = false)
    {
        this.combatItem = item;
            
        if (itemDescriptionText != null)
            itemDescriptionText.SetText(item.Description);
            
        if (itemQuantityText != null)
            itemQuantityText.SetText($"x{quantity}");

        SetupButton(item.ItemName, onClickAction);

        Button.interactable = canUse && quantity > 0;
    }

    public override void SetupButton(string text, UnityAction onClickAction)
    {
        base.SetupButton(text, onClickAction);
        // Additional setup logic specific to ItemButton
    }

    public void UpdateIcon()
    {
        if (InputManager.Instance == null)
            return;

        Sprite icon = InputManager.Instance.TryGetIconForAction(inputAction);

        if (icon != null && buttonImage != null)
        {
            buttonImage.sprite = icon;
            buttonImage.enabled = true;
        }
        else
        {
            if (buttonImage != null)
                buttonImage.enabled = false;
            
            if (inputAction != null)
                Debug.LogWarning($"No icon found for input action {inputAction.name}. Button image will not be updated.");
        }
    }

    private void UpdateButton(InputAction.CallbackContext context)
    {
        if (isSelectingTarget)
        {
            Debug.LogWarning($"Button {name} is currently selecting a target. Ignoring input action.");
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
            Button.onClick?.Invoke();
        }
    }
    
    private void HandleSelectingATarget(bool enable)
    {
        isSelectingTarget = enable;
    }

    /// <summary>
    /// Update the button state based on current entity and inventory
    /// </summary>
    public void UpdateButtonState(Entity currentEntity)
    {
        if (combatItem == null || currentEntity == null)
        {
            Button.interactable = false;
            return;
        }

        // Check if entity has the item and can use it
        bool hasItem = currentEntity.inventory != null && currentEntity.inventory.HasItem(combatItem);
        bool canUseThisTurn = currentEntity.CanUseItemThisTurn();
        bool canTarget = combatItem.CanUseOn(currentEntity, currentEntity); // Basic check, actual target validation happens on use

        Button.interactable = hasItem && canUseThisTurn && canTarget;

        // Update quantity display
        if (itemQuantityText != null && hasItem)
        {
            int quantity = currentEntity.inventory.GetItemQuantity(combatItem);
            itemQuantityText.SetText($"x{quantity}");
        }
        else if (itemQuantityText != null)
        {
            itemQuantityText.SetText("x0");
        }
    }
}
