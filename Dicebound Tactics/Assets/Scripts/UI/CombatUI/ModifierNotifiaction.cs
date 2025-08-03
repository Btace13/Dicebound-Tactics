using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using TacticsToolkit;

public enum ModifierNotificationType
{
    Character,
    Enemy
}

public class ModifierNotification : MonoBehaviour
{
    [SerializeField] private ModifierNotificationType notificationType = ModifierNotificationType.Character;
    [SerializeField] private TextMeshProUGUI modifierName;
    [SerializeField] private TextMeshProUGUI modifierDescription;
    [SerializeField] private Image modifierIcon;
    [SerializeField] private CanvasGroup modifierCanvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        if (modifierCanvasGroup == null)
        {
            modifierCanvasGroup = GetComponent<CanvasGroup>();
        }
        modifierCanvasGroup.alpha = 0f; // Start invisible

        // Event Listeners
        EventManager.OnModifierApplied += ShowModifier;
    }

    void OnDisable()
    {
        EventManager.OnModifierApplied -= ShowModifier;
    }

    public void ShowModifier(DiceModifier dice, Entity user)
    {
        if (notificationType == ModifierNotificationType.Enemy)
            return;

        if (modifierName == null || modifierDescription == null)
        {
            Debug.LogError("Modifier UI TextMeshProUGUI fields are not assigned.");
            return;
        }

        modifierCanvasGroup.DOKill(); // cancel ongoing tweens

        modifierName.text = dice.Name;
        modifierDescription.text = dice.Description;

        if (modifierIcon != null)
            modifierIcon.sprite = dice.Icon;

        modifierCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            DOVirtual.DelayedCall(3f, HideModifier);
        });
    }

    public void HideModifier()
    {
        if (modifierCanvasGroup.alpha <= 0f) return;

        modifierCanvasGroup.DOKill(); // stop fade-in if needed
        modifierCanvasGroup.DOFade(0f, fadeDuration);
    }

    public void ShowEnemyModifier(DiceModifier dice, Entity user)
    {
        if (notificationType == ModifierNotificationType.Character)
            return;

        if (modifierName == null || modifierDescription == null)
        {
            Debug.LogError("Modifier UI TextMeshProUGUI fields are not assigned.");
            return;
        }

        modifierCanvasGroup.DOKill(); // cancel ongoing tweens

        modifierName.text = dice.Name;
        modifierDescription.text = dice.Description;

        if (modifierIcon != null)
            modifierIcon.sprite = dice.Icon;

        modifierCanvasGroup.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            DOVirtual.DelayedCall(3f, HideModifier);
        });
    }

}
