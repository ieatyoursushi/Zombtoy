using UnityEngine;

/// <summary>
/// Centralized game state management
/// Handles pause, game over, level transitions
/// Ready for multiplayer synchronization
/// </summary>
public class GameStateManager : Singleton<GameStateManager>
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Loading,
        Victory
    }
    
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    
    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;
    
    // Properties
    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public bool IsPaused => currentState == GameState.Paused;
    public bool IsGameOver => currentState == GameState.GameOver;
    
    // Events
    public System.Action<GameState, GameState> OnStateChanged;
    public System.Action OnGameStarted;
    public System.Action OnGameEnded;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void Start()
    {
        InitializeDependencies();
        SubscribeToEvents();
        Debug.Log("[GameStateManager] Starting - setting initial state to Playing");
        ChangeState(GameState.Playing); // Default state for gameplay scenes
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEvents();
    }
    
    private void InitializeDependencies()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnGameResumed += HandleGameResumed;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnGameResumed -= HandleGameResumed;
    }
    
    private void HandlePlayerDeath()
    {
        ChangeState(GameState.GameOver);
    }
    
    private void HandleGamePaused()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
        }
    }
    
    private void HandleGameResumed()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
        }
    }
    
    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
            return;
            
        GameState previousState = currentState;
        currentState = newState;
        
        // Handle state transitions
        OnStateExit(previousState);
        OnStateEnter(newState);
        
        // Fire events
        OnStateChanged?.Invoke(previousState, newState);
        
        Debug.Log($"[GameStateManager] State changed from {previousState} to {newState}");
    }
    
    private void OnStateExit(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                // Save any necessary data
                break;
            case GameState.Paused:
                // Clean up pause-related UI
                break;
        }
    }
    
    private void OnStateEnter(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                OnGameStarted?.Invoke();
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
                
            case GameState.GameOver:
                Time.timeScale = 1f; // Allow game over animations
                OnGameEnded?.Invoke();
                GameEvents.GameOver();
                break;
                
            case GameState.Loading:
                Time.timeScale = 1f;
                break;
        }
    }
    
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
            GameEvents.GamePaused();
        }
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(GameState.Playing);
            GameEvents.GameResumed();
        }
    }
    
    public void StartGame()
    {
        ChangeState(GameState.Playing);
    }
    
    public void EndGame()
    {
        ChangeState(GameState.GameOver);
    }
    
    public void RestartGame()
    {
        ChangeState(GameState.Loading);
        // Scene reload logic would go here
        ChangeState(GameState.Playing);
    }
    
    // Static methods for easy access
    public static GameState GetCurrentState() => Instance?.CurrentState ?? GameState.Menu;
    public static bool IsCurrentlyPlaying() => Instance?.IsPlaying ?? false;
    public static bool IsCurrentlyPaused() => Instance?.IsPaused ?? false;
    public static void Pause() => Instance?.PauseGame();
    public static void Resume() => Instance?.ResumeGame();
}
