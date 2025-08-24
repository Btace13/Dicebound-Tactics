using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TacticsToolkit;

public class DefensiveTimingUI : MonoBehaviour
{
  [Header("UI Elements")]
  [SerializeField] private GameObject defensivePromptPanel;
  [SerializeField] private TextMeshProUGUI instructionText;
  [SerializeField] private Image timerFillBar;
  [SerializeField] private Transform buttonPromptContainer;
  [SerializeField] private GameObject buttonPromptPrefab;
  
  [Header("Button Icons")]
  [SerializeField] private Sprite leftGamepadIcon;
  [SerializeField] private Sprite rightGamepadIcon;
  [SerializeField] private Sprite topGamepadIcon;
  [SerializeField] private Sprite bottomGamepadIcon;
  
  [Header("Visual Feedback")]
  [SerializeField] private Color successColor = Color.green;
  [SerializeField] private Color failureColor = Color.red;
  [SerializeField] private Color neutralColor = Color.white;
  [SerializeField] private Color correctButtonColor = Color.yellow;

  private List<GameObject> currentButtonPrompts = new List<GameObject>();
  private Coroutine activeTimingCoroutine;
  
  public static DefensiveTimingUI Instance { get; private set; }
  
  private void Awake()
  {
      if (Instance == null)
      {
          Instance = this;
          DontDestroyOnLoad(gameObject);
      }
      else
      {
          Destroy(gameObject);
      }
      
      // Hide the panel initially
      if (defensivePromptPanel != null)
      {
          defensivePromptPanel.SetActive(false);
      }
  }

  private void OnEnable()
  {
      // Subscribe to defensive timing events
      EventManager.OnDefensivePromptRequested += ShowDefensivePrompt;
      EventManager.OnDefensivePromptHidden += HideDefensivePrompt;
  }

  private void OnDisable()
  {
      // Unsubscribe from events
      EventManager.OnDefensivePromptRequested -= ShowDefensivePrompt;
      EventManager.OnDefensivePromptHidden -= HideDefensivePrompt;
  }
  
  /// <summary>
  /// Starts the defensive timing UI sequence
  /// </summary>
  public void ShowDefensivePrompt(Entity target, string[] buttonSequence, float timeLimit, System.Action<bool> onComplete)
  {
      if (activeTimingCoroutine != null)
      {
          StopCoroutine(activeTimingCoroutine);
      }
      
      activeTimingCoroutine = StartCoroutine(DefensiveTimingSequence(target, buttonSequence, timeLimit, onComplete));
  }
  
  /// <summary>
  /// Hides the defensive timing UI
  /// </summary>
  public void HideDefensivePrompt()
  {
      if (activeTimingCoroutine != null)
      {
          StopCoroutine(activeTimingCoroutine);
          activeTimingCoroutine = null;
      }
      
      if (defensivePromptPanel != null)
      {
          defensivePromptPanel.SetActive(false);
      }
      
      ClearButtonPrompts();
  }

