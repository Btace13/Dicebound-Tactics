using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class DieSideHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dieSideText;
    [SerializeField] private GameObject modifierIndicator;
    [SerializeField] private Button button;

  void Start()
  {
    if(button == null)
    {
      button = GetComponent<Button>();
    }
  }

  public void SetDieSide(DiceSide side, CharacterMenuHandler characterMenuHandler)
    {
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
                Debug.Log($"Die side '{side.value}' clicked with modifier: {side.HasModifier()}");
            });
        }
    }
}
