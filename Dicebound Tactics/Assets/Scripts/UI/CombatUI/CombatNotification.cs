using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEditor.Overlays;
using UnityEngine.UI;
using Unity.VisualScripting;

[RequireComponent(typeof(CanvasGroup))]
public class CombatNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI notificationText; // Assign this in the inspector
    [SerializeField] private float fadeDuration = 0.2f;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f; // Start invisible
    }

    public void ShowNotification(string message, float notifcationDuration = 2f)
    {
        if (notificationText == null)
        {
            Debug.LogError("Notification TextMeshProUGUI is not assigned.");
            return;
        }

        notificationText.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(notificationText.rectTransform);

        // Fade in the notification
        canvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            // Optionally, you can hide it after some time
            DOVirtual.DelayedCall(notifcationDuration, HideNotification);
        });
    }

    public void HideNotification()
    {
        canvasGroup.DOFade(0f, fadeDuration);
    }
}