  private IEnumerator DefensiveTimingSequence(Entity target, string[] buttonSequence, float timeLimit, System.Action<bool> onComplete)
  {
      // Show the panel
      defensivePromptPanel.SetActive(true);
      
      // Setup initial state
      SetupButtonPrompts(buttonSequence);
      UpdateInstructionText("Press the button sequence!");
      
      bool sequenceCompleted = false;
      int currentButtonIndex = 0;
      float timeRemaining = timeLimit;
      
      InputSystem_Actions inputActions = new InputSystem_Actions();
      inputActions.Enable();
      
      // Animate panel entrance
      defensivePromptPanel.transform.localScale = Vector3.zero;
      defensivePromptPanel.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
      
      while (timeRemaining > 0 && !sequenceCompleted)
      {
          timeRemaining -= Time.deltaTime;
          
          // Update timer bar
          UpdateTimerBar(timeRemaining / timeLimit);
          
          // Check for input
          if (currentButtonIndex < buttonSequence.Length)
          {
              string expectedButton = buttonSequence[currentButtonIndex];
              
              if (WasButtonPressed(inputActions, expectedButton))
              {
                  // Correct button pressed
                  EventManager.TriggerDefensiveButtonPressed(expectedButton);
                  MarkButtonAsCorrect(currentButtonIndex);
                  currentButtonIndex++;
                  
                  // Update instruction
                  if (currentButtonIndex >= buttonSequence.Length)
                  {
                      sequenceCompleted = true;
                      UpdateInstructionText("Success!");
                      target.PlayBlockVFX();
                      EventManager.TriggerAttackBlocked();
                      ShowSuccessFeedback();
                      EventManager.TriggerDefensiveSequenceCompleted();
                  }
                  else
                  {
                      UpdateInstructionText($"Good! Press next button ({currentButtonIndex + 1}/{buttonSequence.Length})");
                  }
              }
              else if (AnyUnexpectedButtonPressed(inputActions, expectedButton))
              {
                  // Wrong button pressed
                  ResetButtonPrompts();
                  currentButtonIndex = 0;
                  UpdateInstructionText("Wrong button! Try again!");
                  ShowFailureFeedback();
              }
          }
          
          yield return null;
      }
      
      inputActions.Disable();
      inputActions.Dispose();
      
      // Show final result
      if (sequenceCompleted)
      {
          UpdateInstructionText("Perfect Defense!");
          ShowSuccessFeedback();
      }
      else
      {
          UpdateInstructionText("Time's up!");
          ShowFailureFeedback();
          EventManager.TriggerDefensiveSequenceFailed();
      }
      
      // Wait a moment before hiding
      yield return new WaitForSeconds(1f);
      
      // Animate panel exit
      defensivePromptPanel.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).OnComplete(() =>
      {
          defensivePromptPanel.SetActive(false);
          EventManager.TriggerDefensivePromptHidden();
      });
      
      // Call completion callback
      onComplete?.Invoke(sequenceCompleted);
      
