using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class TurnOrderHandler : MonoBehaviour
{
    [SerializeField] GameObject ImageContainer;
    [SerializeField] GameObject PortraitPrefab;
    [SerializeField] GameObject CurrentTurnHolderImage;

    private void Awake()
    {
        // Event Listeners
        EventManager.OnNewActiveEntity += OnTurnStarted;
        EventManager.OnCombatEncounterEnded += OnCombatEncounterEnded;
    }

    void OnDisable()
    {
        EventManager.OnNewActiveEntity -= OnTurnStarted;
        EventManager.OnCombatEncounterEnded -= OnCombatEncounterEnded;
    }

    public async void OnTurnStarted(Entity entity)
    {
        ImageContainer.SetActive(true);

        if (ImageContainer.transform.childCount == 1)
        {
            // If there are no portraits, create them
            await CreateTurnOrderPortraits();
        }

        UpdateCurrentTurnHolder(entity);
    }

    public void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        ClearTurnOrder();
    }

    private async Task CreateTurnOrderPortraits()
    {
        await ClearTurnOrder();

        // Create new portraits for each character in the turn order
        foreach (var entity in TurnManager.Instance.GetRemainingEntitiesThisRound())
        {
            if (entity.portrait != null)
            {
                // instantiate a new image for the portrait
                GameObject portraitObject = Instantiate(PortraitPrefab, ImageContainer.transform);
                portraitObject.transform.GetChild(0).GetComponent<Image>().sprite = entity.portrait;
            }
        }
    }

    private void UpdateCurrentTurnHolder(Entity entity = null)
    {
        Entity turnHolder = entity ?? TurnManager.Instance.GetCurrentUnit();

        if (turnHolder == null || turnHolder.portrait == null)
        {
            CurrentTurnHolderImage.SetActive(false);
            return;
        }

        CurrentTurnHolderImage.SetActive(true);
        CurrentTurnHolderImage.transform.GetChild(0).GetComponent<Image>().sprite = turnHolder.portrait;
        // get textmeshpro component in child and update it with the name of the current unit
        var textMeshPro = CurrentTurnHolderImage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.text = turnHolder.name;
        }
    }

    public async Task ClearTurnOrder()
    {
        // Clear the turn order UI minus the first child (which is the current turn holder)
        foreach (Transform child in ImageContainer.transform)
        {
            if (child.gameObject == CurrentTurnHolderImage) continue; // Skip the current turn holder
            Destroy(child.gameObject);
        }

        while (ImageContainer.transform.childCount > 1)
        {
            await Task.Yield(); // Wait for the UI to update
        }

        CurrentTurnHolderImage.SetActive(false);
    }
}
