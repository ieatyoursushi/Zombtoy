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
    
    // Trigger Methods
    public static void PlayerHealthChanged(int health) => OnPlayerHealthChanged?.Invoke(health);
    public static void PlayerDeath() => OnPlayerDeath?.Invoke();
    public static void PlayerRevive() => OnPlayerRevive?.Invoke();
    public static void PlayerStaminaChanged(int stamina) => OnPlayerStaminaChanged?.Invoke(stamina);
    
    public static void EnemyKilled(int score, Vector3 position) => OnEnemyKilled?.Invoke(score, position);
    public static void EnemyDamaged(GameObject enemy, int damage, Vector3 hitPoint) => OnEnemyDamaged?.Invoke(enemy, damage, hitPoint);
    public static void WeaponFired(Vector3 position, int damage) => OnWeaponFired?.Invoke(position, damage);
    
    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void HighScoreChanged(int highScore) => OnHighScoreChanged?.Invoke(highScore);
    
    public static void GameStarted() => OnGameStarted?.Invoke();
    public static void GamePaused() => OnGamePaused?.Invoke();
    public static void GameResumed() => OnGameResumed?.Invoke();
    public static void GameOver() => OnGameOver?.Invoke();
    public static void LevelComplete() => OnLevelComplete?.Invoke();
    
    public static void EnemySpawned(GameObject enemy) => OnEnemySpawned?.Invoke(enemy);
    public static void EnemyDestroyed(GameObject enemy) => OnEnemyDestroyed?.Invoke(enemy);
    public static void EnemyCountChanged(int count) => OnEnemyCountChanged?.Invoke(count);
    
    public static void AmmoPickup(int amount) => OnAmmoPickup?.Invoke(amount);
    public static void HealthPickup(int amount) => OnHealthPickup?.Invoke(amount);
    
    public static void NetworkEvent(string eventType, object data) => OnNetworkEvent?.Invoke(eventType, data);
}
