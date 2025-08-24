using UnityEngine;
using TacticsToolkit;

public class AudioManager : MonoBehaviour
{
  [SerializeField] AudioSource audioSource;
  [SerializeField] AudioConfigProfile audioConfig;

  private void Awake()
  {
    EventManager.OnMenuButtonPressed += HandleMenuButtonPressed;
    EventManager.OnSuccessButtonPressed += HandleSuccessButtonPressed;
    EventManager.OnModifierApplied += HandleModifierApplied;
    EventManager.OnAttackBlocked += HandleAttackBlocked;
  }

  private void OnDisable()
  {
    EventManager.OnMenuButtonPressed -= HandleMenuButtonPressed;
    EventManager.OnSuccessButtonPressed -= HandleSuccessButtonPressed;
    EventManager.OnModifierApplied -= HandleModifierApplied;
    EventManager.OnAttackBlocked -= HandleAttackBlocked;
  }

  private void HandleMenuButtonPressed()
  {
    if (audioConfig.menuButtonPressed != null)
    {
      audioSource.PlayOneShot(audioConfig.menuButtonPressed);
    }
  }

  private void HandleSuccessButtonPressed()
  {
    if (audioConfig.successButtonPressed != null)
    {
      audioSource.PlayOneShot(audioConfig.successButtonPressed);
    }
  }

  private void HandleModifierApplied(DiceModifier modifier, Entity user)
  {
    if (audioConfig.modifierApplied != null && user is CharacterManager character)
    {
      audioSource.PlayOneShot(audioConfig.modifierApplied);
    }
  }

  private void HandleAttackBlocked()
  {
    if (audioConfig.attackedBlocked != null)
    {
      audioSource.PlayOneShot(audioConfig.attackedBlocked);
    }
  }
}