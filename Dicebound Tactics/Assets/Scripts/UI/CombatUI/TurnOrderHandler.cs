using UnityEngine;
using UnityEngine.UI;

public class TurnOrderHandler : MonoBehaviour
{
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
    }

    private void CreateTurnOrderPortraits()
    {
        // Clear existing portraits
        foreach (Transform child in transform)
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
                portraitObject.transform.SetParent(transform);
                portraitObject.AddComponent<Image>().sprite = entity.portrait;
            }
        }
    }
    
    public void UpdateTurnOrder()
    {
        CreateTurnOrderPortraits();
    }
}
