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

  public void ToggleBackButton(bool isVisible)
  {
    if (backButton != null)
    {
      backButton.SetActive(isVisible);
    }
  }

  private void ShowCharacterMenu()
  {
    characterMenuUI.DOFade(1f, 0.2f).SetEase(Ease.InOutQuad).OnComplete(() =>
    {
      characterMenuUI.interactable = true;
      characterMenuUI.blocksRaycasts = true;
    });
  }

  private void HideCharacterMenu()
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
