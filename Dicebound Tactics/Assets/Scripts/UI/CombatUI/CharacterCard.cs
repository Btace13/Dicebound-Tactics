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

  public void SetCharacterInfo(Entity character)
  {
    if (characterPortrait != null)
      characterPortrait.sprite = character.portrait;

    if (characterNameText != null)
      characterNameText.text = character.name;

    if (healthBar != null)
    {
      healthBar.fillAmount = character.GetStat(Stats.CurrentHealth).statValue / character.GetStat(Stats.Health).statValue;
    }

    if (healthText != null)
      healthText.text = $"{character.GetStat(Stats.CurrentHealth).statValue} / {character.GetStat(Stats.Health).statValue}";

    if (apAmountText != null)
      apAmountText.text = character.GetStat(Stats.ActionPoints).statValue.ToString();
  }
}