using UnityEngine;
using DG.Tweening;
using TacticsToolkit;

[RequireComponent(typeof(ResourceBarUI))]
public class EnemyHealthBarUI : MonoBehaviour
{
  [SerializeField] private ModifierNotification modifierNotification;
  private ResourceBarUI resourceBarUI;
  private Entity enemy;

  private void Awake()
  {
    resourceBarUI = GetComponent<ResourceBarUI>();

    // Event Listeners
    EventManager.OnModifierApplied += ShowModifierNotification;
  }

  private void OnDestroy()
  {
    if (enemy != null)
    {
      enemy.OnCharacterStatChanged -= UpdateEnemyInfo;
    }

    EventManager.OnModifierApplied -= ShowModifierNotification;
  }

  public void SetEnemyInfo(Entity enemy)
  {
    if (this.enemy != null)
    {
      this.enemy.OnCharacterStatChanged -= UpdateEnemyInfo;
    }

    this.enemy = enemy;

    if (enemy == null)
    {
      resourceBarUI.ResourceName = string.Empty;
      resourceBarUI.MaxResource = 0;
      resourceBarUI.CurrentResource = 0;
      return;
    }

    this.enemy.OnCharacterStatChanged += UpdateEnemyInfo;

    resourceBarUI.ResourceName = enemy.gameObject.name;
    int max = enemy.GetStat(Stats.Health).statValue;
    int to = enemy.GetStat(Stats.CurrentHealth).statValue;
    int from = resourceBarUI.CurrentResource;
    resourceBarUI.MaxResource = max;
    DOTween.To(() => from, x =>
    {
      from = x;
      resourceBarUI.CurrentResource = from;
    }, to, 0.5f).SetEase(Ease.OutCubic);
  }

  private void UpdateEnemyInfo(Entity enemy)
  {
    SetEnemyInfo(enemy);
  }

  public Entity GetEnemy()
  {
    return enemy;
  }
  
  private void ShowModifierNotification(DiceModifier dice, Entity user)
  {
    if (modifierNotification == null || enemy == null || enemy != user)
      return;

    modifierNotification.ShowEnemyModifier(dice, user);
  }
}
