using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class CharacterCard : MonoBehaviour
{
  public DieIconSet diceIconSet;
  [Header("UI References")]
  [SerializeField] private Image characterPortrait;
  [SerializeField] private TextMeshProUGUI characterNameText;
  [SerializeField] private TextMeshProUGUI lvlText;
  [SerializeField] private Slider healthBar;
  [SerializeField] private TextMeshProUGUI healthText;
  [SerializeField] private TextMeshProUGUI apAmountText;
  [SerializeField] private TextMeshProUGUI modifierText;
  [SerializeField] private GameObject CurrentTurnIndicator;
  [SerializeField] private TextMeshProUGUI modifierTextTitle;
  [SerializeField] private TextMeshProUGUI modifierTextContent;
  [SerializeField] private Image diceIconImage;
  [SerializeField] private GameObject apInfo;

  private Entity character;
  public float rollSpeed = 2f; // higher = faster
  private Coroutine rollingCoroutine;

  private void Awake()
  {
    // Event Listeners
    EventManager.OnCharacterTurnStarted += UpdateCurrentTurnIndicator;
    EventManager.OnCharacterTurnEnded += RemoveCurrentTurnIndicator;
    EventManager.OnBattleStarted += HandleBattleStarted;
    EventManager.OnBattleEnded += HandleBattleEnded;
  }

  void OnDestroy()
  {
    if (character != null)
    {
      character.OnCharacterStatChanged -= HandleUpdatingCharacterInfo;
      character.OnLevelChanged -= UpdateLevelText;
    }

    EventManager.OnCharacterTurnStarted -= UpdateCurrentTurnIndicator;
    EventManager.OnCharacterTurnEnded -= RemoveCurrentTurnIndicator;
    EventManager.OnBattleStarted -= HandleBattleStarted;
    EventManager.OnBattleEnded -= HandleBattleEnded;
  }

  private void HandleUpdatingCharacterInfo(Entity character)
  {
    SetCharacterInfo(character);
    
    if(CurrentTurnIndicator != null)
    {
      CurrentTurnIndicator.SetActive(CombatManager.Instance.TurnManager.GetCurrentUnit() == character);
    }
  }

  public void SetCharacterInfo(Entity character)
  {
    if (character == null)
    {
      characterPortrait.sprite = null;
      characterNameText.text = string.Empty;
      healthBar.value = 0f;
      healthText.text = string.Empty;
      apAmountText.text = string.Empty;
      modifierText.text = string.Empty;
      modifierTextTitle.text = string.Empty;
      modifierTextContent.text = string.Empty;
      lvlText.text = string.Empty;
      return;
    }

    this.character = character;

    // Event Listeners
    this.character.OnCharacterStatChanged += HandleUpdatingCharacterInfo;
    this.character.OnLevelChanged += UpdateLevelText;

    if (characterPortrait != null)
      characterPortrait.sprite = character.portrait;

    if (characterNameText != null)
      characterNameText.text = character.name;

    if (lvlText != null)
      lvlText.text = $"Lvl: {character.level}";

    if (healthBar != null)
    {
      healthBar.maxValue = character.GetStat(Stats.Health).statValue;
      healthBar.DOValue(character.GetStat(Stats.CurrentHealth).statValue, 0.5f).SetEase(Ease.OutCubic);
    }

    if (healthText != null)
    {
      int from = 0;
      if (!string.IsNullOrEmpty(healthText.text) && healthText.text.Contains("/"))
      {
        int.TryParse(healthText.text.Split('/')[0].Trim(), out from);
      }
      int to = character.GetStat(Stats.CurrentHealth).statValue;
      int max = character.GetStat(Stats.Health).statValue;

      // Animate health number
      int currentHealth = from;
      DOTween.To(() => currentHealth, x =>
      {
        currentHealth = x;
        healthText.text = $"{currentHealth} / {max}";
      }, to, 0.5f).SetEase(Ease.OutCubic);
    }

    if (apAmountText != null)
      SetNumber(character.GetStat(Stats.ActionPoints).statValue, apAmountText);

    if (modifierText != null)
    {
      if (!TurnManager.Instance.BattlePlaying)
        modifierText.text = "";

      if (character.equippedDice == null || character.equippedDice.LastRollModifier == null)
        modifierText.text = "";
      else
        modifierText.text = character.equippedDice.LastRollModifier.Name;
    }

    if (modifierTextTitle != null)
      if (character.equippedDice == null || character.equippedDice.LastRollModifier == null)
        modifierTextTitle.text = "";
      else
        modifierTextTitle.text = character.equippedDice.LastRollModifier.Name;

    if (modifierTextContent != null)
      if (character.equippedDice == null || character.equippedDice.LastRollModifier == null)
        modifierTextContent.text = "";
      else
        modifierTextContent.text = character.equippedDice.LastRollModifier.Description;

    if (diceIconImage != null && diceIconSet != null && TurnManager.Instance.GetCurrentUnit() == character)
    {
      Sprite icon = diceIconSet.GetIconForValue(character.equippedDice?.LastRollValue ?? 1);
      if (icon != null)
      {
        diceIconImage.sprite = icon;
        diceIconImage.gameObject.SetActive(true);
      }
      else
      {
        diceIconImage.gameObject.SetActive(false);
      }
    }
  }

  private void UpdateLevelText()
  {
    if (lvlText != null)
    {
      lvlText.text = $"Lvl: {character.level}";
    }
  }

  private void UpdateCurrentTurnIndicator(Entity c)
  {
    if (CurrentTurnIndicator != null)
    {
      CurrentTurnIndicator.SetActive(CombatManager.Instance.TurnManager.GetCurrentUnit() == character);
    }
  }

  public void SetNumber(int newValue, TextMeshProUGUI numberText)
  {
    // Stop previous animation if any
    if (rollingCoroutine != null)
      StopCoroutine(rollingCoroutine);

    int currentValue = int.Parse(numberText.text);
    rollingCoroutine = StartCoroutine(RollToValue(currentValue, newValue, numberText));
  }

  private IEnumerator RollToValue(int from, int to, TextMeshProUGUI numberText)
  {
    float duration = Mathf.Abs(to - from) / rollSpeed;
    float elapsed = 0f;
    float minFontSize = numberText.fontSize;
    float maxFontSize = numberText.fontSize * 2f;

    while (elapsed < duration)
    {
      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / duration);
      int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
      numberText.text = value.ToString();
      numberText.DOFontSize(maxFontSize, duration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
              numberText.DOFontSize(minFontSize, duration * 0.5f)
                  .SetEase(Ease.InQuad);
            });

      yield return null;
    }

    numberText.text = to.ToString();
    rollingCoroutine = null;
  }

  private void RemoveCurrentTurnIndicator(CharacterManager character)
  {
    if (CurrentTurnIndicator != null && character == CombatManager.Instance.TurnManager.GetCurrentUnit())
    {
      CurrentTurnIndicator.SetActive(false);
    }
  }

  private void HandleBattleStarted()
  { 
    if (apInfo != null)
    {
      apInfo.SetActive(true);
    }
  }

  private void HandleBattleEnded()
  {
    // Hide the current turn indicator when the battle ends
    if (CurrentTurnIndicator != null)
    {
      CurrentTurnIndicator.SetActive(false);
    }

    if (diceIconImage != null)
    {
      diceIconImage.gameObject.SetActive(false);
    }

    if (apAmountText != null)
    {
      apAmountText.text = string.Empty;
    }

    if (apInfo != null)
    {
      apInfo.SetActive(false);
    }
  }
}