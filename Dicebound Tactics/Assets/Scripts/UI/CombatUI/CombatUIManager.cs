using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TacticsToolkit;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Unified Combat UI Manager with State Machine for Panel Management
/// Combines CombatUIHandler and CombatUIPhaseController functionality
/// </summary>
public class CombatUIManager : MonoBehaviour
{
    #region UI State Machine

    public enum UIState
    {
        Hidden,           // Combat UI is completely hidden
        BattleStart,      // Combat UI visible, action panel open
        PlayerTurn,       // Player's turn, action panel visible
        AbilitySelection, // Ability panel open
        ItemSelection,    // Item panel open
        TargetSelection,  // Target selection mode active
        EnemyTurn,        // Enemy turn, UI mostly hidden
        BattleEnd         // Battle ended, transitioning to hidden
    }

    public enum UITransition
    {
        ShowCombatUI,
        HideCombatUI,
        StartPlayerTurn,
        OpenAbilityPanel,
        OpenItemPanel,
        StartTargetSelection,
        EndTargetSelection,
        StartEnemyTurn,
        EndEnemyTurn,
        GoBack,
        PassTurn,
        EndBattle
    }

    [Header("State Machine Debug")]
    [SerializeField] private UIState currentState = UIState.Hidden;
    public bool debugStateChanges = false;

    private Dictionary<(UIState, UITransition), UIState> stateTransitions;
    private Dictionary<UIState, Action> stateActions;

    #endregion

    #region UI Configuration

    [Header("Animation Settings")]
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector3 _canvasOffset = new Vector3(0.25f, 0.5f, 0);
    [SerializeField] private Ease _defaultEase = Ease.InOutQuad;

    [Header("Camera Settings")]
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private UDictionary<CombatPanel, string> PanelCameras = new UDictionary<CombatPanel, string>();

    [Header("Panel References")]
    public ActionPanel ActionPanel;
    public AbilityPanel AbilityPanel;
    public ItemPanel ItemPanel;

    [Header("Screen Space UI References")]
    [SerializeField] private CanvasGroup screenSpaceCanvasGroup;
    [SerializeField] private CombatNotification notificationUI;
    [SerializeField] private CanvasGroup panelInputsCanvasGroup;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private CanvasGroup targetSelectionCanvasGroup;

    [Header("Other References")]
    public DamageNumberUIHandler damageNumberUIHandler;

    #endregion

    #region Private Fields

    private CombatPanel currentPanel;
    private CharacterManager currentCharacter;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeStateMachine();
        InitializePanels();
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region State Machine Initialization

