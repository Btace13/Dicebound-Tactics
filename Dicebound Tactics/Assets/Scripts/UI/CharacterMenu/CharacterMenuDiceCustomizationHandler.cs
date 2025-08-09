using UnityEngine;
using TacticsToolkit;
using UnityEngine.UI;
using Sirenix.Utilities;
using DG.Tweening;
using System.Collections.Generic;

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
    [SerializeField] private GameObject addModifierButton;
    [SerializeField] private GameObject removeModifierButton;
    [SerializeField] private CanvasGroup modifierCanvasGroup;

    private DieSideHandler stagedDieSide;

    void Start()
    {
        SetInventory();
    }

    public void SetInventory()
    {
        foreach (Transform child in diceModifierContainer.transform)
        {
            Destroy(child.gameObject);
        }

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

    public bool HasStagedDieSide()
    {
        return stagedDieSide != null;
    }

    public void SetStagedDieSide(DieSideHandler dieSideHandler)
    {
        stagedDieSide = dieSideHandler;

        foreach (Transform child in characterDiceSidesContainer.transform)
        {
            DieSideHandler handler = child.GetComponent<DieSideHandler>();
            if (handler != null)
            {
                if (handler == stagedDieSide)
                {
                    child.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
                }
                else
                {
                    child.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
                }
            }
        }
    }

    public DieSideHandler GetStagedDieSide()
    {
        return stagedDieSide;
    }

    public void ToggleEditButtons(bool isVisible)
    {
        if (addModifierButton != null)
        {
            addModifierButton.SetActive(isVisible);
            addModifierButton.GetComponent<Button>().interactable = isVisible;
        }

        if (removeModifierButton != null && stagedDieSide != null && stagedDieSide.GetDieSide().HasModifier() && isVisible)
        {
            removeModifierButton.SetActive(true);
            removeModifierButton.GetComponent<Button>().interactable = true;
        }
        else if (removeModifierButton != null)
        {
            removeModifierButton.SetActive(false);
            removeModifierButton.GetComponent<Button>().interactable = false;
        }
    }

    public void RemoveModifierFromStagedDieSide()
    {
        if (stagedDieSide != null && stagedDieSide.GetDieSide().HasModifier())
        {
            DiceModifier modifier = stagedDieSide.GetDieSide().modifier;
            diceModifierInventory.AddItem(modifier, 1);
            stagedDieSide.GetDieSide().modifier = null;
            stagedDieSide.SetDieSide(stagedDieSide.GetDieSide(), characterMenuHandler, diceModifierInventory);
            ToggleEditButtons(true);
            characterMenuHandler.UpdateModifierDescription(null);
            SetInventory();
        }
    }

    public void ToggleModifierCanvasGroup(bool value)
    {
        if (modifierCanvasGroup != null)
        {
            modifierCanvasGroup.interactable = value;
            modifierCanvasGroup.blocksRaycasts = value;
        }
    }
}
