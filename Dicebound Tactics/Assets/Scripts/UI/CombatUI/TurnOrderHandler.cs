using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TurnOrderHandler : MonoBehaviour
{
    [SerializeField] GameObject ImageContainer;
    [SerializeField] GameObject CurrentTurnHolderImage;

    public async void OnTurnStarted()
    {
        ImageContainer.SetActive(true);

        if (ImageContainer.transform.childCount == 0)
        {
            // If there are no portraits, create them
            await CreateTurnOrderPortraits();
        }

        UpdateCurrentTurnHolder();
    }

    public void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        // Handle the end of the combat encounter
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
                GameObject portraitObject = new(entity.name + " Portrait");
                portraitObject.transform.SetParent(ImageContainer.transform, false);
                portraitObject.AddComponent<Image>().sprite = entity.portrait;
            }
        }
    }

    private void UpdateCurrentTurnHolder()
    {
        if (TurnManager.Instance == null || TurnManager.Instance.GetCurrentUnit() == null)
        {
            CurrentTurnHolderImage.SetActive(false);
            return;
        }

        CurrentTurnHolderImage.SetActive(true);
        CurrentTurnHolderImage.GetComponent<Image>().sprite = TurnManager.Instance.GetCurrentUnit().portrait;
    }

    public async Task ClearTurnOrder()
    {
        // Clear the turn order UI
        foreach (Transform child in ImageContainer.transform)
        {
            Destroy(child.gameObject);
        }

        while (ImageContainer.transform.childCount > 0)
        {
            await Task.Yield(); // Wait for the UI to update
        }

        CurrentTurnHolderImage.SetActive(false);
    }
}