    private void InitializeStateMachine()
    {
        // Define state transitions
        stateTransitions = new Dictionary<(UIState, UITransition), UIState>
        {
            // From Hidden
            { (UIState.Hidden, UITransition.ShowCombatUI), UIState.BattleStart },
            
            // From BattleStart
            { (UIState.BattleStart, UITransition.StartPlayerTurn), UIState.PlayerTurn },
            { (UIState.BattleStart, UITransition.StartEnemyTurn), UIState.EnemyTurn },
            { (UIState.BattleStart, UITransition.EndBattle), UIState.BattleEnd },
            
            // From PlayerTurn
            { (UIState.PlayerTurn, UITransition.OpenAbilityPanel), UIState.AbilitySelection },
            { (UIState.PlayerTurn, UITransition.OpenItemPanel), UIState.ItemSelection },
            { (UIState.PlayerTurn, UITransition.StartTargetSelection), UIState.TargetSelection },
            { (UIState.PlayerTurn, UITransition.StartEnemyTurn), UIState.EnemyTurn },
            { (UIState.PlayerTurn, UITransition.PassTurn), UIState.EnemyTurn },
            { (UIState.PlayerTurn, UITransition.EndBattle), UIState.BattleEnd },
            
            // From AbilitySelection
            { (UIState.AbilitySelection, UITransition.GoBack), UIState.PlayerTurn },
            { (UIState.AbilitySelection, UITransition.StartTargetSelection), UIState.TargetSelection },
            { (UIState.AbilitySelection, UITransition.EndBattle), UIState.BattleEnd },
            
            // From ItemSelection
            { (UIState.ItemSelection, UITransition.GoBack), UIState.PlayerTurn },
            { (UIState.ItemSelection, UITransition.StartTargetSelection), UIState.TargetSelection },
            { (UIState.ItemSelection, UITransition.EndBattle), UIState.BattleEnd },
            
            // From TargetSelection
            { (UIState.TargetSelection, UITransition.EndTargetSelection), UIState.PlayerTurn },
            { (UIState.TargetSelection, UITransition.GoBack), UIState.PlayerTurn },
            { (UIState.TargetSelection, UITransition.EndBattle), UIState.BattleEnd },
            
            // From EnemyTurn
            { (UIState.EnemyTurn, UITransition.StartPlayerTurn), UIState.PlayerTurn },
            { (UIState.EnemyTurn, UITransition.EndBattle), UIState.BattleEnd },
            
            // From BattleEnd
            { (UIState.BattleEnd, UITransition.HideCombatUI), UIState.Hidden }
        };

        // Define state entry actions
        stateActions = new Dictionary<UIState, Action>
        {
            { UIState.Hidden, OnEnterHiddenState },
            { UIState.BattleStart, OnEnterBattleStartState },
            { UIState.PlayerTurn, OnEnterPlayerTurnState },
            { UIState.AbilitySelection, OnEnterAbilitySelectionState },
            { UIState.ItemSelection, OnEnterItemSelectionState },
            { UIState.TargetSelection, OnEnterTargetSelectionState },
            { UIState.EnemyTurn, OnEnterEnemyTurnState },
            { UIState.BattleEnd, OnEnterBattleEndState }
        };
    }

    #endregion

    #region State Machine Core

    public bool TryTransition(UITransition transition)
    {
        if (stateTransitions.TryGetValue((currentState, transition), out UIState newState))
        {
            TransitionToState(newState);
            return true;
        }

        if (debugStateChanges)
        {
            Debug.LogWarning($"[CombatUIManager] Invalid transition '{transition}' from state '{currentState}'");
            Debug.Log($"[CombatUIManager] Available transitions from {currentState}:");
            foreach (var kvp in stateTransitions)
            {
                if (kvp.Key.Item1 == currentState)
                {
                    Debug.Log($"  - {kvp.Key.Item2} -> {kvp.Value}");
                }
            }
        }
        return false;
    }

    private void TransitionToState(UIState newState)
    {
        currentState = newState;

        // Execute state entry action
        if (stateActions.TryGetValue(newState, out Action action))
        {
            action.Invoke();
        }
    }

    public UIState GetCurrentState() => currentState;

    /// <summary>
    /// Enable or disable debug logging for state transitions
    /// </summary>
    public void SetDebugMode(bool enabled)
    {
        debugStateChanges = enabled;
        if (enabled)
        {
            Debug.Log($"[CombatUIManager] Debug mode enabled. Current state: {currentState}");
        }
    }

    /// <summary>
    /// Get detailed debug information about the current state
    /// </summary>
    public void LogCurrentStateInfo()
    {
        Debug.Log($"[CombatUIManager] Current State: {currentState}");
        Debug.Log($"[CombatUIManager] Current Character: {currentCharacter?.name ?? "NULL"}");
        Debug.Log($"[CombatUIManager] Current Panel: {currentPanel?.name ?? "NULL"}");
        
        Debug.Log($"[CombatUIManager] Available transitions from {currentState}:");
        foreach (var kvp in stateTransitions)
        {
            if (kvp.Key.Item1 == currentState)
            {
                Debug.Log($"  - {kvp.Key.Item2} -> {kvp.Value}");
            }
        }
    }

    #endregion

    #region State Entry Actions

