using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;

public class DiceModifierCardHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler
{
  [SerializeField] private TextMeshProUGUI modifierNameText;
  [SerializeField] private TextMeshProUGUI modifierValueText;
  [SerializeField] private Button diceModifierButton;
  [SerializeField] private CanvasGroup group;
  private CharacterMenuHandler characterMenuHandler;
  private CharacterMenuDiceCustomizationHandler characterMenuDiceCustomizationHandler;
  private DiceModifier diceModifier;
  public Transform parentAfterDrag;
  private Transform canvasParent;
  private ProceduralImage proceduralImage;
  private Color originalColor;

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

    if (proceduralImage == null)
    {
      proceduralImage = GetComponent<ProceduralImage>();
    }

    originalColor = proceduralImage.color;
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    if (characterMenuHandler != null)
    {
      characterMenuHandler.UpdateModifierDescription(diceModifier);
    }
  }

  public void OnBeginDrag(PointerEventData eventData)
  {
    // parentAfterDrag = transform.parent;
    // transform.SetParent(transform.root);
    // transform.SetAsLastSibling();

    // group.alpha = .5f;
  }

  public void OnDrag(PointerEventData eventData)
  {
    //transform.position = Input.mousePosition;
  }

  public void OnEndDrag(PointerEventData eventData)
  {
    // transform.SetParent(parentAfterDrag);
    // group.alpha = 1f;
  }

  public void SetDiceModifier(CharacterMenuDiceCustomizationHandler characterMenuDiceCustomizationHandler, CharacterMenuHandler characterMenuHandler, DiceModifier diceModifier, int quantity)
  {
    this.diceModifier = diceModifier;
    this.characterMenuHandler = characterMenuHandler;
    this.characterMenuDiceCustomizationHandler = characterMenuDiceCustomizationHandler;
    canvasParent = characterMenuHandler.transform.parent;

    modifierNameText.text = diceModifier.name;
    modifierValueText.text = "x" + quantity.ToString();

    diceModifierButton.onClick.AddListener(() =>
    {
      EventManager.TriggerMenuButtonPressed();
      transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack).OnComplete(() =>
      {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
      });

      if(characterMenuDiceCustomizationHandler != null)
      {
        if(characterMenuDiceCustomizationHandler.HasStagedDieSide())
        {
          characterMenuDiceCustomizationHandler.GetStagedDieSide().ApplyModifierToDiceSide(diceModifier);
        }
      }
    });
  }

  public DiceModifier GetDiceModifier()
  {
    return diceModifier;
  }
}
