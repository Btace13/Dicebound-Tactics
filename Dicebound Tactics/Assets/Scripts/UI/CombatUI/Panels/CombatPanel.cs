using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CombatPanel : MonoBehaviour
{
    public CanvasGroup PanelCanvasGroup { get; set; }
    public Ease FadeEase { get; set; } = Ease.InOutQuad;
    public Sequence FadeSequence { get; set; }
    public CombatPanel PreviousPanel { get; set; } = null;

    void Awake()
    {
        if (PanelCanvasGroup == null)
        {
            PanelCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

#if UNITY_EDITOR
    private void OValidate()
    {
        if (PanelCanvasGroup == null)
        {
            PanelCanvasGroup = GetComponent<CanvasGroup>();
        }
    }
#endif

    public void FadeCanvas(bool fadeIn, UnityAction OnFadeComplete = null, float duration = 0.15f, Ease ease = Ease.InOutQuad)
    {
        gameObject.SetActive(true);

        if (FadeSequence != null && FadeSequence.IsActive())
        {
            FadeSequence.Kill();
        }

        FadeSequence = DOTween.Sequence();

        if (fadeIn)
        {
            FadeSequence.AppendCallback(() =>
            {
                PanelCanvasGroup.blocksRaycasts = true;
                PanelCanvasGroup.interactable = true;
                // Fade in the canvas after moving
                PanelCanvasGroup.DOFade(1, 0.15f).SetEase(FadeEase);
            });
        }
        else
        {
            if (PanelCanvasGroup.alpha > 0)
            {
                PanelCanvasGroup.blocksRaycasts = false;
                PanelCanvasGroup.interactable = false;
                // Fade out the canvas before moving
                FadeSequence.Append(PanelCanvasGroup.DOFade(0, 0.15f).SetEase(FadeEase));
            }
        }

        FadeSequence.AppendCallback(() =>
        {
            OnFadeComplete?.Invoke();
        });
    }

    public void FadeInCanvas(UnityAction OnFadeComplete = null, float duration = 0.15f, Ease ease = Ease.InOutQuad)
    {
        FadeCanvas(true, OnFadeComplete, duration, ease);
    }

    public void FadeOutCanvas(UnityAction OnFadeComplete = null, float duration = 0.15f, Ease ease = Ease.InOutQuad)
    {
        FadeCanvas(false, OnFadeComplete, duration, ease);
    }
}