    private void OnEnterHiddenState()
    {
        FadeCanvasGroup(screenSpaceCanvasGroup, false);
        CloseAllPanelsImmediate();
    }

    private void OnEnterBattleStartState()
    {
        FadeCanvasGroup(screenSpaceCanvasGroup, true);
        // Action panel will be opened when character turn starts, not at battle start
    }

    private void OnEnterPlayerTurnState()
    {
        Debug.Log("[CombatUIManager] Entering PlayerTurn state - opening ActionPanel");
        OpenPanel(ActionPanel);
        if (currentCharacter != null)
        {
            Debug.Log($"[CombatUIManager] Moving canvas to character: {currentCharacter.name}");
            MoveCanvasToCharacter(currentCharacter);
        }
        ShowConfirmButton(false);
        ToggleBackButtonInteractable(false);
    }

    private void OnEnterAbilitySelectionState()
    {
        OpenPanel(AbilityPanel);
        ToggleBackButtonInteractable(true);
        ShowConfirmButton(false);
    }

    private void OnEnterItemSelectionState()
    {
        OpenPanel(ItemPanel);
        ToggleBackButtonInteractable(true);
        ShowConfirmButton(false);
    }

    private void OnEnterTargetSelectionState()
    {
        ShowConfirmButton(true);
        ShowTargetSelectionUI(true);
        ToggleBackButtonInteractable(true);
    }

    private void OnEnterEnemyTurnState()
    {
        CloseAllPanels();
        ShowScreenSpacePanelInputs(false);
        ShowConfirmButton(false);
        ToggleBackButtonInteractable(false);
    }

    private void OnEnterBattleEndState()
    {
        CloseAllPanels();
        ShowConfirmButton(false);
        ToggleBackButtonInteractable(false);
    }

    #endregion

    #region Event Management

    private void SubscribeToEvents()
    {
        // Combat flow events
        EventManager.OnBattleStarted += OnBattleStarted;
        EventManager.OnBattleEnded += OnBattleEnded;
        EventManager.OnCharacterTurnStarted += OnCharacterTurnStarted;
        EventManager.OnCharacterTurnEnded += OnCharacterTurnEnded;
        EventManager.OnEnemyTurnStarted += OnEnemyTurnStarted;
        EventManager.OnEnemyTurnEnded += OnEnemyTurnEnded;
        EventManager.OnSelectingATarget += OnSelectingATarget;
        EventManager.OnPassTurn += OnPassTurn;
        EventManager.OnBackButtonPressed += OnBackButtonPressed;

        // Panel show requests
        EventManager.OnShowActionPanel += HandleShowActionPanel;
        EventManager.OnShowAbilityPanel += HandleShowAbilityPanel;
        EventManager.OnShowItemPanel += HandleShowItemPanel;
    }

    private void UnsubscribeFromEvents()
    {
        // Combat flow events
        EventManager.OnBattleStarted -= OnBattleStarted;
        EventManager.OnBattleEnded -= OnBattleEnded;
        EventManager.OnCharacterTurnStarted -= OnCharacterTurnStarted;
        EventManager.OnCharacterTurnEnded -= OnCharacterTurnEnded;
        EventManager.OnEnemyTurnStarted -= OnEnemyTurnStarted;
        EventManager.OnEnemyTurnEnded -= OnEnemyTurnEnded;
        EventManager.OnSelectingATarget -= OnSelectingATarget;
        EventManager.OnPassTurn -= OnPassTurn;
        EventManager.OnBackButtonPressed -= OnBackButtonPressed;

        // Panel show requests
        EventManager.OnShowActionPanel -= HandleShowActionPanel;
        EventManager.OnShowAbilityPanel -= HandleShowAbilityPanel;
        EventManager.OnShowItemPanel -= HandleShowItemPanel;
    }

    #endregion

    #region Event Handlers

    private void OnBattleStarted()
    {
        TryTransition(UITransition.ShowCombatUI);
    }

