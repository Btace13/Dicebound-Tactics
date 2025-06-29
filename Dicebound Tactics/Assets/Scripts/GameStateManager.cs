using TacticsToolkit;
using UnityEngine;

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
}
