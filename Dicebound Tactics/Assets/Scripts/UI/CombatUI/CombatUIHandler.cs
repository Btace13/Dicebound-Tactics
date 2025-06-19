using UnityEngine;
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
            else
            {
                Debug.LogError("Attempted to set CurrentPanel to null.");
            }
        }
    }

    [Header("Panel References")]
    public ActionPanel ActionPanel;
    public AbilityPanel AbilityPanel;
    public ItemPanel ItemPanel;

    [Header("Screen Space UI References")]
    [SerializeField] private CanvasGroup screenSpaceCanvasGroup;
    [SerializeField] private CanvasGroup panelInputsCanvasGroup;

    private void Awake()
    {
        if (currentPanel == null)
        {
            Debug.LogError("Current Panel is not set in CombatUIHandler.");
        }
    }

    public void MoveCanvasToGameObject(GameObject target)
    {
        MoveCanvasToTarget(target.transform);
    }

    public void MoveCanvasToTarget(Transform target)
    {
        if (currentPanel == null || target == null)
        {
            Debug.LogError("Current Panel or target is not set.");
            return;
        }

        // Check if the target is a CharacterManager
        // If it is, populate the AbilityPanel with the character's abilities
        if (target.TryGetComponent(out CharacterManager character))
        {
            if (AbilityPanel != null)
            {
                AbilityPanel.PopulateAbilityPanel(character);
            }
            else
            {
                Debug.LogError("AbilityPanel is not set in CombatUIHandler.");
            }

            if (ItemPanel != null)
            {
                ItemPanel.PopulateItemPanel(character);
            }
            else
            {
                Debug.LogError("ItemPanel is not set in CombatUIHandler.");
            }

            return;
        }

        Vector3 targetPosition = target.position + _canvasOffset;

        currentPanel.FadeOutCanvas(() =>
        {
            // Move the canvas to the target position after fading out
            transform.position = targetPosition;

            // Fade in the canvas after moving
            currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);
        }, _fadeDuration, Ease.InOutQuad);
    }

    public void OpenPanel(CombatPanel panel)
    {
        if (panel == null)
        {
            Debug.LogError("Panel to open is not set.");
            return;
        }

        if (currentPanel != null)
        {
            // store the current panel as previous
            panel.PreviousPanel = currentPanel;

            // Fade out the current panel before switching
            currentPanel.FadeOutCanvas(() =>
            {
                currentPanel = panel;
                currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);
            }, _fadeDuration, Ease.InOutQuad);
        }
    }

    public void CloseCurrentPanel()
    {
        if (currentPanel == null)
        {
            Debug.LogError("No current panel to close.");
            return;
        }

        // if the panel you're returning to has no previous panel,
        // hide the screen space inputs
        if (currentPanel.PreviousPanel.PreviousPanel == null)
        {
            ShowScreenSpacePanelInputs(false);
        }

        currentPanel.FadeOutCanvas(() =>
        {

            if (currentPanel != null)
            {
                currentPanel = currentPanel.PreviousPanel;
                currentPanel.FadeInCanvas(null, _fadeDuration, Ease.InOutQuad);
            }
        }, _fadeDuration, Ease.InOutQuad);
    }

    public void ShowScreenSpacePanelInputs(bool enable)
    {
        if (panelInputsCanvasGroup == null)
        {
            Debug.LogError("Panel Inputs Canvas Group is not set.");
            return;
        }

        DOTween.To(() => panelInputsCanvasGroup.alpha, x => panelInputsCanvasGroup.alpha = x, enable ? 1 : 0, _fadeDuration)
            .OnStart(() =>
            {
                panelInputsCanvasGroup.interactable = enable;
                panelInputsCanvasGroup.blocksRaycasts = enable;
            })
            .OnComplete(() =>
            {
                if (!enable)
                {
                    panelInputsCanvasGroup.interactable = false;
                    panelInputsCanvasGroup.blocksRaycasts = false;
                }
            });
    }
}
