using UnityEngine;

public class CharacterCardsHandler : MonoBehaviour
{
  [SerializeField] GameObject characterCardPrefab;
  
  public void InitializeCharacterCards()
  {
    foreach (var character in TurnManager.Instance.playerUnits)
    {
      if (character == null || character.statsContainer == null)
      {
        Debug.Log("Character or statsContainer is null, skipping card creation.");
        continue;
      }

      GameObject card = Instantiate(characterCardPrefab, transform);
      card.name = character.name + " Card";

      CharacterCard cardScript = card.GetComponent<CharacterCard>();
      if (cardScript != null)
      {
        cardScript.SetCharacterInfo(character);
      }
    }
  }
}