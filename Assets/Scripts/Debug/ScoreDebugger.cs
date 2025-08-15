using UnityEngine;

/// <summary>
/// Temporary debug script to test score reset functionality
/// Add to any GameObject and use the context menu options
/// </summary>
public class ScoreDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private bool logScoreEverySecond = false;
    [SerializeField] private bool watchForScoreChanges = true;
    
    private int lastKnownScore = -1;
    
    void Update()
    {
        if (logScoreEverySecond && Time.frameCount % 60 == 0) // Every second
        {
            LogCurrentScore();
        }
        
        if (watchForScoreChanges)
        {
            var currentScore = ScoreManager.GetScore();
            if (currentScore != lastKnownScore)
            {
                Debug.Log($"[ScoreDebugger] Score changed detected: {lastKnownScore} -> {currentScore}");
                lastKnownScore = currentScore;
            }
        }
    }
    
    [ContextMenu("Log Current Score")]
    public void LogCurrentScore()
    {
        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            Debug.Log($"[ScoreDebugger] Current Score: {ScoreManager.GetScore()}, High Score: {ScoreManager.GetHighScore()}, Kills: {ScoreManager.GetMonsterKills()}");
        }
        else
        {
            Debug.LogWarning("[ScoreDebugger] No ScoreManager instance found!");
        }
    }
    
    [ContextMenu("Force Reset Score")]
    public void ForceResetScore()
    {
        Debug.Log("[ScoreDebugger] Force resetting score...");
        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            sm.ResetScore();
            Debug.Log("[ScoreDebugger] Score reset complete!");
        }
        else
        {
            Debug.LogWarning("[ScoreDebugger] No ScoreManager instance found!");
        }
    }
    
    [ContextMenu("Add Test Score")]
    public void AddTestScore()
    {
        Debug.Log("[ScoreDebugger] Adding test score...");
        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            sm.AddScore(100);
            Debug.Log("[ScoreDebugger] Added 100 points!");
        }
        else
        {
            Debug.LogWarning("[ScoreDebugger] No ScoreManager instance found!");
        }
    }
    
    [ContextMenu("Continuous Score Monitoring")]
    public void StartContinuousMonitoring()
    {
        StartCoroutine(ContinuousScoreMonitor());
    }
    
    private System.Collections.IEnumerator ContinuousScoreMonitor()
    {
        Debug.Log("[ScoreDebugger] Starting continuous score monitoring...");
        int previousScore = ScoreManager.GetScore();
        
        for (int i = 0; i < 30; i++) // Monitor for 30 seconds
        {
            yield return new UnityEngine.WaitForSeconds(1f);
            
            int currentScore = ScoreManager.GetScore();
            if (currentScore != previousScore)
            {
                Debug.Log($"[ScoreDebugger] Score change detected: {previousScore} -> {currentScore}");
                var sm = ScoreManager.Instance;
                if (sm != null)
                {
                    Debug.Log($"[ScoreDebugger] ScoreManager Instance: {sm.gameObject.name} in scene {sm.gameObject.scene.name}");
                }
                previousScore = currentScore;
            }
        }
        
        Debug.Log("[ScoreDebugger] Continuous monitoring complete");
    }
    
    [ContextMenu("Test Start New Game")]
    public void TestStartNewGame()
    {
        Debug.Log("[ScoreDebugger] Testing StartNewGame...");
        var sm = ScoreManager.Instance;
        if (sm != null)
        {
            sm.StartNewGame();
            Debug.Log("[ScoreDebugger] StartNewGame called!");
        }
        else
        {
            Debug.LogWarning("[ScoreDebugger] No ScoreManager instance found!");
        }
    }
}
