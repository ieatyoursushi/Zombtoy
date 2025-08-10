using UnityEngine;

/// <summary>
/// Ensures ScoreManager starts fresh when entering a game scene
/// Place this on any GameObject in gameplay scenes (Level1, Level2, etc.)
/// </summary>
public class GameStarter : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool resetScoreOnStart = true;
    [SerializeField] private bool triggerGameStartedEvent = true;
    
    void Start()
    {
        Debug.Log("[GameStarter] Initializing game start sequence...");
        
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        var isGameScene = sceneName.Contains("Level") || sceneName.Contains("Game");
        
        if (isGameScene && resetScoreOnStart)
        {
            // Ensure ScoreManager exists (will auto-create if needed)
            var scoreManager = ScoreManager.Instance;
            if (scoreManager != null)
            {
                Debug.Log("[GameStarter] Found ScoreManager - forcing score reset for new game");
                scoreManager.StartNewGame();
            }
            else
            {
                Debug.LogWarning("[GameStarter] Could not access ScoreManager!");
            }
            
            // Set game state to Playing
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.ChangeState(GameStateManager.GameState.Playing);
                Debug.Log("[GameStarter] Set game state to Playing");
            }
        }
        else
        {
            Debug.Log($"[GameStarter] Skipping game start logic - not a game scene: {sceneName}");
        }
    }
}
