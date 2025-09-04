using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class CombatPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup _panelCanvasGroup; // cached to avoid null refs
    public CanvasGroup PanelCanvasGroup
    {
        get
        {
            if (_panelCanvasGroup == null)
            {
                _panelCanvasGroup = GetComponent<CanvasGroup>();
            }
            return _panelCanvasGroup;
        }
        set => _panelCanvasGroup = value;
    }

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
    private void OnValidate()
    {
        if (_panelCanvasGroup == null)
        {
            _panelCanvasGroup = GetComponent<CanvasGroup>();
        }
    }
#endif

    public void FadeCanvas(bool fadeIn, UnityAction OnFadeComplete = null, float duration = 0.15f, Ease ease = Ease.InOutQuad)
    {
        // Ensure we have a CanvasGroup before proceeding
        var cg = PanelCanvasGroup;
        if (cg == null)
        {
            Debug.LogError($"[{nameof(CombatPanel)}] Missing CanvasGroup on {name}. Aborting fade.");
            OnFadeComplete?.Invoke();
            return;
        }

        gameObject.SetActive(true);

        if (FadeSequence != null && FadeSequence.IsActive())
        {
            FadeSequence.Kill();
        }

        FadeSequence = DOTween.Sequence();

        if (fadeIn)
        {
            FadeSequence.OnStart(() =>
            {
                cg.blocksRaycasts = true;
                cg.interactable = true;
            });
            FadeSequence.Append(cg.DOFade(1f, duration).SetEase(ease));
        }
        else
        {
            if (cg.alpha > 0f)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
                FadeSequence.Append(cg.DOFade(0f, duration).SetEase(ease));
            }
        }

        FadeSequence.AppendCallback(() => { OnFadeComplete?.Invoke(); });
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
