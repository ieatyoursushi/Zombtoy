using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Standalone results display that works without ScoreManager singleton
/// Gets data from PlayerPrefs backup or ScoreManager if available
/// </summary>
public class ResultStandalone : MonoBehaviour
{
    [Header("UI Elements")]
    public Text highscoreText;
    public Text MonstersKilled;
    public Text score;
    
    [Header("Network Settings")]
    private RequestPacket scoreStorage = new RequestPacket("http://localhost:3000/");
    private bool hasPosted = false;
    
    void Start()
    {
        Debug.Log("[ResultStandalone] Initializing results display...");
        DisplayResults();
    }
    
    private void DisplayResults()
    {
        // Try to get data from ScoreManager first
        if (TryGetFromScoreManager(out var smData))
        {
            Debug.Log("[ResultStandalone] Using ScoreManager data");
            DisplayData(smData.score, smData.highScore, smData.kills);
            
            // Try to post score if it's a high score
            if (!hasPosted && smData.score >= smData.highScore && smData.score > 0)
            {
                PostScore(smData.highScore);
            }
        }
        // Fallback to PlayerPrefs backup
        else if (TryGetFromPlayerPrefs(out var ppData))
        {
            Debug.Log("[ResultStandalone] Using PlayerPrefs backup data");
            DisplayData(ppData.score, ppData.highScore, ppData.kills);
            
            // Try to post score if it's a high score  
            if (!hasPosted && ppData.score >= ppData.highScore && ppData.score > 0)
            {
                PostScore(ppData.highScore);
            }
        }
        // Final fallback to current PlayerPrefs values
        else
        {
            Debug.Log("[ResultStandalone] Using current PlayerPrefs values");
            var fallbackScore = PlayerPrefs.GetInt("Score", 0);
            var fallbackHigh = PlayerPrefs.GetInt("HighScore", 0);
            DisplayData(fallbackScore, fallbackHigh, 0);
        }
    }
    
    private bool TryGetFromScoreManager(out (int score, int highScore, int kills) data)
    {
        data = default;
        
        try
        {
            // Check if ScoreManager singleton exists and has finalized data
            if (ScoreManager.HasFinalizedRun())
            {
                var stats = ScoreManager.GetLastRunStats();
                data = (stats.score, stats.highScore, stats.monsterKills);
                return true;
            }
            
            // Try live ScoreManager data
            var score = ScoreManager.GetScore();
            var highScore = ScoreManager.GetHighScore();
            var kills = ScoreManager.GetMonsterKills();
            
            if (score > 0 || highScore > 0)
            {
                data = (score, highScore, kills);
                return true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ResultStandalone] Failed to get ScoreManager data: {ex.Message}");
        }
        
        return false;
    }
    
    private bool TryGetFromPlayerPrefs(out (int score, int highScore, int kills) data)
    {
        data = default;
        
        // Check if we have finalized run data
        if (PlayerPrefs.GetInt("HasFinalizedRun", 0) == 1)
        {
            data = (
                PlayerPrefs.GetInt("LastRunScore", 0),
                PlayerPrefs.GetInt("LastRunHighScore", 0),
                PlayerPrefs.GetInt("LastRunKills", 0)
            );
            return data.score > 0 || data.highScore > 0;
        }
        
        return false;
    }
    
    private void DisplayData(int scoreValue, int highScoreValue, int killsValue)
    {
        Debug.Log($"[ResultStandalone] Displaying - Score: {scoreValue}, High: {highScoreValue}, Kills: {killsValue}");
        
        if (score != null)
            score.text = scoreValue.ToString();
        else
            Debug.LogWarning("[ResultStandalone] Score Text component not assigned!");
            
        if (highscoreText != null)
            highscoreText.text = highScoreValue.ToString();
        else
            Debug.LogWarning("[ResultStandalone] HighScore Text component not assigned!");
            
        if (MonstersKilled != null)
            MonstersKilled.text = killsValue.ToString();
        else
            Debug.LogWarning("[ResultStandalone] MonstersKilled Text component not assigned!");
    }
    
    private async void PostScore(int highScore)
    {
        if (hasPosted) return;
        
        try
        {
            Debug.Log($"[ResultStandalone] Posting high score: {highScore}");
            await scoreStorage.postRequest(scoreStorage.getUrl() + "addScore", highScore.ToString());
            hasPosted = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ResultStandalone] Failed to post score: {ex.Message}");
        }
    }
}
