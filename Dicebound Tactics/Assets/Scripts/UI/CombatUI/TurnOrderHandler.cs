using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHandler : MonoBehaviour
{
    [SerializeField] GameObject ImageContainer;
    [SerializeField] GameObject CurrentTurnHolderImage;

    void Start()
    {
        CreateTurnOrderPortraits();
    }

    void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.GameIsPlaying)
        {
            CreateTurnOrderPortraits();
        }

        UpdateCurrentTurnHolder();
    }

    private void CreateTurnOrderPortraits()
    {
        // Clear existing portraits
        foreach (Transform child in ImageContainer.transform)
        {
            Destroy(child.gameObject);
        }

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

    public void UpdateTurnOrder()
    {
        CreateTurnOrderPortraits();
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
}
