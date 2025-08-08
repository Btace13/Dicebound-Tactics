using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Animancer.Examples.AnimatorControllers.GameKit;

public class DiceModifierCardHandler : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI modifierNameText;
  [SerializeField] private TextMeshProUGUI modifierValueText;
  [SerializeField] private Button diceModifierButton;
  private CharacterMenuHandler characterMenuHandler;

  void Start()
  {
    if(diceModifierButton == null)
    {
      diceModifierButton = GetComponent<Button>();
    }
  }

  public void SetDiceModifier(CharacterMenuHandler characterMenuHandler, DiceModifier diceModifier, int quantity)
  {
    this.characterMenuHandler = characterMenuHandler;
    modifierNameText.text = diceModifier.name;
    modifierValueText.text = "x" + quantity.ToString();

    diceModifierButton.onClick.AddListener(() =>
    {
      EventManager.TriggerMenuButtonPressed();
      characterMenuHandler.UpdateModifierDescription(diceModifier);
      Debug.Log($"Dice Modifier '{diceModifier.name}' clicked. Quantity: {quantity}");
    });
  }
}
