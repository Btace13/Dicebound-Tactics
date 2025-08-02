using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;

public class ModifierNotification : MonoBehaviour
{
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

    public void ShowModifier(DiceModifier dice)
    {
        Debug.Log("ModifierNotification: ShowModifier called with dice: " + dice.Name);

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

}
