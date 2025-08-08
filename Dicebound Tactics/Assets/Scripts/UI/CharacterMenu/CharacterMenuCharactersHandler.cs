using UnityEngine;
using UnityEngine.UI;

public class CharacterMenuCharactersHandler : MonoBehaviour
{
    [SerializeField] CharacterMenuHandler characterMenuHandler;
    [SerializeField] private GameObject characterPrefab;
    void Start()
    {
        CreateCharacters();
    }

    private void CreateCharacters()
    {
        TurnManager.Instance.playerUnits.ForEach(character =>
        {
            GameObject characterObject = Instantiate(characterPrefab, transform);
            CharacterMenuPortraitHandler portraitHandler = characterObject.GetComponent<CharacterMenuPortraitHandler>();
            if (portraitHandler != null)
            {
                portraitHandler.InitializeCharacterCard(character, characterMenuHandler);
            }
        });
    }
}
