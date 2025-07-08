using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEditor.Overlays;
using UnityEngine.UI;
using Unity.VisualScripting;

public class CombatNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bigNotificationText;
    [SerializeField] private CanvasGroup bigNotificationCanvasGroup;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private CanvasGroup notificationCanvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    private CanvasGroup smallCanvasGroup;
    private CanvasGroup bigCanvasGroup;

    private void Awake()
    {
        // Ensure we have the CanvasGroup reference
        if (smallCanvasGroup == null)
        {
            smallCanvasGroup = notificationText.transform.parent.GetComponent<CanvasGroup>();
        }

        if (bigCanvasGroup == null)
        {
            bigCanvasGroup = bigNotificationText.transform.parent.GetComponent<CanvasGroup>();
        }

        smallCanvasGroup.alpha = 0f; // Start invisible
        bigCanvasGroup.alpha = 0f; // Start invisible
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

        // Ensure we have the CanvasGroup reference
        if (smallCanvasGroup == null)
        {
            smallCanvasGroup = notificationText.transform.parent.GetComponent<CanvasGroup>();
        }

        // Fade in the notification
        smallCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            // Optionally, you can hide it after some time
            DOVirtual.DelayedCall(notifcationDuration, HideNotification);
        });
    }

    public void HideNotification()
    {
        smallCanvasGroup.DOFade(0f, fadeDuration);
    }

    public void ShowBigNotification(string message, float notifcationDuration = 2f)
    {
        if (bigNotificationText == null)
        {
            Debug.LogError("Big Notification TextMeshProUGUI is not assigned.");
            return;
        }

        bigNotificationText.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(bigNotificationText.rectTransform);

        // Ensure we have the CanvasGroup reference
        if (bigCanvasGroup == null)
        {
            bigCanvasGroup = bigNotificationText.transform.parent.GetComponent<CanvasGroup>();
        }

        //scale the canvas
        bigCanvasGroup.transform.localScale = Vector3.one * 0.75f;
        bigCanvasGroup.transform.DOScale(Vector3.one * 1.2f, fadeDuration).SetEase(Ease.OutBack);

        // Fade in the notification
        bigCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            // Optionally, you can hide it after some time
            DOVirtual.DelayedCall(notifcationDuration, HideBigNotification);
        });
    }

    public void HideBigNotification()
    {
        bigCanvasGroup.DOFade(0f, fadeDuration);
    }
}
