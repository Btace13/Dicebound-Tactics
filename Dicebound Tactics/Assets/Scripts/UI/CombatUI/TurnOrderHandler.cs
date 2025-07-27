using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;
using DG.Tweening;

public class TurnOrderHandler : MonoBehaviour
{
    [SerializeField] GameObject ImageContainer;
    [SerializeField] GameObject PortraitPrefab;
    [SerializeField] GameObject CurrentTurnHolderImage;

    private bool isCreatingPortraits = false;

    private void Awake()
    {
        // Subscribe to events
        EventManager.OnNewActiveEntity += OnTurnStarted;
        EventManager.OnCombatEncounterEnded += OnCombatEncounterEnded;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        EventManager.OnNewActiveEntity -= OnTurnStarted;
        EventManager.OnCombatEncounterEnded -= OnCombatEncounterEnded;
    }

    public async void OnTurnStarted(Entity entity)
    {
        if (ImageContainer == null || PortraitPrefab == null || CurrentTurnHolderImage == null)
            return;

        ImageContainer.SetActive(true);

        isCreatingPortraits = true;
        await CreateTurnOrderPortraits();
        isCreatingPortraits = false;

        UpdateCurrentTurnHolder(entity);
    }

    public async void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        await ClearTurnOrder();
    }

    private async Task CreateTurnOrderPortraits()
    {
        if (TurnManager.Instance == null) return;

        await ClearTurnOrder();

        foreach (var entity in TurnManager.Instance.GetRemainingEntitiesThisRound())
        {
            if (entity.portrait != null)
            {
                GameObject portraitObject = Instantiate(PortraitPrefab, ImageContainer.transform);
                portraitObject.transform.GetChild(0).GetComponent<Image>().sprite = entity.portrait;
            }
        }
    }

    private void UpdateCurrentTurnHolder(Entity entity = null)
    {
        if (TurnManager.Instance == null) return;

        Entity turnHolder = entity ?? TurnManager.Instance.GetCurrentUnit();

        if (turnHolder == null || turnHolder.portrait == null)
        {
            CurrentTurnHolderImage.SetActive(false);
            return;
        }

        CurrentTurnHolderImage.SetActive(true);
        CurrentTurnHolderImage.transform.GetChild(0).GetComponent<Image>().sprite = turnHolder.portrait;
        // DOTween scale animation
        var rectTransform = CurrentTurnHolderImage.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one; // Reset scale in case previous tween is still active

        // Optional: kill any existing tweens to avoid stacking
        rectTransform.DOKill();

        rectTransform
            .DOScale(1.2f, 0.2f) // scale up
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                rectTransform
                    .DOScale(1f, 0.2f) // scale back down
                    .SetEase(Ease.InBack);
            });

        var textMeshPro = CurrentTurnHolderImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = turnHolder.name;
        }
    }

    public async Task ClearTurnOrder()
    {
        if (ImageContainer == null) return;

        foreach (Transform child in ImageContainer.transform)
        {
            if (child.gameObject == CurrentTurnHolderImage) continue;
            Destroy(child.gameObject);
        }

        await Task.Delay(1); // Let Unity complete object destruction

        CurrentTurnHolderImage.SetActive(false);
    }
}
