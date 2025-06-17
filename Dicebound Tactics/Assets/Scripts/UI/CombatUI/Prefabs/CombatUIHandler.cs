using UnityEngine;
using DG.Tweening;

public class CombatUIHandler : MonoBehaviour
{
    [SerializeField] private CanvasGroup _combatCanvasGroup;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Ease _fadeEase = Ease.InOutQuad;

    private Sequence _fadeSequence;

    public void MoveCanvasToTarget(Transform target)
    {
        if (_combatCanvasGroup == null || target == null)
        {
            Debug.LogError("CombatCanvasGroup or target is not set.");
            return;
        }

        Vector3 targetPosition = target.position;

        if (_fadeSequence != null && _fadeSequence.IsActive())
        {
            _fadeSequence.Kill();
        }

        _fadeSequence = DOTween.Sequence();

        if (_combatCanvasGroup.alpha > 0)
        {
            _combatCanvasGroup.blocksRaycasts = false;
            _combatCanvasGroup.interactable = false;
            // Fade out the canvas before moving
            _fadeSequence.Append(_combatCanvasGroup.DOFade(0, 0.15f).SetEase(_fadeEase));
        }

        _fadeSequence.AppendCallback(() =>
        {
            _combatCanvasGroup.transform.position = targetPosition; // Move to target position
        });

        _fadeSequence.AppendCallback(() =>
        {
            _combatCanvasGroup.blocksRaycasts = true;
            _combatCanvasGroup.interactable = true;
            // Fade in the canvas after moving
            _combatCanvasGroup.DOFade(1, 0.15f).SetEase(_fadeEase);
        });
    }
}
