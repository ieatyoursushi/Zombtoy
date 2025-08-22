using UnityEngine;
using System;

/// <summary>
/// Core game events for decoupled communication between systems
/// Ready for multiplayer event synchronization
/// </summary>
public static class GameEvents
{
    // Player Events
    public static event Action<int> OnPlayerHealthChanged;
    public static event Action OnPlayerDeath;
    public static event Action OnPlayerRevive;
    public static event Action<int> OnPlayerStaminaChanged;
    
    // Combat Events
    public static event Action<int, Vector3> OnEnemyKilled; // score, position
    public static event Action<GameObject, int, Vector3> OnEnemyDamaged; // enemy, damage, hit point
    public static event Action<Vector3, int> OnWeaponFired; // position, damage
    
    // Score Events
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnHighScoreChanged;
    
    // Game State Events
    public static event Action OnGameStarted;
    public static event Action OnGamePaused;
    public static event Action OnGameResumed;
    public static event Action OnGameOver;
    public static event Action OnLevelComplete;
    
    // Enemy Events
    public static event Action<GameObject> OnEnemySpawned;
    public static event Action<GameObject> OnEnemyDestroyed;
    public static event Action<int> OnEnemyCountChanged;
    
    // Item Events
    public static event Action<int> OnAmmoPickup;
    public static event Action<int> OnHealthPickup;
    
    // Multiplayer Events (future)
    public static event Action<string, object> OnNetworkEvent;
    
    // Debug helpers
    public static int GetSubscriberCount<T>(System.Action<T> eventDelegate) 
        => eventDelegate?.GetInvocationList()?.Length ?? 0;
    
    public static int GetSubscriberCount<T1, T2>(System.Action<T1, T2> eventDelegate) 
        => eventDelegate?.GetInvocationList()?.Length ?? 0;
    
    public static int GetSubscriberCount<T1, T2, T3>(System.Action<T1, T2, T3> eventDelegate) 
        => eventDelegate?.GetInvocationList()?.Length ?? 0;
    
    public static int GetSubscriberCount(System.Action eventDelegate) 
        => eventDelegate?.GetInvocationList()?.Length ?? 0;
    
    public static void SafeInvoke<T>(System.Action<T> eventDelegate, T arg, string eventName = "Unknown")
    {
        if (eventDelegate == null) return;
        
        foreach (var handler in eventDelegate.GetInvocationList())
        {
            try
            {
                ((System.Action<T>)handler)(arg);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[GameEvents] Exception in {eventName} handler {handler.Method.Name}: {ex.Message}");
            }
        }
    }
    
    public static void SafeInvoke(System.Action eventDelegate, string eventName = "Unknown")
    {
        if (eventDelegate == null) return;
        
        foreach (var handler in eventDelegate.GetInvocationList())
        {
            try
            {
                ((System.Action)handler)();
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[GameEvents] Exception in {eventName} handler {handler.Method.Name}: {ex.Message}");
            }
        }
    }

    // Trigger Methods (with debug info)
    public static void PlayerHealthChanged(int health)
    {
        var count = GetSubscriberCount(OnPlayerHealthChanged);
        if (UnityEngine.Application.isPlaying && count == 0)
            UnityEngine.Debug.LogWarning("[GameEvents] PlayerHealthChanged fired but no subscribers");
        OnPlayerHealthChanged?.Invoke(health);
    }
    
    public static void PlayerDeath() 
    {
        var count = GetSubscriberCount(OnPlayerDeath);
        UnityEngine.Debug.Log($"[GameEvents] PlayerDeath fired, subscribers={count}");
        OnPlayerDeath?.Invoke();
    }
    
    public static void PlayerRevive() 
    {
        var count = GetSubscriberCount(OnPlayerRevive);
        UnityEngine.Debug.Log($"[GameEvents] PlayerRevive fired, subscribers={count}");
        OnPlayerRevive?.Invoke();
    }
    
    public static void PlayerStaminaChanged(int stamina) => OnPlayerStaminaChanged?.Invoke(stamina);
    
    public static void EnemyKilled(int score, Vector3 position)
    {
        var count = GetSubscriberCount(OnEnemyKilled);
        UnityEngine.Debug.Log($"[GameEvents] EnemyKilled fired, subscribers={count}, score={score}");
        OnEnemyKilled?.Invoke(score, position);
    }
    
    public static void EnemyDamaged(GameObject enemy, int damage, Vector3 hitPoint) => OnEnemyDamaged?.Invoke(enemy, damage, hitPoint);
    public static void WeaponFired(Vector3 position, int damage) => OnWeaponFired?.Invoke(position, damage);
    
    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void HighScoreChanged(int highScore) => OnHighScoreChanged?.Invoke(highScore);
    
    public static void GameStarted() => OnGameStarted?.Invoke();
    public static void GamePaused() => OnGamePaused?.Invoke();
    public static void GameResumed() => OnGameResumed?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void LevelComplete() => OnLevelComplete?.Invoke();
    
    public static void EnemySpawned(GameObject enemy)
    {
        var count = GetSubscriberCount(OnEnemySpawned);
        UnityEngine.Debug.Log($"[GameEvents] EnemySpawned fired, subscribers={count}, enemy={enemy?.name}");
        OnEnemySpawned?.Invoke(enemy);
    }
    
    public static void EnemyDestroyed(GameObject enemy)
    {
        var count = GetSubscriberCount(OnEnemyDestroyed);
        UnityEngine.Debug.Log($"[GameEvents] EnemyDestroyed fired, subscribers={count}, enemy={enemy?.name}");
        OnEnemyDestroyed?.Invoke(enemy);
    }
    
    public static void EnemyCountChanged(int count) => OnEnemyCountChanged?.Invoke(count);
    
    public static void AmmoPickup(int amount) => OnAmmoPickup?.Invoke(amount);
    public static void HealthPickup(int amount) => OnHealthPickup?.Invoke(amount);
    
    public static void NetworkEvent(string eventType, object data) => OnNetworkEvent?.Invoke(eventType, data);
}
