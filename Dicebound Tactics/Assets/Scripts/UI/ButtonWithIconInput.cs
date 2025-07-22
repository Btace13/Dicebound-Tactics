using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ButtonWithIconInput : MonoBehaviour
{
    public InputActionReference inputAction;
    public Image buttonImage;

    private Button button;

    private void OnEnable()
    {
        button = GetComponent<Button>();
        if (inputAction != null)
        {
            inputAction.action.performed += OnInputActionPerformed;
        }
        UpdateIcon();
    }

    private void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.action.performed -= OnInputActionPerformed;
        }
    }

    private void OnInputActionPerformed(InputAction.CallbackContext context)
    {
        if (button != null && button.interactable)
        {
            button.onClick?.Invoke();
        }
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
            Debug.LogWarning($"No icon found for input action {inputAction?.name}. Button image will not be updated.");
            buttonImage.enabled = false;
        }
    }
}
