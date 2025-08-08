using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenuPortraitHandler : MonoBehaviour
{
    [SerializeField] private CharacterCard characterCard;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Button cardButton;
    private CharacterManager currentCharacter;
    private CharacterMenuHandler characterMenuHandler;

    void Start()
    {
        if (cardButton == null)
        {
            cardButton = GetComponent<Button>();
        }
    }

    public void InitializeCharacterCard(CharacterManager characterManager, CharacterMenuHandler menuHandler)
    {
        characterCard.SetCharacterInfo(characterManager);
        currentCharacter = characterManager;
        characterMenuHandler = menuHandler;

        if (characterManager.characterFullBodyImage != null)
        {
            portraitImage.sprite = characterManager.characterFullBodyImage;
        }

        if(cardButton != null)
        {
            cardButton.onClick.AddListener(() =>
            {
                EventManager.TriggerMenuButtonPressed();
                menuHandler.CloseCharacterSelector();
                menuHandler.OpenCharacterScreen(characterManager);
            });
        }
    }

    public CharacterManager GetCardCharacter()
    {
        return currentCharacter;
    }
}
