using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TacticsToolkit;

public class CombatUIHandler : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector3 _canvasOffset = new Vector3(0.25f, 0.5f, 0); // Offset for the canvas position

    [SerializeField] private CombatPanel currentPanel;
    public CombatPanel CurrentPanel
    {
        get => currentPanel;
        set
        {
            if (value != null)
            {
                currentPanel = value;
            }
        }
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

    [Header("Other References")]
    public DamageNumberUIHandler damageNumberUIHandler;

    private void Awake()
    {
        if (ActionPanel)
        {
            ActionPanel.gameObject.SetActive(false);
        }

        if (AbilityPanel)
        {
            AbilityPanel.gameObject.SetActive(false);
        }

        if (ItemPanel)
        {
            ItemPanel.gameObject.SetActive(false);
        }

        // Event Listeners
        EventManager.OnBattleStarted += OpenActionPanel;
        EventManager.OnCharacterTurnStarted += HandleNewCharacterTurn;
        EventManager.OnEnemyTurnStarted += HandleNewEnemyTurn;
    }

    private void OnDisable()
    {
        EventManager.OnBattleStarted -= OpenActionPanel;
        EventManager.OnCharacterTurnStarted -= HandleNewCharacterTurn;
        EventManager.OnEnemyTurnStarted -= HandleNewEnemyTurn;
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

        print($"Moving canvas to target: {target.name}");

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
        if (panel == null)
        {
            return;
        }

        if (currentPanel != null)
        {
            // Fade out the current panel before switching
            currentPanel.FadeOutCanvas(() =>
            {
                CombatPanel previousPanel = currentPanel;
                currentPanel = panel;
                if (previousPanel != null && previousPanel != panel)
                {
                    currentPanel.PreviousPanel = previousPanel;
                }

                // deactivate the previous panel and activate the new one
                previousPanel.gameObject.SetActive(false);
                panel.gameObject.SetActive(true);

                ShowScreenSpacePanelInputs(currentPanel.PreviousPanel != null);
                currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);
            }, _fadeDuration, Ease.InOutQuad);
        }
        else
        {
            ShowScreenSpacePanelInputs(false);
            currentPanel = panel;
            currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);

        }

        if (cameraManager)
        {
            if (PanelCameras.TryGetValue(panel, out string cameraName))
            {
                cameraManager.TrySetActiveCamera(cameraName);
            }
        }
    }

    public void OpenActionPanel()
    {
        if (ActionPanel == null)
        {
            return;
        }

        OpenPanel(ActionPanel);
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
        {
            OpenPanel(ActionPanel);
            return;
        }

        // if the panel you're returning to has no previous panel,
        // hide the screen space inputs
        if (currentPanel.PreviousPanel == null || currentPanel.PreviousPanel.PreviousPanel == null)
        {
            ShowScreenSpacePanelInputs(false);
        }

        currentPanel.FadeOutCanvas(() =>
        {
            if (currentPanel.PreviousPanel != null)
            {
                currentPanel.FadeOutCanvas(() =>
                {
                    // Deactivate the current panel and set the previous panel as current
                    currentPanel.gameObject.SetActive(false);

                    currentPanel = currentPanel.PreviousPanel;
                    ShowScreenSpacePanelInputs(currentPanel.PreviousPanel != null);

                    currentPanel.gameObject.SetActive(true);
                    currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);

                    if (cameraManager)
                    {
                        if (PanelCameras.TryGetValue(currentPanel, out string cameraName))
                        {
                            cameraManager.TrySetActiveCamera(cameraName);
                        }
                    }
                }, _fadeDuration, Ease.InOutQuad);
            }
            else
            {
                currentPanel = null;
            }

        }, _fadeDuration, Ease.InOutQuad);
    }

    public void CloseAllPanels()
    {
        if (currentPanel == null)
        {
            return;
        }

        currentPanel.FadeOutCanvas(() =>
        {
            currentPanel = null;
            ShowScreenSpacePanelInputs(false);
        }, _fadeDuration, Ease.InOutQuad);
    }

    public void ShowScreenSpacePanelInputs(bool enable)
    {
        if (panelInputsCanvasGroup == null)
        {
            return;
        }

        DOTween.To(() => panelInputsCanvasGroup.alpha, x => panelInputsCanvasGroup.alpha = x, enable ? 1 : 0, _fadeDuration)
            .OnStart(() =>
            {
                panelInputsCanvasGroup.interactable = enable;
                panelInputsCanvasGroup.blocksRaycasts = enable;
            });
    }

    public void ShowConfirmButton(bool enable)
    {
        if (confirmButton == null)
        {
            return;
        }

        if (enable)
        {
            // Ensure the panel inputs are visible before showing the confirm button
            ShowScreenSpacePanelInputs(true);
        }

        confirmButton.gameObject.SetActive(enable);
    }

    public void ShowBigNotification(string message, float duration = 2f)
    {
        if (notificationUI == null)
        {
            return;
        }

        notificationUI.ShowBigNotification(message, duration);
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationUI == null)
        {
            return;
        }

        notificationUI.ShowNotification(message, duration);
    }

    public void ShowCombatUI()
    {
        if (screenSpaceCanvasGroup == null)
        {
            return;
        }

        DOTween.To(() => screenSpaceCanvasGroup.alpha, x => screenSpaceCanvasGroup.alpha = x, 1, _fadeDuration)
            .OnStart(() =>
            {
                screenSpaceCanvasGroup.interactable = true;
                screenSpaceCanvasGroup.blocksRaycasts = true;
            });
    }

    public void HideCombatUI()
    {
        if (screenSpaceCanvasGroup == null)
        {
            return;
        }

        CloseAllPanels();

        DOTween.To(() => screenSpaceCanvasGroup.alpha, x => screenSpaceCanvasGroup.alpha = x, 0, _fadeDuration)
            .OnComplete(() =>
            {
                screenSpaceCanvasGroup.interactable = false;
                screenSpaceCanvasGroup.blocksRaycasts = false;
            });
    }

    private void HandleNewCharacterTurn(CharacterManager character)
    {
        ShowConfirmButton(true);
        MoveCanvasToCharacter(character);
        OpenActionPanel();
    }

    public void HandleNewEnemyTurn(EnemyManager enemy)
    {
        CloseAllPanels();
    }
}
