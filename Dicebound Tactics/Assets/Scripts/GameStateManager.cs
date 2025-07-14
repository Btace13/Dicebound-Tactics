using TacticsToolkit;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public enum GameState
{
    Overworld,
    Combat,
    Menu,
    Cutscene
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState currentGameState = GameState.Overworld;

    [Header("References")]
    [SerializeField] private CanvasGroup gameOverScreenCanvasGroup;
    [SerializeField] private CanvasGroup gameOverTextCanvasGroup;

    [Header("Game State Events")]
    public GameEventGameState OnGameStateChanged;

    public GameState CurrentGameState => currentGameState;

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
    }

    private void Start()
    {
        InitializeGameState(currentGameState);
    }

    public void InitializeGameState(GameState initialState)
    {
        currentGameState = initialState;
        OnGameStateChanged?.Raise(currentGameState);
        Debug.Log($"Game state initialized to: {currentGameState}");
    }

    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
        OnGameStateChanged?.Raise(currentGameState);
        Debug.Log($"Game state changed to: {currentGameState}");
    }

    public void OnCombatEncounterStarted(CombatEncounter encounter)
    {
        // Handle logic when a combat encounter starts
        Debug.Log("Combat encounter started. Switching to combat state.");
        ChangeGameState(GameState.Combat);
    }

    public void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        // Handle logic when a combat encounter ends
        Debug.Log("Combat encounter ended. Returning to overworld.");
        ChangeGameState(GameState.Overworld);
    }

    [Button("Show Game Over Screen")]
    public void ShowGameOverScreen()
    {
        // Handle logic for showing the game over screen
        Debug.Log("Game Over! Switching to game over state.");
        ChangeGameState(GameState.Menu);

        gameOverScreenCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.InOutQuad)
        .OnComplete(() =>
        {
            gameOverTextCanvasGroup.DOFade(1, 0.35f).SetEase(Ease.InOutQuad);
            gameOverScreenCanvasGroup.blocksRaycasts = true;
            gameOverScreenCanvasGroup.interactable = true;
        });
    }

    [Button("Hide Game Over Screen")]
    public void HideGameOverScreen()
    {
        // Handle logic for hiding the game over screen
        Debug.Log("Hiding game over screen.");
        gameOverTextCanvasGroup.DOFade(0, 0.35f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            gameOverScreenCanvasGroup.DOFade(0, 0.35f).SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                gameOverScreenCanvasGroup.blocksRaycasts = false;
                gameOverScreenCanvasGroup.interactable = false;
            });
        });
    }
}