    private void OnBattleEnded()
    {
        TryTransition(UITransition.EndBattle);
        TryTransition(UITransition.HideCombatUI);
    }

    private void OnCharacterTurnStarted(CharacterManager character)
    {
        Debug.Log($"[CombatUIManager] OnCharacterTurnStarted called for {character.name}, current state: {currentState}");
        Debug.Log($"[CombatUIManager] Previous currentCharacter: {currentCharacter?.name ?? "NULL"}");
        currentCharacter = character;
        Debug.Log($"[CombatUIManager] New currentCharacter: {currentCharacter?.name ?? "NULL"}");
        
        // Force transition to PlayerTurn state or directly call OnEnterPlayerTurnState if already in PlayerTurn
        bool transitionSucceeded = TryTransition(UITransition.StartPlayerTurn);
        Debug.Log($"[CombatUIManager] StartPlayerTurn transition succeeded: {transitionSucceeded}, new state: {currentState}");
        
        if (!transitionSucceeded && currentState == UIState.PlayerTurn)
        {
            Debug.Log("[CombatUIManager] Already in PlayerTurn state, manually calling OnEnterPlayerTurnState");
            OnEnterPlayerTurnState();
        }
    }

    private void OnCharacterTurnEnded(CharacterManager _)
    {
        // Transition will be handled by enemy turn start or battle end
    }

    private void OnEnemyTurnStarted(EnemyManager _)
    {
        TryTransition(UITransition.StartEnemyTurn);
    }

    private void OnEnemyTurnEnded(EnemyManager _)
    {
        // Next transition will be handled by character turn start
    }

    private void OnSelectingATarget(bool isSelecting)
    {
        if (isSelecting)
        {
            TryTransition(UITransition.StartTargetSelection);
        }
        else
        {
            TryTransition(UITransition.EndTargetSelection);
        }
    }

    private void OnPassTurn()
    {
        Debug.Log($"[CombatUIManager] Pass turn requested for current character: {currentCharacter?.name ?? "NULL"}");
        
        if (currentCharacter != null)
        {
            // End the current character's turn - this will trigger the next turn automatically
            EventManager.TriggerCharacterTurnEnded(currentCharacter);
        }
        else
        {
            Debug.LogWarning("[CombatUIManager] Cannot pass turn - no current character");
        }
        
        // Transition UI to indicate turn is being passed
        TryTransition(UITransition.PassTurn);
    }

    private void OnBackButtonPressed()
    {
        if (currentState == UIState.TargetSelection)
        {
            ShowTargetSelectionUI(false);
            SelectionController.Instance.ClearAllSelections();
            EventManager.TriggerSelectingATarget(false);
        }
        
        TryTransition(UITransition.GoBack);
    }

    private void HandleShowActionPanel()
    {
        if (currentState == UIState.AbilitySelection || currentState == UIState.ItemSelection)
        {
            TryTransition(UITransition.GoBack);
        }
        else if (currentState == UIState.TargetSelection)
        {
            TryTransition(UITransition.EndTargetSelection);
        }
        else if (currentState == UIState.PlayerTurn)
        {
            // Already in the correct state, just make sure the action panel is visible
            OnEnterPlayerTurnState();
        }
        else
        {
            // Force transition to PlayerTurn state
            TransitionToState(UIState.PlayerTurn);
        }
    }

    private void HandleShowAbilityPanel()
    {
        TryTransition(UITransition.OpenAbilityPanel);
    }

    private void HandleShowItemPanel()
    {
        TryTransition(UITransition.OpenItemPanel);
    }

    #endregion

    #region Panel Management (Improved from original CombatUIHandler)

