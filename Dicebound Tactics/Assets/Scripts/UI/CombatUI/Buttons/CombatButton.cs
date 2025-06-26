using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Button))]
public class CombatButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    private CanvasGroup canvasGroup;
    private Button button;
    public Button Button
    {
        get
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }
            return button;
        }
    }

    UnityAction OnClickAction { get; set; }

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();

        button.onClick.AddListener(InvokeClickAction);
    }

    void OnDestroy()
    {
        button.onClick.RemoveListener(InvokeClickAction);
    }

    public virtual void AnimateIn()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        DOTween.To(
            () => canvasGroup.alpha,
            x => canvasGroup.alpha = x,
            1f,
            0.2f
        ).SetEase(Ease.InFlash);
    }

    public virtual void SetupButton(string text, UnityAction onClickAction)
    {
        Debug.Log($"Setting up button for {text}, action null? {onClickAction == null}");

        buttonText.SetText(text);
        OnClickAction = onClickAction;
    }

    protected void InvokeClickAction()
    {
        try
        {
            if (OnClickAction == null)
            {
                Debug.LogError("OnClickAction is null. Cannot invoke.");
                return;
            }

            if (button.interactable == false)
            {
                Debug.LogWarning("Button is not interactable. Cannot invoke OnClickAction.");
                return;
            }

            OnClickAction.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception occurred while invoking OnClickAction: {ex.Message}");
        }
    }
}
