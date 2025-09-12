using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TacticsToolkit;
using System.Linq;

public class CombatUIHandler : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector3 _canvasOffset = new Vector3(0.25f, 0.5f, 0); // Offset for the canvas position
    [SerializeField] private Ease _defaultEase = Ease.InOutQuad;

    [SerializeField] private CombatPanel currentPanel;
    public CombatPanel CurrentPanel
    {
        get => currentPanel;
        set => currentPanel = value; // Simplified - null assignment is valid and useful for clearing
    }

    [Header("Camera Settings")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private UDictionary<CombatPanel, string> PanelCameras = new UDictionary<CombatPanel, string>();

    [Header("Panel References")]
    public ActionPanel ActionPanel;
    public AbilityPanel AbilityPanel;
    public ItemPanel ItemPanel;

    [Header("Screen Space UI References")]
    [SerializeField] private CanvasGroup screenSpaceCanvasGroup;
    [SerializeField] private CombatNotification notificationUI;
    [SerializeField] private CanvasGroup panelInputsCanvasGroup;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private CanvasGroup targetSelectionCanvasGroup;

    [Header("Other References")]
    public DamageNumberUIHandler damageNumberUIHandler;

    private void Awake()
    {
        // Initialize all panels as inactive
        SetPanelActive(ActionPanel?.gameObject, false);
        SetPanelActive(AbilityPanel?.gameObject, false);
        SetPanelActive(ItemPanel?.gameObject, false);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private void OnDisable()
    {
        // Intentionally left blank: this class no longer subscribes to global events.
    }

    public void MoveCanvasToCharacter(CharacterManager character)
    {
        if (character == null)
        {
            return;
        }

        MoveCanvasToTarget(character.transform);
    }

    public void MoveCanvasToTarget(Transform target)
    {
        if (currentPanel == null || target == null)
        {
            return;
        }

        // print($"Moving canvas to target: {target.name}");

        // Check if the target is a CharacterManager
        // If it is, populate the AbilityPanel with the character's abilities
        if (target.TryGetComponent(out CharacterManager character))
        {
            if (AbilityPanel != null)
            {
                AbilityPanel.PopulateAbilityPanel(character);
            }

            if (ItemPanel != null)
            {
                ItemPanel.PopulateItemPanel(character);
            }
        }

        transform.position = target.position + _canvasOffset;
    }

    public void OpenPanel(CombatPanel panel)
    {
        if (panel == null) return;

        if (currentPanel != null)
        {
            SwitchPanels(currentPanel, panel);
        }
        else
        {
            ShowFirstPanel(panel);
        }

        SetCameraForPanel(panel);
    }

    private void SwitchPanels(CombatPanel fromPanel, CombatPanel toPanel)
    {
        fromPanel.FadeOutCanvas(() =>
        {
            CompletePanelSwitch(fromPanel, toPanel);
        }, _fadeDuration, _defaultEase);
    }

    private void ShowFirstPanel(CombatPanel panel)
    {
        ShowScreenSpacePanelInputs(false);
        currentPanel = panel;
        currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
    }

    private void CompletePanelSwitch(CombatPanel fromPanel, CombatPanel toPanel)
    {
        // Set up panel relationships
        if (fromPanel != toPanel)
        {
            toPanel.PreviousPanel = fromPanel;
        }

        // Update active states
        fromPanel.gameObject.SetActive(false);
        toPanel.gameObject.SetActive(true);

        // Update current panel reference
        currentPanel = toPanel;

        // Update UI state
        ShowScreenSpacePanelInputs(currentPanel.PreviousPanel != null);
        currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
    }

    private void SetCameraForPanel(CombatPanel panel)
    {
        if (cameraManager != null && PanelCameras.TryGetValue(panel, out string cameraName))
        {
            cameraManager.TrySetActiveCamera(cameraName);
        }
    }

    public void OpenActionPanel()
    {
        OpenPanel(ActionPanel);
    }

    public void FadeCurrentPanel(bool fadeIn)
    {
        if (currentPanel == null) return;

        if (fadeIn)
            currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
        else
            currentPanel.FadeOutCanvas(null, _fadeDuration, _defaultEase);
    }

    public void FadeOutCurrentPanel() => FadeCurrentPanel(false);
    public void FadeInCurrentPanel() => FadeCurrentPanel(true);

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
        {
            OpenPanel(ActionPanel);
            return;
        }

        var panelToReturn = currentPanel.PreviousPanel;
        bool shouldHideInputs = ShouldHideScreenSpaceInputs(panelToReturn);

        if (shouldHideInputs)
        {
            ShowScreenSpacePanelInputs(false);
        }

        currentPanel.FadeOutCanvas(() =>
        {
            if (panelToReturn != null)
            {
                ReturnToPreviousPanel(panelToReturn);
            }
            else
            {
                currentPanel = null;
            }
        }, _fadeDuration, _defaultEase);
    }

    private bool ShouldHideScreenSpaceInputs(CombatPanel panelToReturn)
    {
        return panelToReturn == null || panelToReturn.PreviousPanel == null;
    }

    private void ReturnToPreviousPanel(CombatPanel panelToReturn)
    {
        // Deactivate current panel
        currentPanel.gameObject.SetActive(false);

        // Set previous panel as current
        currentPanel = panelToReturn;
        ShowScreenSpacePanelInputs(currentPanel.PreviousPanel != null);

        // Activate and fade in previous panel
        currentPanel.gameObject.SetActive(true);
        currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);

        // Update camera
        SetCameraForPanel(currentPanel);
    }

    public void CloseAllPanels()
    {
        if (currentPanel == null) return;

        if (cancelButton != null)
            cancelButton.interactable = false;

        currentPanel.FadeOutCanvas(() =>
        {
            currentPanel = null;
            ShowScreenSpacePanelInputs(false);
        }, _fadeDuration, _defaultEase);
    }

    private void FadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, fadeIn ? 1 : 0, _fadeDuration)
            .OnStart(() =>
            {
                canvasGroup.interactable = fadeIn;
                canvasGroup.blocksRaycasts = fadeIn;
            })
            .OnComplete(() => onComplete?.Invoke());
    }

    public void ShowScreenSpacePanelInputs(bool enable)
    {
        FadeCanvasGroup(panelInputsCanvasGroup, enable);
    }

    public void ShowConfirmButton(bool enable)
    {
        if (confirmButton == null) return;

        if (enable)
        {
            // Ensure the panel inputs are visible before showing the confirm button
            ShowScreenSpacePanelInputs(true);
        }

        confirmButton.gameObject.SetActive(enable);
        confirmButton.interactable = enable;
    }

    public void ShowBigNotification(string message, float duration = 2f)
    {
        notificationUI?.ShowBigNotification(message, duration);
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        notificationUI?.ShowNotification(message, duration);
    }

    public void ShowCombatUI()
    {
        FadeCanvasGroup(screenSpaceCanvasGroup, true);
    }

    public void HideCombatUI()
    {
        if (screenSpaceCanvasGroup == null) return;

        CloseAllPanels();
        FadeCanvasGroup(screenSpaceCanvasGroup, false);
    }

    private void HandleNewCharacterTurn(CharacterManager character)
    {
        //Debug.Log($"New character turn started for {character.name}");
        OpenActionPanel();
        // ShowConfirmButton(true);
        MoveCanvasToCharacter(character);
    }

    public void HandleNewEnemyTurn(EnemyManager enemy)
    {
        CloseAllPanels();
    }

    public void ShowTargetSelectionUI(bool enable)
    {
        FadeCanvasGroup(targetSelectionCanvasGroup, enable);
    }

    public void ToggleBackButtonInteractable(bool enable)
    {
        if (cancelButton != null)
            cancelButton.interactable = enable;
    }
}
