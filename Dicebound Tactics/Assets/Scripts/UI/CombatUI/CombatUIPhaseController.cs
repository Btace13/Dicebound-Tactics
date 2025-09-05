using UnityEngine;
using TacticsToolkit;

// Central controller that orchestrates which combat UI elements are visible for each phase of combat.
// It relies on the existing EventManager events and delegates concrete show/hide to CombatUIHandler and existing panels.
public class CombatUIPhaseController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CombatUIHandler ui; // assign in scene

    // Optional: selection panel reacts to OnSelectingATarget already, we only need to emit the event.

    private void Awake()
    {
        if (ui == null)
        {
#if UNITY_2023_1_OR_NEWER
            ui = Object.FindFirstObjectByType<CombatUIHandler>();
#else
        ui = Object.FindObjectOfType<CombatUIHandler>();
#endif
        }

        // Subscribe to high-level combat flow
        EventManager.OnBattleStarted += OnBattleStarted;
        EventManager.OnBattleEnded += OnBattleEnded;
        EventManager.OnCharacterTurnStarted += OnCharacterTurnStarted;
        EventManager.OnCharacterTurnEnded += OnCharacterTurnEnded;
        EventManager.OnEnemyTurnStarted += OnEnemyTurnStarted;
        EventManager.OnEnemyTurnEnded += OnEnemyTurnEnded;
        EventManager.OnSelectingATarget += OnSelectingATarget;

        // Panel show requests
        EventManager.OnShowActionPanel += HandleShowActionPanel;
        EventManager.OnShowAbilityPanel += HandleShowAbilityPanel;
        EventManager.OnShowItemPanel += HandleShowItemPanel;
    }

    private void OnDisable()
    {
        EventManager.OnBattleStarted -= OnBattleStarted;
        EventManager.OnBattleEnded -= OnBattleEnded;
        EventManager.OnCharacterTurnStarted -= OnCharacterTurnStarted;
        EventManager.OnCharacterTurnEnded -= OnCharacterTurnEnded;
        EventManager.OnEnemyTurnStarted -= OnEnemyTurnStarted;
        EventManager.OnEnemyTurnEnded -= OnEnemyTurnEnded;
        EventManager.OnSelectingATarget -= OnSelectingATarget;

        // Panel show requests
        EventManager.OnShowActionPanel -= HandleShowActionPanel;
        EventManager.OnShowAbilityPanel -= HandleShowAbilityPanel;
        EventManager.OnShowItemPanel -= HandleShowItemPanel;
    }

    // Phase: Battle started -> show base UI and action panel
    private void OnBattleStarted()
    {
        if (ui == null) return;
        ui.ShowCombatUI();
        ui.OpenActionPanel();
    }

    // Phase: Battle ended -> hide everything
    private void OnBattleEnded()
    {
        if (ui == null) return;
        ui.HideCombatUI();
        ui.CloseAllPanels();
    }

    // Phase: Player turn -> open action panel and anchor to character
    private void OnCharacterTurnStarted(CharacterManager character)
    {
        if (ui == null) return;
        ui.OpenActionPanel();
        ui.MoveCanvasToCharacter(character);
        // Ensure confirm is hidden until we enter targeting
        ui.ShowConfirmButton(false);
    }

    private void OnCharacterTurnEnded(CharacterManager _)
    {
        if (ui == null) return;
        ui.CloseAllPanels();
        ui.ShowConfirmButton(false);
    }

    // Phase: Enemy turn -> close player controls
    private void OnEnemyTurnStarted(EnemyManager _)
    {
        if (ui == null) return;
        ui.CloseAllPanels();
        ui.ShowScreenSpacePanelInputs(false);
        ui.ShowConfirmButton(false);
    }

    private void OnEnemyTurnEnded(EnemyManager _)
    {
        // No-op: CharacterTurnStarted will configure next phase
    }

    // Sub-phase: Target selection toggles confirm button and selection UI via existing SelectionPanel
    private void OnSelectingATarget(bool isSelecting)
    {
        if (ui == null) return;

        // The SelectionPanel listens to this event to show/hide targeting buttons.
        // Here we only manage the confirm button visibility and inputs.
        ui.ShowConfirmButton(isSelecting);
    }

    // External requests to show specific panels
    private void HandleShowActionPanel()
    {
        if (ui == null || ui.ActionPanel == null) return;
        ui.OpenPanel(ui.ActionPanel);
    }

    private void HandleShowAbilityPanel()
    {
        if (ui == null || ui.AbilityPanel == null) return;
        ui.OpenPanel(ui.AbilityPanel);
        ui.ToggleBackButtonInteractable(true);
        ui.ShowConfirmButton(false);
    }

    private void HandleShowItemPanel()
    {
        if (ui == null || ui.ItemPanel == null) return;
        ui.OpenPanel(ui.ItemPanel);
        //ui.ToggleBackButtonInteractable(true);
    }
}
