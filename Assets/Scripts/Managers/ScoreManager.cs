using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Centralized score management system
/// Multiplayer-ready with event-driven architecture
/// </summary>
public class ScoreManager : Singleton<ScoreManager>
{
    // Singleton behavior overrides
    protected override bool AllowAutoCreate => true;   // Allow creation in game scenes for first game
    protected override bool Persistent => true;        // Keep across scenes for results
    protected override bool LogCreation => false;      // Reduce log noise
    [Header("Score Configuration")]
    public int enemyKillScore = 100;
    public int bossKillScore = 500;
    public int comboMultiplier = 2;
    
    [Header("UI References")]
    [SerializeField] private Text scoreText;
    [SerializeField] private GameObject cheatModeIndicator;
    
    // Properties
    [System.NonSerialized] private int _currentScore = 0; // Force initialize to 0
    public int CurrentScore 
    { 
        get => _currentScore;
        private set 
        { 
            if (_currentScore != value)
            {
                Debug.Log($"[ScoreManager] CurrentScore changed: {_currentScore} -> {value}");
            }
            _currentScore = value;
        } 
    }
    public int HighScore { get; private set; }
    public int MonsterKills { get; private set; }
    public bool IsHighScore { get; private set; }
    public bool CheatModeActive { get; private set; }

    // Run summary snapshot (persisted until next run start)
    public struct RunStats
    {
        public int score;
        public int highScore;
        public int monsterKills;
    }
    private RunStats lastRun;
    private bool runFinalized;
    
    // Events
    public System.Action<int> OnScoreUpdated;
    public System.Action<int> OnHighScoreUpdated;
    public System.Action<bool> OnHighScoreStatusChanged;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Force clear any Unity-serialized score data
        _currentScore = 0;
        Debug.Log($"[ScoreManager] Initialized fresh ScoreManager instance");
        
        LoadHighScore();
        
        // Initialize UI first, before any score operations
        InitializeUI();
        
        // Always start with a clean score when ScoreManager is first created
        // GameStarter will handle subsequent resets when entering game scenes
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[ScoreManager] Awake in scene: {currentScene}");
        