    private void InitializePanels()
    {
        SetPanelActive(ActionPanel?.gameObject, false);
        SetPanelActive(AbilityPanel?.gameObject, false);
        SetPanelActive(ItemPanel?.gameObject, false);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    public void MoveCanvasToCharacter(CharacterManager character)
    {
        Debug.Log($"[CombatUIManager] MoveCanvasToCharacter called for: {character?.name ?? "NULL"}");
        if (character == null) 
        {
            Debug.LogWarning("[CombatUIManager] Character is null, cannot move canvas");
            return;
        }
        Debug.Log($"[CombatUIManager] Character position: {character.transform.position}");
        MoveCanvasToTarget(character.transform);
    }

    public void MoveCanvasToTarget(Transform target)
    {
        Debug.Log($"[CombatUIManager] MoveCanvasToTarget called for: {target?.name ?? "NULL"}");
        if (currentPanel == null || target == null) 
        {
            Debug.LogWarning($"[CombatUIManager] Cannot move canvas - currentPanel: {currentPanel?.name ?? "NULL"}, target: {target?.name ?? "NULL"}");
            return;
        }

        // Check if the target is a CharacterManager and populate panels
        if (target.TryGetComponent(out CharacterManager character))
        {
            AbilityPanel?.PopulateAbilityPanel(character);
            ItemPanel?.PopulateItemPanel(character);
        }

        Vector3 newPosition = target.position + _canvasOffset;
        Debug.Log($"[CombatUIManager] Moving canvas from {transform.position} to {newPosition} (target: {target.position} + offset: {_canvasOffset})");
        transform.position = newPosition;
        Debug.Log($"[CombatUIManager] Canvas moved to: {transform.position}");
    }

    private void OpenPanel(CombatPanel panel)
    {
        Debug.Log($"[CombatUIManager] OpenPanel called for: {panel?.name ?? "NULL"}");
        
        if (panel == null) return;

        if (currentPanel != null)
        {
            Debug.Log($"[CombatUIManager] Switching from {currentPanel.name} to {panel.name}");
            SwitchPanels(currentPanel, panel);
        }
        else
        {
            Debug.Log($"[CombatUIManager] Showing first panel: {panel.name}");
            ShowFirstPanel(panel);
        }

        Debug.Log($"[CombatUIManager] Setting camera for panel: {panel.name}");
        SetCameraForPanel(panel);
    }

    private void SwitchPanels(CombatPanel fromPanel, CombatPanel toPanel)
    {
        fromPanel.FadeOutCanvas(() =>
        {
            CompletePanelSwitch(fromPanel, toPanel);
        }, _fadeDuration, _defaultEase);
    }

    private void ShowFirstPanel(CombatPanel panel)
    {
        ShowScreenSpacePanelInputs(false);
        currentPanel = panel;
        currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
    }

    private void CompletePanelSwitch(CombatPanel fromPanel, CombatPanel toPanel)
    {
        // Set up panel relationships
        if (fromPanel != toPanel)
        {
            toPanel.PreviousPanel = fromPanel;
        }

        // Update active states
        fromPanel.gameObject.SetActive(false);
        toPanel.gameObject.SetActive(true);

        // Update current panel reference
        currentPanel = toPanel;

        // Update UI state
        ShowScreenSpacePanelInputs(currentPanel.PreviousPanel != null);
        currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
    }

    private void SetCameraForPanel(CombatPanel panel)
    {
        Debug.Log($"[CombatUIManager] SetCameraForPanel called for panel: {panel?.name ?? "NULL"}");
        
        if (cameraManager == null)
        {
            Debug.LogWarning("[CombatUIManager] CameraManager is null, cannot set camera for panel");
            return;
        }
        
        if (panel == null)
        {
            Debug.LogWarning("[CombatUIManager] Panel is null, cannot set camera");
            return;
        }
        
        if (PanelCameras.TryGetValue(panel, out string cameraName))
        {
            Debug.Log($"[CombatUIManager] Found camera mapping: {panel.name} -> {cameraName}");
            Debug.Log($"[CombatUIManager] Attempting to switch to camera: {cameraName}");
            
            // Add a small delay to ensure this camera switch happens after any turn setup camera changes
            StartCoroutine(DelayedCameraSwitch(cameraName, 0.1f));
        }
        else
        {
            Debug.LogWarning($"[CombatUIManager] No camera mapping found for panel: {panel.name}");
            Debug.Log("[CombatUIManager] Available panel camera mappings:");
            foreach (var kvp in PanelCameras)
            {
                Debug.Log($"  - {kvp.Key?.name ?? "NULL"} -> {kvp.Value}");
            }
        }
    }
    
    private System.Collections.IEnumerator DelayedCameraSwitch(string cameraName, float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"[CombatUIManager] Delayed camera switch to: {cameraName}");
        cameraManager.TrySetActiveCamera(cameraName);
    }

