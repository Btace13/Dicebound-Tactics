using UnityEngine;

public class AudioManager : MonoBehaviour
{
  [SerializeField] AudioSource audioSource;
  [SerializeField] AudioConfigProfile audioConfig;

  private void Awake()
  {
    EventManager.OnMenuButtonPressed += HandleMenuButtonPressed;
    EventManager.OnSuccessButtonPressed += HandleSuccessButtonPressed;
  }

  private void OnDisable()
  {
    EventManager.OnMenuButtonPressed -= HandleMenuButtonPressed;
    EventManager.OnSuccessButtonPressed -= HandleSuccessButtonPressed;
  }

  private void HandleMenuButtonPressed()
  {
    audioSource.PlayOneShot(audioConfig.menuButtonPressed);
  }
  
  private void HandleSuccessButtonPressed()
  {
    audioSource.PlayOneShot(audioConfig.successButtonPressed);
  }
}