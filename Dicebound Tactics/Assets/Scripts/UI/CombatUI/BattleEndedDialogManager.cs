using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TacticsToolkit;

public class BattleEndedDialogManager : MonoBehaviour
{
  public static BattleEndedDialogManager Instance { get; private set; }
  [SerializeField] private GameObject dialogPanel;
  [SerializeField] private TextMeshProUGUI resultText;
  [SerializeField] private GameObject characterBattleCardContainer;
  [SerializeField] private GameObject battleCardPrefab;
  [SerializeField] private GameObject characterCards;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;
  }

  public void Show(bool playerWon)
  {
    if (dialogPanel == null || characterBattleCardContainer == null || battleCardPrefab == null)
    {
      Debug.LogError("Dialog panel or character battle card container or battle card prefab is not assigned.");
      return;
    }

    if(dialogPanel.activeSelf)
    {
      Debug.LogWarning("Dialog is already open.");
      return;
    }

    string resultMessage = playerWon ? "Victory!" : "Defeat!";

    TurnManager.Instance.playerUnits.ForEach(unit =>
    {
      GameObject card = Instantiate(battleCardPrefab, characterBattleCardContainer.transform);
      int expAdded = playerWon ? CalculateEXP(unit) / TurnManager.Instance.playerUnits.Count : 0;
      unit.IncreaseExp(expAdded);
      card.GetComponent<CharacterBattleCard>().SetCharacterInfo(unit, expAdded);
    });

    resultText.text = resultMessage;
    characterCards.SetActive(false);
    dialogPanel.SetActive(true);
  }

  public void CloseDialog()
  {
    if (dialogPanel != null)
      dialogPanel.SetActive(false);

    if (characterBattleCardContainer != null)
      characterCards.SetActive(true);

    foreach (Transform child in characterBattleCardContainer.transform)
    {
      Destroy(child.gameObject);
    }

  }

  public int CalculateEXP(Entity character)
  {
      int totalExp = 0;
      foreach (var enemy in TurnManager.Instance.enemyUnits)
      {
          float levelDiff = enemy.level - character.level;
          float multiplier = 1f + (levelDiff * 0.1f);
          multiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
          int baseExp = enemy.level * 10;
          totalExp += Mathf.RoundToInt(baseExp * multiplier);
      }
      return totalExp;
  }
}