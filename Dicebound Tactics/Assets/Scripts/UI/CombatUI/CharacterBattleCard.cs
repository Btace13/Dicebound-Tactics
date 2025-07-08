using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterBattleCard : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private Image characterPortrait;
  [SerializeField] private TextMeshProUGUI characterNameText;
  [SerializeField] private Image healthBar;
  [SerializeField] private Image expBar;
  [SerializeField] private TextMeshProUGUI healthText;
  [SerializeField] private TextMeshProUGUI expText;
  [SerializeField] private TextMeshProUGUI expAddedText;
  [SerializeField] private TextMeshProUGUI levelText;

  public void SetCharacterInfo(CharacterManager character, int expAdded = 0)
  {
    if (character == null)
    {
      characterPortrait.sprite = null;
      characterNameText.text = string.Empty;
      healthBar.fillAmount = 0f;
      healthText.text = string.Empty;
      expBar.fillAmount = 0f;
      expText.text = string.Empty;
      expAddedText.text = string.Empty;
      levelText.text = string.Empty;
      return;
    }

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

    if (expBar != null)
      expBar.fillAmount = (float)character.experience / character.requiredExperience;

    if (expText != null)
      expText.text = $"{character.experience} / {character.requiredExperience}";

    if (expAddedText != null)
      expAddedText.text = $"{expAdded}";

    if (levelText != null)
      levelText.text = $"Lvl: {character.level}";
  }
}