    private void CloseAllPanels()
    {
        if (currentPanel == null) return;

        if (cancelButton != null)
            cancelButton.interactable = false;

        currentPanel.FadeOutCanvas(() =>
        {
            currentPanel = null;
            ShowScreenSpacePanelInputs(false);
        }, _fadeDuration, _defaultEase);
    }

    private void CloseAllPanelsImmediate()
    {
        if (currentPanel != null)
        {
            currentPanel.gameObject.SetActive(false);
            currentPanel = null;
        }
        
        SetPanelActive(ActionPanel?.gameObject, false);
        SetPanelActive(AbilityPanel?.gameObject, false);
        SetPanelActive(ItemPanel?.gameObject, false);
        
        ShowScreenSpacePanelInputs(false);
    }

    #endregion

    #region UI Component Management

    private void FadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, fadeIn ? 1 : 0, _fadeDuration)
            .OnStart(() =>
            {
                canvasGroup.interactable = fadeIn;
                canvasGroup.blocksRaycasts = fadeIn;
            })
            .OnComplete(() => onComplete?.Invoke());
    }

    public void ShowScreenSpacePanelInputs(bool enable)
    {
        FadeCanvasGroup(panelInputsCanvasGroup, enable);
    }

    public void ShowConfirmButton(bool enable)
    {
        if (confirmButton == null) return;

        if (enable)
        {
            ShowScreenSpacePanelInputs(true);
        }

        confirmButton.gameObject.SetActive(enable);
        confirmButton.interactable = enable;
    }

    public void ToggleBackButtonInteractable(bool enable)
    {
        if (cancelButton != null)
            cancelButton.interactable = enable;
    }

    public void ShowTargetSelectionUI(bool enable)
    {
        FadeCanvasGroup(targetSelectionCanvasGroup, enable);
    }

    public void ShowBigNotification(string message, float duration = 2f)
    {
        notificationUI?.ShowBigNotification(message, duration);
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        notificationUI?.ShowNotification(message, duration);
    }

    #endregion

    #region Public API (for backward compatibility)

    public CombatPanel CurrentPanel => currentPanel;

    public void OpenActionPanel()
    {
        HandleShowActionPanel();
    }

    /// <summary>
    /// Continues the current character's turn after an ability completion
    /// Ensures proper UI state and character setup without restarting the turn
    /// </summary>
    public void ContinueCharacterTurn(CharacterManager character)
    {
        // Update the current character
        currentCharacter = character;
        
        // Force transition to PlayerTurn state if not already there
        if (currentState != UIState.PlayerTurn)
        {
            TransitionToState(UIState.PlayerTurn);
        }
        else
        {
            // Already in PlayerTurn state, just refresh the UI
            OnEnterPlayerTurnState();
        }
    }

    public void FadeCurrentPanel(bool fadeIn)
    {
        if (currentPanel == null) return;

        if (fadeIn)
            currentPanel.FadeInCanvas(null, _fadeDuration, _defaultEase);
        else
            currentPanel.FadeOutCanvas(null, _fadeDuration, _defaultEase);
    }

    public void FadeOutCurrentPanel() => FadeCurrentPanel(false);
    public void FadeInCurrentPanel() => FadeCurrentPanel(true);

    #endregion
}
