using UnityEngine;
using DG.Tweening;

public class ContinueButtonController : MonoBehaviour
{
    [SerializeField] private RectTransform arrowReference;

    private Vector3 _startPosition;

    private void Awake()
    {
        if (arrowReference == null)
        {
            Debug.LogError("Arrow reference is not assigned in ContinueButtonController.");
        }

        _startPosition = arrowReference.localPosition;
    }

    void OnEnable()
    {
        AnimateArrow();
    }

    void OnDisable()
    {
        // Stop the arrow animation when the button is disabled
        StopArrowAnimation();
    }

    public void AnimateArrow()
    {
        if (arrowReference == null) return;

        // Reset the arrow position
        arrowReference.localPosition = _startPosition;

        // Create a bounce effect: up slower, down faster
        Sequence bounceSequence = DOTween.Sequence();
        bounceSequence.SetUpdate(true); // Update during timeScale = 0

        bounceSequence.Append(
            arrowReference.DOLocalMoveY(45f, 0.7f)
                .SetEase(Ease.OutQuad) // Up slower
        );
        bounceSequence.Append(
            arrowReference.DOLocalMoveY(0f, 0.3f)
                .SetEase(Ease.InQuad) // Down faster
        );
        bounceSequence.SetLoops(-1);
    }

    private void StopArrowAnimation()
    {
        if (arrowReference == null) return;

        // Stop the animation and reset the position
        arrowReference.DOKill();
        arrowReference.localPosition = Vector3.zero;
    }
}
