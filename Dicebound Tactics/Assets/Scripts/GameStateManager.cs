using TacticsToolkit;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

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

    public GameState CurrentGameState => currentGameState;
    private UDictionary<string, bool> flags = new();

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

        // Event Listeners
        EventManager.OnGameOver += ShowGameOverScreen;
        EventManager.OnCombatEncounterStarted += OnCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded += OnCombatEncounterEnded;
    }

    void OnDisable()
    {
        EventManager.OnGameOver -= ShowGameOverScreen;
        EventManager.OnCombatEncounterStarted -= OnCombatEncounterStarted;
        EventManager.OnCombatEncounterEnded -= OnCombatEncounterEnded;
    }

    private void Start()
    {
        InitializeGameState(currentGameState);
    }

    public void InitializeGameState(GameState initialState)
    {
        currentGameState = initialState;
        EventManager.TriggerGameStateChanged(currentGameState);
    }

    public void ChangeGameState(GameState newState)
    {
        currentGameState = newState;
        EventManager.TriggerGameStateChanged(currentGameState);
    }

    public void OnCombatEncounterStarted(CombatEncounter encounter)
    {
        // Handle logic when a combat encounter starts
        ChangeGameState(GameState.Combat);
    }

    public void OnCombatEncounterEnded(CombatEncounter encounter)
    {
        // Handle logic when a combat encounter ends
        ChangeGameState(GameState.Overworld);
    }

    [Button("Show Game Over Screen")]
    public void ShowGameOverScreen()
    {
        // Handle logic for showing the game over screen
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

    #region Flag Management
    public void Set(string key, bool value = true) => flags[key] = value;
    public bool Get(string key) => flags.TryGetValue(key, out bool value) && value;
    public void Clear(string key) => flags.Remove(key);

    public UDictionary<string, bool> GetAll()
    {
        var copy = new UDictionary<string, bool>();
        foreach (var kvp in flags)
        {
            copy.Add(kvp.Key, kvp.Value);
        }
        return copy;
    } // for serialization
    public void LoadAll(UDictionary<string, bool> loadedFlags)
    {
        flags.Clear();
        foreach (var kvp in loadedFlags)
        {
            flags.Add(kvp.Key, kvp.Value);
        }
    }
    public List<string> GetAllKeys()
    {
        return flags.Keys.ToList();
    }
    #endregion
}
