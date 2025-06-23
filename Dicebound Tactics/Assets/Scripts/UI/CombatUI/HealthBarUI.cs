using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image delayedHealthBarFill;
    [SerializeField] private float fillDelay = 0.5f; // Delay before


    public void SetHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0)
        {
            healthBarFill.fillAmount = 0f;
            return;
        }

        float targetFillAmount = currentHealth / maxHealth;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(healthBarFill.DOFillAmount(targetFillAmount, fillDelay).SetEase(Ease.Linear));
        sequence.AppendInterval(0.5f);
        sequence.Append(delayedHealthBarFill.DOFillAmount(targetFillAmount, fillDelay).SetEase(Ease.Linear));
        sequence.Play();
    }

    public void SetVisibility(bool isVisible, float fadeDurationOverride = -1f)
    {
        float duration = fadeDurationOverride > 0 ? fadeDurationOverride : fadeDuration;

        if (isVisible)
        {
            canvasGroup.DOFade(1f, duration).SetUpdate(true);
        }
        else
        {
            canvasGroup.DOFade(0f, duration).SetUpdate(true);
        }
    }

    public void SetColor(Color color)
    {
        healthBarFill.color = color;
    }
}
