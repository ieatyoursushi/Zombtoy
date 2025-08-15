using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Button handler for results screen
/// Handles "Play Again" functionality with proper ScoreManager state management
/// </summary>
public class ResultsButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Level1"; // Scene to load for new game
    [SerializeField] private int gameSceneIndex = 2;          // Alternative: use scene index
    [SerializeField] private bool useSceneName = true;        // Use scene name vs index
    
    /// <summary>
    /// Call this from UI Button's OnClick event for "Play Again"
    /// </summary>
    public void PlayAgain()
    {
        Debug.Log("[ResultsButton] Starting new game...");
        
        // Force ScoreManager reset before scene transition
        var scoreManager = ScoreManager.Instance;
        if (scoreManager != null)
        {
            Debug.Log("[ResultsButton] Forcing score reset before scene transition");
            scoreManager.StartNewGame();
        }
        else
        {
            Debug.LogWarning("[ResultsButton] No ScoreManager found - continuing anyway");
        }
        
        // Load the game scene
        if (useSceneName && !string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log($"[ResultsButton] Loading scene: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.Log($"[ResultsButton] Loading scene index: {gameSceneIndex}");
            SceneManager.LoadScene(gameSceneIndex);
        }
    }
    
    /// <summary>
    /// Call this from UI Button's OnClick event for "Main Menu"
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("[ResultsButton] Going to main menu...");
        SceneManager.LoadScene(0); // Assuming main menu is scene index 0
    }
    
    /// <summary>
    /// Call this from UI Button's OnClick event for "Quit"
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[ResultsButton] Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
