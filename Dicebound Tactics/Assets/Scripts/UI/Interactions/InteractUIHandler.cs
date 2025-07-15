using UnityEngine;
using DG.Tweening;

public class InteractUIHandler : MonoBehaviour
{
    public static InteractUIHandler Instance { get; private set; }

    [SerializeField] CanvasGroup interactCanvasGroup;
    [SerializeField] float fadeDuration = 0.2f;

    private float normalizedFadeValue = 0f;
    private Tween fadeTween;

    private Interactable currentInteractable;
    private Interactable previousInteractable;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (interactCanvasGroup == null)
        {
            interactCanvasGroup = GetComponent<CanvasGroup>();
        }

        HideInteractUI();
    }

    public void SetInteractUIVisibility(bool isVisible)
    {
        if (interactCanvasGroup == null) return;

        if (fadeTween != null && fadeTween.IsActive() && fadeTween.IsPlaying())
        {
            fadeTween.Kill();
        }

        float updateDuration = fadeDuration * (1f - normalizedFadeValue);

        fadeTween = interactCanvasGroup.DOFade(isVisible ? 1f : 0f, fadeDuration).OnUpdate(() =>
            {
                normalizedFadeValue = interactCanvasGroup.alpha;
            })
            .OnComplete(() =>
            {
                normalizedFadeValue = isVisible ? 1f : 0f;
            });
    }

    private void Update()
    {
        if (interactCanvasGroup == null || interactCanvasGroup.alpha <= 0f)
        {
            return; // No need to update if the UI is not visible
        }

        if (currentInteractable != null)
        {
            UpdateScreenPosition(currentInteractable);
        }
    }

    public void ShowInteractUI(Interactable interactable)
    {
        if (interactable == null)
        {
            Debug.LogWarning("Interactable is null.");
            return;
        }

        UpdateScreenPosition(interactable);

        interactCanvasGroup.alpha = 0f; // Reset alpha before showing
        interactCanvasGroup.gameObject.SetActive(true);
        SetInteractUIVisibility(true);

        currentInteractable = interactable;
    }

    public void HideInteractUI()
    {
        SetInteractUIVisibility(false);
        previousInteractable = currentInteractable;
        currentInteractable = null;
    }

    private void UpdateScreenPosition(Interactable interactable)
    {
        if (interactCanvasGroup == null)
        {
            return;
        }

        if (interactable == null)
        {
            interactable = previousInteractable;
        }

        float extentsY = 1.5f;

        if (interactable.TryGetComponent(out Collider col))
        {
            extentsY = col.bounds.extents.y;
        }
        else if (interactable.TryGetComponent(out Renderer rend))
        {
            extentsY = rend.bounds.extents.y;
        }

        Vector3 worldTargetPosition = interactable.transform.position + Vector3.up * (extentsY + interactCanvasGroup.alpha); // Adjust height for UI display
        Vector3 screenTargetPosition = Camera.main.WorldToScreenPoint(worldTargetPosition);

        interactCanvasGroup.transform.position = screenTargetPosition;

        if (interactCanvasGroup.alpha <= 0f)
        {
            previousInteractable = null;
        }
    }
}
