using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using DG.Tweening;

public class DieSideHandler : MonoBehaviour, IDropHandler
{
    [SerializeField] private TextMeshProUGUI dieSideText;
    [SerializeField] private GameObject modifierIndicator;
    [SerializeField] private Button button;
    private DiceSide dieSide;
    private CharacterMenuHandler characterMenuHandler;
    private DiceModifierInventory diceModifierInventory;

    void Start()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // if (transform.childCount == 0)
        // {
        //     if(dieSide.modifier != null)
        //     {
        //         diceModifierInventory.AddItem(dieSide.modifier, 1);
        //     }

        //     dieSide.modifier = eventData.pointerDrag.GetComponent<DiceModifierCardHandler>().GetDiceModifier();
        //     diceModifierInventory.RemoveItem(dieSide.modifier, 1);
        //     if (dieSide.modifier != null)
        //     {
        //         modifierIndicator.SetActive(true);
        //         characterMenuHandler.UpdateModifierDescription(dieSide.modifier);
        //     }
        //     else
        //     {
        //         modifierIndicator.SetActive(false);
        //         characterMenuHandler.UpdateModifierDescription(null);
        //     }
        // }
    }

    public void SetDieSide(DiceSide side, CharacterMenuHandler characterMenuHandler, DiceModifierInventory diceModifierInventory)
    {
        dieSide = side;
        this.characterMenuHandler = characterMenuHandler;
        this.diceModifierInventory = diceModifierInventory;

        if (dieSideText != null)
        {
            dieSideText.text = side.value.ToString();
        }

        if (modifierIndicator != null)
        {
            modifierIndicator.SetActive(side.HasModifier());
        }

        if (button != null)
        {
            button.onClick.AddListener(() =>
            {
                EventManager.TriggerMenuButtonPressed();
                if (side.HasModifier())
                {
                    if (characterMenuHandler != null)
                    {
                        characterMenuHandler.UpdateModifierDescription(side.modifier);
                    }
                }
                else
                {
                    if (characterMenuHandler != null)
                    {
                        characterMenuHandler.UpdateModifierDescription(null);
                    }
                }

                CharacterMenuDiceCustomizationHandler charactersHandler = characterMenuHandler.GetCharacterMenuDiceCustomizationHandler();

                if (charactersHandler != null)
                {
                    charactersHandler.SetStagedDieSide(this);
                    charactersHandler.ToggleEditButtons(true);
                    charactersHandler.ToggleModifierCanvasGroup(false);
                }
            });
        }
    }

    public DiceSide GetDieSide()
    {
        return dieSide;
    }

    public void ApplyModifierToDiceSide(DiceModifier modifier)
    {
        if (dieSide.modifier != null)
        {
            diceModifierInventory.AddItem(dieSide.modifier, 1);
        }

        dieSide.modifier = modifier;
        diceModifierInventory.RemoveItem(dieSide.modifier, 1);
        if (dieSide.modifier != null)
        {
            modifierIndicator.SetActive(true);
            characterMenuHandler.UpdateModifierDescription(dieSide.modifier);
        }
        else
        {
            modifierIndicator.SetActive(false);
            characterMenuHandler.UpdateModifierDescription(null);
        }

        characterMenuHandler.GetCharacterMenuDiceCustomizationHandler().SetInventory();
        characterMenuHandler.GetCharacterMenuDiceCustomizationHandler().SetStagedDieSide(null);
        characterMenuHandler.GetCharacterMenuDiceCustomizationHandler().ToggleEditButtons(false);
        characterMenuHandler.GetCharacterMenuDiceCustomizationHandler().ToggleModifierCanvasGroup(false);
    }
}
