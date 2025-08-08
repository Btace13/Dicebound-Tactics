using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DiceModifierCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
  [SerializeField] private TextMeshProUGUI modifierNameText;
  [SerializeField] private TextMeshProUGUI modifierValueText;
  [SerializeField] private Button diceModifierButton;
  [SerializeField] private CanvasGroup group;
  private CharacterMenuHandler characterMenuHandler;
  private DiceModifier diceModifier;
  public Transform parentAfterDrag;
  private Transform canvasParent;

  void Start()
  {
    if (diceModifierButton == null)
    {
      diceModifierButton = GetComponent<Button>();
    }

    if (group == null)
    {
      group = GetComponent<CanvasGroup>();
    }
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
      parentAfterDrag = transform.parent;
      transform.SetParent(transform.root);
      transform.SetAsLastSibling();

      group.alpha = .5f;
  }

  public void OnDrag(PointerEventData eventData)
  {
      transform.position = Input.mousePosition;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
      transform.SetParent(parentAfterDrag);
      group.alpha = 1f;
  }

  public void SetDiceModifier(CharacterMenuHandler characterMenuHandler, DiceModifier diceModifier, int quantity)
  {
    this.diceModifier = diceModifier;
    this.characterMenuHandler = characterMenuHandler;
    this.canvasParent = characterMenuHandler.transform.parent;

    modifierNameText.text = diceModifier.name;
    modifierValueText.text = "x" + quantity.ToString();

    diceModifierButton.onClick.AddListener(() =>
    {
      EventManager.TriggerMenuButtonPressed();
      characterMenuHandler.UpdateModifierDescription(diceModifier);
    });
  }
  
  public DiceModifier GetDiceModifier()
  {
    return diceModifier;
  }
}
