using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCard : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Image characterPortrait;
  [SerializeField] private TextMeshProUGUI characterNameText;
  [SerializeField] private Image healthBar;
  [SerializeField] private TextMeshProUGUI healthText;
  [SerializeField] private TextMeshProUGUI apAmountText;
  [SerializeField] private TextMeshProUGUI modifierText;

  private string characterId;

  private void Update()
  {
    SetCharacterInfo(TurnManager.Instance.playerUnits.Find(c => c.characterId == characterId));
  }

  public void SetCharacterInfo(CharacterManager character)
  {
    if (character == null)
    {
      characterPortrait.sprite = null;
      characterNameText.text = string.Empty;
      healthBar.fillAmount = 0f;
      healthText.text = string.Empty;
      apAmountText.text = string.Empty;
      modifierText.text = string.Empty;
      return;
    }

    characterId = character.characterId;

    if (characterPortrait != null)
      characterPortrait.sprite = character.portrait;

    if (characterNameText != null)
      characterNameText.text = character.name;

    if (healthBar != null)
    {
      healthBar.fillAmount = (float)character.GetStat(Stats.CurrentHealth).statValue / character.GetStat(Stats.Health).statValue;
    }

    if (healthText != null)
      healthText.text = $"{character.GetStat(Stats.CurrentHealth).statValue} / {character.GetStat(Stats.Health).statValue}";

    if (apAmountText != null)
      apAmountText.text = character.GetStat(Stats.ActionPoints).statValue.ToString();

    if (modifierText != null)
      modifierText.text = character.equippedDice.LastRollModifier.Name;
  }
}