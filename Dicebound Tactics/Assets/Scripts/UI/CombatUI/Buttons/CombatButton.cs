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

    UnityAction OnClickAction;

    private void Awake()
    {
        canvasGroup ??= GetComponent<CanvasGroup>();
        button ??= GetComponent<Button>();

        button.onClick.AddListener(OnClickAction);
    }

    public virtual void AnimateIn()
    {
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
        buttonText.SetText(text);
        OnClickAction = onClickAction;
    }
}
