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
  }

  private void OnDisable()
  {
    EventManager.OnMenuButtonPressed -= HandleMenuButtonPressed;
    EventManager.OnSuccessButtonPressed -= HandleSuccessButtonPressed;
    EventManager.OnModifierApplied -= HandleModifierApplied;
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
}