        if (currentScene == "Menu 1") // Results scene
        {
            Debug.Log("[ScoreManager] Created in results scene - preserving any existing data");
            // Don't reset here, let the results display existing data
        }
        else
        {
            Debug.Log("[ScoreManager] Created in game scene - starting fresh");
            ResetScore();
        }
    }
    
    private void Start()
    {
        // UI already initialized in Awake(), just subscribe to events
        SubscribeToEvents();
    }
    
    void OnEnable()
    {
        // Re-initialize UI references when entering a new scene
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Reset score for game scenes to ensure clean start
        if (scene.name.Contains("Level") || scene.name.Contains("Game"))
        {
            Debug.Log($"[ScoreManager] Game scene detected ({scene.name}) - resetting for new game");
            _currentScore = 0; // Direct field assignment 
            MonsterKills = 0;  // Reset monster kills too
            runFinalized = false; // Reset run state
        }
        
        // Reset UI references so they get re-found in the new scene
        scoreText = null;
        cheatModeIndicator = null;
        InitializeUI();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEvents();
    }
    
    private void InitializeUI()
    {
        // Find score text if not assigned
        if (scoreText == null)
        {
            scoreText = GetComponent<Text>();
            if (scoreText == null)
            {
                var scoreGO = GameObject.Find("ScoreText") ?? GameObject.Find("Score") ?? GameObject.Find("PlayerScore");
                if (scoreGO != null)
                {
                    scoreText = scoreGO.GetComponent<Text>();
                }
            }
        }
            
        // Find cheat mode indicator if not assigned
        if (cheatModeIndicator == null)
        {
            cheatModeIndicator = GameObject.Find("CheatMode") ?? GameObject.Find("CheatIndicator");
        }
            
        CheatModeActive = cheatModeIndicator != null;
        UpdateScoreUI();
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnEnemyKilled += HandleEnemyKilled;
        GameEvents.OnGameOver += SaveHighScore;
    GameEvents.OnGameOver += CaptureRunStats;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnEnemyKilled -= HandleEnemyKilled;
        GameEvents.OnGameOver -= SaveHighScore;
    GameEvents.OnGameOver -= CaptureRunStats;
    }
    
    private void HandleEnemyKilled(int scoreValue, Vector3 position)
    {
        // Temporarily disable game state checking to restore functionality
        // TODO: Re-enable once GameStateManager integration is properly tested
        
        Debug.Log($"[ScoreManager] Enemy killed for {scoreValue} points (game state check temporarily disabled)");
        
        if (GameStateManager.Instance != null)
        {
            Debug.Log($"[ScoreManager] GameStateManager found - State: {GameStateManager.Instance.CurrentState}");
        }
        else
        {
            Debug.Log("[ScoreManager] No GameStateManager found");
        }
        
        AddScore(scoreValue);
        MonsterKills++;
    }
    
    public void AddScore(int points)
    {
        Debug.Log($"[ScoreManager] AddScore called: {CurrentScore} + {points} = {CurrentScore + points} | StackTrace: {System.Environment.StackTrace}");
        CurrentScore += points;
        UpdateHighScoreStatus();
        UpdateScoreUI();
        
        OnScoreUpdated?.Invoke(CurrentScore);
        GameEvents.ScoreChanged(CurrentScore);
    }
    
    public void ResetScore()
    {
        Debug.Log($"[ScoreManager] ResetScore called - Current score: {CurrentScore} -> 0");
        CurrentScore = 0;
        MonsterKills = 0;
        runFinalized = false; // new run started
        UpdateHighScoreStatus();
        UpdateScoreUI();
        
        // Clear backup data since we're starting a new run
        PlayerPrefs.DeleteKey("LastRunScore");
        PlayerPrefs.DeleteKey("LastRunHighScore");
        PlayerPrefs.DeleteKey("LastRunKills");
        PlayerPrefs.DeleteKey("HasFinalizedRun");
        PlayerPrefs.Save();
        
        OnScoreUpdated?.Invoke(CurrentScore);
        GameEvents.ScoreChanged(CurrentScore);
        
        // Notify other systems that a new game has started
        GameEvents.GameStarted();
        
        Debug.Log("[ScoreManager] Score reset complete - new score: " + CurrentScore);
    }
    
    public void StartNewGame()
    {
        Debug.Log("[ScoreManager] Explicitly starting new game - resetting score");
        ResetScore();
    }
    
    private void UpdateHighScoreStatus()
    {
        bool wasHighScore = IsHighScore;
        IsHighScore = CurrentScore >= HighScore && CheatModeActive;
        
        if (IsHighScore && CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
            OnHighScoreUpdated?.Invoke(HighScore);
            GameEvents.HighScoreChanged(HighScore);
        }
        
        if (wasHighScore != IsHighScore)
        {
            OnHighScoreStatusChanged?.Invoke(IsHighScore);
        }
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {CurrentScore:N0}";
        }
    }
    
    private void LoadHighScore()
    {
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    private void SaveHighScore()
    {
        if (CurrentScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", CurrentScore);
            PlayerPrefs.Save();
        }
    }

    private void CaptureRunStats()
    {
        lastRun = new RunStats
        {
            score = CurrentScore,
            highScore = Mathf.Max(HighScore, CurrentScore),
            monsterKills = MonsterKills
        };
        runFinalized = true;
        
        // Backup to PlayerPrefs in case of singleton conflicts
        PlayerPrefs.SetInt("LastRunScore", lastRun.score);
        PlayerPrefs.SetInt("LastRunHighScore", lastRun.highScore);
        PlayerPrefs.SetInt("LastRunKills", lastRun.monsterKills);
        PlayerPrefs.SetInt("HasFinalizedRun", 1);
        PlayerPrefs.Save();
        
        Debug.Log($"[ScoreManager] Run stats captured - Score: {lastRun.score}, High: {lastRun.highScore}, Kills: {lastRun.monsterKills}");
    }

    public static RunStats GetLastRunStats()
    {
        if (Instance != null && Instance.runFinalized)
        {
            return Instance.lastRun;
        }
        
        // Fallback: try to get from PlayerPrefs backup
        if (PlayerPrefs.HasKey("LastRunScore"))
        {
            return new RunStats
            {
                score = PlayerPrefs.GetInt("LastRunScore", 0),
                highScore = PlayerPrefs.GetInt("LastRunHighScore", 0),
                monsterKills = PlayerPrefs.GetInt("LastRunKills", 0)
            };
        }
        
        // Final fallback: return current values if instance exists
        if (Instance != null)
        {
            return new RunStats
            {
                score = Instance.CurrentScore,
                highScore = Instance.HighScore,
                monsterKills = Instance.MonsterKills
            };
        }
        
        return default;
    }

    public static bool HasFinalizedRun()
    {
        if (Instance != null && Instance.runFinalized)
            return true;
            
        // Check backup in PlayerPrefs
        return PlayerPrefs.GetInt("HasFinalizedRun", 0) == 1;
    }
    
    // Static methods for backwards compatibility
    public static int GetScore() => Instance?.CurrentScore ?? 0;
    public static int GetHighScore() => Instance?.HighScore ?? 0;
    public static int GetMonsterKills() => Instance?.MonsterKills ?? 0;
    public static bool GetIsHighScore() => Instance?.IsHighScore ?? false;
    
    // Static game control methods
    public static void StartNewGameStatic() => Instance?.StartNewGame();
    public static void ResetScoreStatic() => Instance?.ResetScore();
    
    // Debug method to check persistence status
    public static string GetInstanceStatus()
    {
        if (Instance == null) return "No Instance";
        
        var scene = Instance.gameObject.scene.name;
        var persistent = scene == "DontDestroyOnLoad";
        return $"Instance exists in scene '{scene}', Persistent: {persistent}";
    }
}


