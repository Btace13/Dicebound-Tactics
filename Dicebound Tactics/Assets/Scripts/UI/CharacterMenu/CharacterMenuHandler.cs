using UnityEngine;
using DG.Tweening;
using TacticsToolkit;

public class CharacterMenuHandler : MonoBehaviour
{
  [SerializeField] private CanvasGroup characterMenuUI;
  [SerializeField] private CanvasGroup characterSelectorUI;
  [SerializeField] private CanvasGroup characterScreenUI;
  [SerializeField] private CharacterMenuDiceCustomizationHandler charactersHandler;
  [SerializeField] private GameObject backButton;
  [SerializeField] private GameObject closeMenuButton
  ;

  private void Awake()
  {
    EventManager.OnCharacterMenuOpened += ShowCharacterMenu;
    EventManager.OnCharacterMenuClosed += HideCharacterMenu;
  }

  private void OnDestroy()
  {
    EventManager.OnCharacterMenuOpened -= ShowCharacterMenu;
    EventManager.OnCharacterMenuClosed -= HideCharacterMenu;
  }

  private void Update() {
    if (Input.GetKeyDown(KeyCode.M))
    {
      if(characterMenuUI.alpha == 0f)
      {
        ShowCharacterMenu();
      }
      else
      {
        HideCharacterMenu();
      }
    }
  }

  public void ToggleBackButton(bool isVisible)
  {
    if (backButton != null)
    {
      backButton.SetActive(isVisible);
    }
  }

  public void ToggleCloseMenuButton(bool isVisible)
  {
    if (closeMenuButton != null)
    {
      closeMenuButton.SetActive(isVisible);
    }
  }

  public void ShowCharacterMenu()
  {
    characterMenuUI.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterMenuUI.interactable = true;
      characterMenuUI.blocksRaycasts = true;
    });
  }

  public void HideCharacterMenu()
  {
    characterMenuUI.DOFade(0f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterMenuUI.interactable = false;
      characterMenuUI.blocksRaycasts = false;
    });
  }

  public void OpenCharacterSelector()
  {
    characterSelectorUI.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterSelectorUI.interactable = true;
      characterSelectorUI.blocksRaycasts = true;
    });

    ToggleBackButton(false);
    ToggleCloseMenuButton(true);
  }

  public void CloseCharacterSelector()
  {
    characterSelectorUI.DOFade(0f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterSelectorUI.interactable = false;
      characterSelectorUI.blocksRaycasts = false;
    });
  }

  public void OpenCharacterScreen(CharacterManager character)
  {
    charactersHandler.SetCharacter(character);
    characterScreenUI.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterScreenUI.interactable = true;
      characterScreenUI.blocksRaycasts = true;
    });

    ToggleBackButton(true);
    ToggleCloseMenuButton(false);
  }
  
  public void CloseCharacterScreen()
  {
    characterScreenUI.DOFade(0f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterScreenUI.interactable = false;
      characterScreenUI.blocksRaycasts = false;
    });
  }
}
