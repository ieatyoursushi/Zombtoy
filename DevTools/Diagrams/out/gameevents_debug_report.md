# GameEvents Debug Report
Generated: generate_gameevents_debug.py
Static source analysis only; scene/prefab wiring, dynamic dispatch and runtime execution order are not inferred.
Findings are source-reference observations, not proof of leaks or gameplay activity.

## Summary
- Total Events: 19
- Classes with Event Interactions: 8
- Issues Found: 18

## 🚨 Issues Detected

- Event 'OnAmmoPickup' has no source subscriptions
- Event 'OnAmmoPickup' has no source publishers
- Event 'OnEnemyDamaged' has no source subscriptions
- Event 'OnEnemyDamaged' has no source publishers
- Event 'OnGameOver' has no source publishers
- Event 'OnGamePaused' has no source publishers
- Event 'OnGameResumed' has no source publishers
- Event 'OnHealthPickup' has no source subscriptions
- Event 'OnHealthPickup' has no source publishers
- Event 'OnHighScoreChanged' has no source subscriptions
- Event 'OnLevelComplete' has no source subscriptions
- Event 'OnLevelComplete' has no source publishers
- Event 'OnPlayerHealthChanged' has no source subscriptions
- Event 'OnPlayerRevive' has no source publishers
- Event 'OnPlayerStaminaChanged' has no source subscriptions
- Event 'OnPlayerStaminaChanged' has no source publishers
- Event 'OnWeaponFired' has no source subscriptions
- Event 'OnWeaponFired' has no source publishers

## 📡 Events Overview

### OnAmmoPickup
- **Type**: `Action<int>`
- **Trigger Method**: `AmmoPickup`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnEnemyCountChanged
- **Type**: `Action<int>`
- **Trigger Method**: `EnemyCountChanged`
- **Publishers**: 1 (EnemyManager)
- **Subscribers**: 1 (ZombieCountBinder.HandleCountChanged)

### OnEnemyDamaged
- **Type**: `Action<GameObject, int, Vector3>`
- **Trigger Method**: `EnemyDamaged`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnEnemyDestroyed
- **Type**: `Action<GameObject>`
- **Trigger Method**: `EnemyDestroyed`
- **Publishers**: 2 (EnemyHealth, TransientEnemyRegistration)
- **Subscribers**: 1 (EnemyManager.UnregisterEnemy)

### OnEnemyKilled
- **Type**: `Action<int, Vector3>`
- **Trigger Method**: `EnemyKilled`
- **Publishers**: 1 (EnemyHealth)
- **Subscribers**: 1 (ScoreManager.HandleEnemyKilled)

### OnEnemySpawned
- **Type**: `Action<GameObject>`
- **Trigger Method**: `EnemySpawned`
- **Publishers**: 3 (EnemyHealth, EnemyManager, TransientEnemyRegistration)
- **Subscribers**: 1 (EnemyManager.RegisterEnemy)

### OnGameOver
- **Type**: `Action`
- **Trigger Method**: `GameOver`
- **Publishers**: 0 ()
- **Subscribers**: 3 (MusicManager.HandleGameEnded, ScoreManager.CaptureRunStats, ScoreManager.SaveHighScore)

### OnGamePaused
- **Type**: `Action`
- **Trigger Method**: `GamePaused`
- **Publishers**: 0 ()
- **Subscribers**: 2 (EnemyManager.StopSpawning, MusicManager.HandleGamePaused)

### OnGameResumed
- **Type**: `Action`
- **Trigger Method**: `GameResumed`
- **Publishers**: 0 ()
- **Subscribers**: 2 (EnemyManager.StartSpawning, MusicManager.HandleGameResumed)

### OnGameStarted
- **Type**: `Action`
- **Trigger Method**: `GameStarted`
- **Publishers**: 1 (ScoreManager)
- **Subscribers**: 2 (EnemyManager.HandleGameStarted, MusicManager.HandleGameStarted)

