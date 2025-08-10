using UnityEngine;

/// <summary>
/// Debug utility to check ScoreManager status across scenes
/// Place this on any GameObject to get information about ScoreManager state
/// </summary>
public class ScoreManagerDebugger : MonoBehaviour
{
    [Header("Debug Info")]
    [SerializeField] private bool logOnStart = true;
    [SerializeField] private bool logOnUpdate = false;
    
    void Start()
    {
        if (logOnStart)
        {
            LogScoreManagerStatus("Start");
        }
    }
    
    void Update()
    {
        if (logOnUpdate && Time.frameCount % 60 == 0) // Every second
        {
            LogScoreManagerStatus("Update");
        }
    }
    
    public void LogScoreManagerStatus(string context = "Manual")
    {
        var sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[ScoreManagerDebugger] === {context} - Scene: {sceneName} ===");
        
        // Check for ScoreManager components in current scene
        var scoreManagers = FindObjectsOfType<ScoreManager>();
        Debug.Log($"[ScoreManagerDebugger] ScoreManager components in scene: {scoreManagers.Length}");
        
        for (int i = 0; i < scoreManagers.Length; i++)
        {
            var sm = scoreManagers[i];
            var smScene = sm.gameObject.scene.name;
            var isPersistent = smScene == "DontDestroyOnLoad";
            Debug.Log($"[ScoreManagerDebugger] [{i}] Name: {sm.gameObject.name}, Scene: {smScene}, Persistent: {isPersistent}");
        }
        
        // Check singleton instance
        var instance = ScoreManager.Instance;
        if (instance != null)
        {
            var instanceScene = instance.gameObject.scene.name;
            var instancePersistent = instanceScene == "DontDestroyOnLoad";
            Debug.Log($"[ScoreManagerDebugger] Singleton Instance: {instance.gameObject.name}, Scene: {instanceScene}, Persistent: {instancePersistent}");
            
            // Check data availability
            Debug.Log($"[ScoreManagerDebugger] HasFinalizedRun: {ScoreManager.HasFinalizedRun()}");
            Debug.Log($"[ScoreManagerDebugger] Current Score: {ScoreManager.GetScore()}");
            Debug.Log($"[ScoreManagerDebugger] High Score: {ScoreManager.GetHighScore()}");
            Debug.Log($"[ScoreManagerDebugger] Monster Kills: {ScoreManager.GetMonsterKills()}");
            
            if (ScoreManager.HasFinalizedRun())
            {
                var stats = ScoreManager.GetLastRunStats();
                Debug.Log($"[ScoreManagerDebugger] Last Run Stats - Score: {stats.score}, High: {stats.highScore}, Kills: {stats.monsterKills}");
            }
        }
        else
        {
            Debug.LogWarning("[ScoreManagerDebugger] No ScoreManager singleton instance found!");
        }
        
        // Check PlayerPrefs backup
        var hasBackup = PlayerPrefs.GetInt("HasFinalizedRun", 0) == 1;
        Debug.Log($"[ScoreManagerDebugger] PlayerPrefs Backup Available: {hasBackup}");
        if (hasBackup)
        {
            var backupScore = PlayerPrefs.GetInt("LastRunScore", 0);
            var backupHigh = PlayerPrefs.GetInt("LastRunHighScore", 0);
            var backupKills = PlayerPrefs.GetInt("LastRunKills", 0);
            Debug.Log($"[ScoreManagerDebugger] Backup Data - Score: {backupScore}, High: {backupHigh}, Kills: {backupKills}");
        }
        
        Debug.Log("[ScoreManagerDebugger] === End Status ===");
    }
    
    [ContextMenu("Log Status Now")]
    public void LogStatusNow()
    {
        LogScoreManagerStatus("Manual");
    }
}
