using UnityEngine;
using TacticsToolkit;
using UnityEngine.UI;

public class CharacterMenuDiceCustomizationHandler : MonoBehaviour
{
    [SerializeField] private CharacterMenuHandler characterMenuHandler;
    [SerializeField] private CharacterCard characterCard;
    [SerializeField] private Image portraitImage;
    void Start()
    {
        
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
    }
}
