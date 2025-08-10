using UnityEngine;

/// <summary>
/// Handles game over animations and UI
/// Now integrated with centralized GameStateManager
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    private bool gameOverTriggered = false;
    
    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        InitializeDependencies();
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
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
        GameEvents.OnGameOver += HandleGameOver;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
        GameEvents.OnGameOver -= HandleGameOver;
    }
    
    private void HandlePlayerDeath()
    {
        TriggerGameOver();
    }
    
    private void HandleGameOver()
    {
        TriggerGameOver();
    }
    
    private void TriggerGameOver()
    {
        if (gameOverTriggered)
            return;
            
        gameOverTriggered = true;
        
        // Notify GameStateManager that game is over
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ChangeState(GameStateManager.GameState.GameOver);
        }
        
        if (animator != null)
        {
            animator.SetTrigger("GameOver");
        }
        
        Debug.Log("[GameOverManager] Game Over animation triggered");
    }
    
    // Legacy Update method for backwards compatibility
    void Update()
    {
        if (!gameOverTriggered && playerHealth != null && playerHealth.currentHealth <= 0)
        {
            TriggerGameOver();
        }
    }
    
    public void ResetGameOver()
    {
        gameOverTriggered = false;
    }
}