      activeTimingCoroutine = null;
  }
  
  private void SetupButtonPrompts(string[] buttonSequence)
  {
    ClearButtonPrompts();
    
    if (buttonPromptPrefab == null)
    {
        Debug.LogError("DefensiveTimingUI: buttonPromptPrefab is not assigned!");
        return;
    }
    
    if (buttonPromptContainer == null)
    {
        Debug.LogError("DefensiveTimingUI: buttonPromptContainer is not assigned!");
        return;
    }
    
    foreach (string buttonName in buttonSequence)
    {
      GameObject prompt = Instantiate(buttonPromptPrefab, buttonPromptContainer);
      

      Sprite buttonSprite = GetButtonIcon(buttonName);
      if (buttonSprite != null)
      {
          prompt.GetComponent<ButtonPrompt>()?.SetButtonIcon(buttonSprite);
          Debug.Log($"DefensiveTimingUI: Set sprite for button {buttonName}");
      }
      else
      {
          Debug.LogWarning($"DefensiveTimingUI: No sprite found for button {buttonName}!");
      }
        
      currentButtonPrompts.Add(prompt);
    }
    
    Debug.Log($"DefensiveTimingUI: Created {currentButtonPrompts.Count} button prompts for sequence: {string.Join(", ", buttonSequence)}");
  }
  
  private void ClearButtonPrompts()
  {
      Debug.Log($"DefensiveTimingUI: Clearing {currentButtonPrompts.Count} button prompts");
      
      foreach (GameObject prompt in currentButtonPrompts)
      {
          if (prompt != null)
          {
              DestroyImmediate(prompt);
          }
      }
      currentButtonPrompts.Clear();
      
      // Also clear any remaining children in the container as a safety measure
      if (buttonPromptContainer != null)
      {
          for (int i = buttonPromptContainer.childCount - 1; i >= 0; i--)
          {
              DestroyImmediate(buttonPromptContainer.GetChild(i).gameObject);
          }
      }
  }
  
  private void MarkButtonAsCorrect(int index)
  {
      if (index < currentButtonPrompts.Count)
      {
          GameObject buttonToRemove = currentButtonPrompts[index];
          if (buttonToRemove != null)
          {
              // Animate the button before removing it
              buttonToRemove.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f).OnComplete(() =>
              {
                  buttonToRemove.GetComponent<ButtonPrompt>()?.SetColors(correctButtonColor, Color.white);
              });
              
              // Remove from our list immediately to prevent accessing it again
              currentButtonPrompts[index] = null;
              Debug.Log($"DefensiveTimingUI: Marked button {index} for removal");
          }
      }
  }
  
  private void ResetButtonPrompts()
  {
      foreach (GameObject prompt in currentButtonPrompts)
      {
          if (prompt != null) // Check for null since some buttons might have been removed
          {
              Image buttonImage = prompt.GetComponent<Image>();
              if (buttonImage != null)
              {
                  buttonImage.color = neutralColor;
              }
          }
      }
  }
  
  private void UpdateInstructionText(string text)
  {
      if (instructionText != null)
      {
          instructionText.text = text;
      }
  }
  
  private void UpdateTimerBar(float fillAmount)
  {
      if (timerFillBar != null)
      {
          timerFillBar.fillAmount = fillAmount;
          
          // Change color based on remaining time
          if (fillAmount > 0.5f)
              timerFillBar.color = Color.green;
          else if (fillAmount > 0.25f)
              timerFillBar.color = Color.yellow;
          else
              timerFillBar.color = Color.red;
      }
  }

  private void ShowSuccessFeedback()
  {
    if (defensivePromptPanel != null)
    {
      defensivePromptPanel.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
    }

    // Could add particle effects, screen flash, etc.
  }
  
  private void ShowFailureFeedback()
  {
      if (defensivePromptPanel != null)
      {
          defensivePromptPanel.transform.DOShakePosition(0.3f, 10f);
      }
  }
  
  private Sprite GetButtonIcon(string buttonName)
  {
      Debug.Log($"DefensiveTimingUI: Getting icon for button: '{buttonName}'");
      
      switch (buttonName)
      {
          case "LeftGamepad":
              if (leftGamepadIcon == null) Debug.LogWarning("DefensiveTimingUI: leftGamepadIcon is not assigned!");
              return leftGamepadIcon;
          case "RightGamepad":
              if (rightGamepadIcon == null) Debug.LogWarning("DefensiveTimingUI: rightGamepadIcon is not assigned!");
              return rightGamepadIcon;
          case "TopGamepad":
              if (topGamepadIcon == null) Debug.LogWarning("DefensiveTimingUI: topGamepadIcon is not assigned!");
              return topGamepadIcon;
          case "BottomGamepad":
              if (bottomGamepadIcon == null) Debug.LogWarning("DefensiveTimingUI: bottomGamepadIcon is not assigned!");
              return bottomGamepadIcon;
          default:
              Debug.LogWarning($"DefensiveTimingUI: Unknown button name: '{buttonName}'");
              return null;
      }
  }
  
  private bool WasButtonPressed(InputSystem_Actions inputActions, string buttonName)
  {
      switch (buttonName)
      {
          case "LeftGamepad":
              return inputActions.Player.LeftGamepad.WasPressedThisFrame();
          case "RightGamepad":
              return inputActions.Player.RightGamepad.WasPressedThisFrame();
          case "TopGamepad":
              return inputActions.Player.TopGamepad.WasPressedThisFrame();
          case "BottomGamepad":
              return inputActions.Player.BottomGamepad.WasPressedThisFrame();
          default:
              return false;
      }
  }
  
  private bool AnyUnexpectedButtonPressed(InputSystem_Actions inputActions, string expectedButton)
  {
      bool leftPressed = inputActions.Player.LeftGamepad.WasPressedThisFrame();
      bool rightPressed = inputActions.Player.RightGamepad.WasPressedThisFrame();
      bool topPressed = inputActions.Player.TopGamepad.WasPressedThisFrame();
      bool bottomPressed = inputActions.Player.BottomGamepad.WasPressedThisFrame();

      switch (expectedButton)
      {
          case "LeftGamepad":
              return rightPressed || topPressed || bottomPressed;
          case "RightGamepad":
              return leftPressed || topPressed || bottomPressed;
          case "TopGamepad":
              return leftPressed || rightPressed || bottomPressed;
          case "BottomGamepad":
              return leftPressed || rightPressed || topPressed;
          default:
              return leftPressed || rightPressed || topPressed || bottomPressed;
      }
    }
}
