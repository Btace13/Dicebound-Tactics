using UnityEngine;
using TacticsToolkit;
using UnityEngine.UI;
using Sirenix.Utilities;

public class CharacterMenuDiceCustomizationHandler : MonoBehaviour
{
    [SerializeField] private DiceModifierInventory diceModifierInventory;
    [SerializeField] private CharacterMenuHandler characterMenuHandler;
    [SerializeField] private CharacterCard characterCard;
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject diceModifierCardPrefab;
    [SerializeField] private GameObject diceModifierContainer;
    [SerializeField] private GameObject diceSidePrefab;
    [SerializeField] private GameObject characterDiceSidesContainer;
    private DiceModifierCardHandler stagedDiceModifierCard;

    void Start()
    {
        diceModifierInventory.diceModifierItems.ForEach(pair =>
        {
            GameObject card = Instantiate(diceModifierCardPrefab, diceModifierContainer.transform);
            DiceModifierCardHandler diceModifierCard = card.GetComponent<DiceModifierCardHandler>();
            if (diceModifierCard != null)
            {
                diceModifierCard.SetDiceModifier(this, characterMenuHandler, pair.Key, pair.Value);
            }
        });
    }

    public void SetCharacter(CharacterManager character)
    {
        if (characterCard != null)
        {
            characterCard.SetCharacterInfo(character);
        }

        if (character.characterFullBodyImage != null)
        {
            portraitImage.sprite = character.characterFullBodyImage;
        }

        foreach (Transform child in characterDiceSidesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        character.equippedDice.sides.ForEach(side =>
        {
            GameObject dieSideObject = Instantiate(diceSidePrefab, characterDiceSidesContainer.transform);
            DieSideHandler dieSideHandler = dieSideObject.GetComponent<DieSideHandler>();
            if (dieSideHandler != null)
            {
                dieSideHandler.SetDieSide(side, characterMenuHandler, diceModifierInventory);
            }
        });
    }

    public bool HasStagedDiceModifierCard()
    {
        return stagedDiceModifierCard != null;
    }

    public void SetStagedDiceModifierCard(DiceModifierCardHandler card)
    {
        foreach (Transform child in diceModifierContainer.transform)
        {
            DiceModifierCardHandler diceModifierCard = child.GetComponent<DiceModifierCardHandler>();
            if (diceModifierCard != null)
            {
                diceModifierCard.UnstageDiceModifierCard(card);
            }
        }

        stagedDiceModifierCard = card;
    }
    
    public DiceModifierCardHandler GetStagedDiceModifierCard()
    {
        return stagedDiceModifierCard;
    }
}