### OnHealthPickup
- **Type**: `Action<int>`
- **Trigger Method**: `HealthPickup`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnHighScoreChanged
- **Type**: `Action<int>`
- **Trigger Method**: `HighScoreChanged`
- **Publishers**: 1 (ScoreManager)
- **Subscribers**: 0 ()

### OnLevelComplete
- **Type**: `Action`
- **Trigger Method**: `LevelComplete`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnPlayerDeath
- **Type**: `Action`
- **Trigger Method**: `PlayerDeath`
- **Publishers**: 1 (PlayerHealth)
- **Subscribers**: 2 (EnemyManager.StopSpawning, MusicManager.HandleGameEnded)

### OnPlayerHealthChanged
- **Type**: `Action<int>`
- **Trigger Method**: `PlayerHealthChanged`
- **Publishers**: 1 (PlayerHealth)
- **Subscribers**: 0 ()

### OnPlayerRevive
- **Type**: `Action`
- **Trigger Method**: `PlayerRevive`
- **Publishers**: 0 ()
- **Subscribers**: 1 (EnemyManager.HandlePlayerRevive)

### OnPlayerStaminaChanged
- **Type**: `Action<int>`
- **Trigger Method**: `PlayerStaminaChanged`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnScoreChanged
- **Type**: `Action<int>`
- **Trigger Method**: `ScoreChanged`
- **Publishers**: 1 (ScoreManager)
- **Subscribers**: 1 (ScoreTextBinder.HandleScoreChanged)

### OnWeaponFired
- **Type**: `Action<Vector3, int>`
- **Trigger Method**: `WeaponFired`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

## 🏗️ Class Event Interactions

### EnemyHealth
- **File**: `Enemy/EnemyHealth.cs`
- **Lifecycle Methods**: Awake, OnDestroy, Start
- **Publishes**: OnEnemyDestroyed, OnEnemyKilled, OnEnemySpawned

### EnemyManager
- **File**: `Managers/EnemyManager.cs`
- **Lifecycle Methods**: Awake, OnDestroy, OnDisable, OnEnable, Start
- **Subscribes To**: OnEnemyDestroyed(UnregisterEnemy), OnEnemySpawned(RegisterEnemy), OnGamePaused(StopSpawning), OnGameResumed(StartSpawning), OnGameStarted(HandleGameStarted), OnPlayerDeath(StopSpawning), OnPlayerRevive(HandlePlayerRevive)
- **Publishes**: OnEnemyCountChanged, OnEnemySpawned

### MusicManager
- **File**: `UI/MusicManager.cs`
- **Lifecycle Methods**: Awake, OnDestroy, OnDisable, OnEnable, Start
- **Subscribes To**: OnGameOver(HandleGameEnded), OnGamePaused(HandleGamePaused), OnGameResumed(HandleGameResumed), OnGameStarted(HandleGameStarted), OnPlayerDeath(HandleGameEnded)

### PlayerHealth
- **File**: `Player/PlayerHealth.cs`
- **Lifecycle Methods**: Awake, Start
- **Publishes**: OnPlayerDeath, OnPlayerHealthChanged

### ScoreManager
- **File**: `Managers/ScoreManager.cs`
- **Lifecycle Methods**: Awake, OnDestroy, OnDisable, OnEnable, Start
- **Subscribes To**: OnEnemyKilled(HandleEnemyKilled), OnGameOver(CaptureRunStats), OnGameOver(SaveHighScore)
- **Publishes**: OnGameStarted, OnHighScoreChanged, OnScoreChanged

### ScoreTextBinder
- **File**: `UI/ScoreTextBinder.cs`
- **Lifecycle Methods**: Awake, OnDisable, OnEnable
- **Subscribes To**: OnScoreChanged(HandleScoreChanged)

### TransientEnemyRegistration
- **File**: `Utility/TransientEnemyRegistration.cs`
- **Lifecycle Methods**: Awake, OnDestroy
- **Publishes**: OnEnemyDestroyed, OnEnemySpawned

### ZombieCountBinder
- **File**: `UI/ZombieCountBinder.cs`
- **Lifecycle Methods**: Awake, OnDisable, OnEnable
- **Subscribes To**: OnEnemyCountChanged(HandleCountChanged)
