# GameEvents Debug Report
Generated: generate_gameevents_debug.py

## Summary
- Total Events: 20
- Classes with Event Interactions: 13
- Issues Found: 19

## 🚨 Issues Detected

- ⚠️  Event 'OnPlayerHealthChanged' has no subscribers (dead event)
- ⚠️  Event 'OnPlayerStaminaChanged' has no subscribers (dead event)
- ⚠️  Event 'OnPlayerStaminaChanged' is never fired (unused event)
- ⚠️  Event 'OnEnemyDamaged' has no subscribers (dead event)
- ⚠️  Event 'OnWeaponFired' has no subscribers (dead event)
- ⚠️  Event 'OnScoreChanged' has no subscribers (dead event)
- ⚠️  Event 'OnHighScoreChanged' has no subscribers (dead event)
- ⚠️  Event 'OnGamePaused' is never fired (unused event)
- ⚠️  Event 'OnGameResumed' is never fired (unused event)
- ⚠️  Event 'OnGameOver' is never fired (unused event)
- ⚠️  Event 'OnLevelComplete' has no subscribers (dead event)
- ⚠️  Event 'OnLevelComplete' is never fired (unused event)
- ⚠️  Event 'OnEnemyCountChanged' has no subscribers (dead event)
- ⚠️  Event 'OnAmmoPickup' has no subscribers (dead event)
- ⚠️  Event 'OnAmmoPickup' is never fired (unused event)
- ⚠️  Event 'OnHealthPickup' has no subscribers (dead event)
- ⚠️  Event 'OnHealthPickup' is never fired (unused event)
- ⚠️  Event 'OnNetworkEvent' has no subscribers (dead event)
- ⚠️  Event 'OnNetworkEvent' is never fired (unused event)

## 📡 Events Overview

### OnAmmoPickup
- **Type**: `Action<int>`
- **Trigger Method**: `AmmoPickup`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnEnemyCountChanged
- **Type**: `Action<int>`
- **Trigger Method**: `EnemyCountChanged`
- **Publishers**: 4 (EnemyManager, EnemyManager, EnemyManager, EnemyManager)
- **Subscribers**: 0 ()

### OnEnemyDamaged
- **Type**: `Action<GameObject, int, Vector3>`
- **Trigger Method**: `EnemyDamaged`
- **Publishers**: 1 (RaycastWeapon)
- **Subscribers**: 0 ()

### OnEnemyDestroyed
- **Type**: `Action<GameObject>`
- **Trigger Method**: `EnemyDestroyed`
- **Publishers**: 1 (EnemyHealth)
- **Subscribers**: 1 (EnemyManager.UnregisterEnemy)

### OnEnemyKilled
- **Type**: `Action<int, Vector3>`
- **Trigger Method**: `EnemyKilled`
- **Publishers**: 1 (EnemyHealth)
- **Subscribers**: 1 (ScoreManager.HandleEnemyKilled)

### OnEnemySpawned
- **Type**: `Action<GameObject>`
- **Trigger Method**: `EnemySpawned`
- **Publishers**: 3 (EnemyManager, EnemyManager, EnemyHealth)
- **Subscribers**: 1 (EnemyManager.RegisterEnemy)

### OnGameOver
- **Type**: `Action`
- **Trigger Method**: `GameOver`
- **Publishers**: 0 ()
- **Subscribers**: 3 (GameOverManager.HandleGameOver, ScoreManager.SaveHighScore, ScoreManager.CaptureRunStats)

### OnGamePaused
- **Type**: `Action`
- **Trigger Method**: `GamePaused`
- **Publishers**: 0 ()
- **Subscribers**: 3 (EnemyManager.StopSpawning, WeaponManager.HandleGamePaused, PlayerHealthRefactored.HandleGamePaused)

### OnGameResumed
- **Type**: `Action`
- **Trigger Method**: `GameResumed`
- **Publishers**: 0 ()
- **Subscribers**: 2 (EnemyManager.StartSpawning, PlayerHealthRefactored.HandleGameResumed)

### OnGameStarted
- **Type**: `Action`
- **Trigger Method**: `GameStarted`
- **Publishers**: 1 (ScoreManager)
- **Subscribers**: 1 (EnemyManager.HandleGameStarted)

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

### OnNetworkEvent
- **Type**: `Action<string, object>`
- **Trigger Method**: `NetworkEvent`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnPlayerDeath
- **Type**: `Action`
- **Trigger Method**: `PlayerDeath`
- **Publishers**: 2 (PlayerHealthRefactored, PlayerHealth)
- **Subscribers**: 3 (EnemyManager.StopSpawning, GameOverManager.HandlePlayerDeath, WeaponManager.HandlePlayerDeath)

### OnPlayerHealthChanged
- **Type**: `Action<int>`
- **Trigger Method**: `PlayerHealthChanged`
- **Publishers**: 6 (PlayerHealthRefactored, PlayerHealthRefactored, PlayerHealthRefactored, PlayerHealthRefactored, PlayerHealth, PlayerHealth)
- **Subscribers**: 0 ()

### OnPlayerRevive
- **Type**: `Action`
- **Trigger Method**: `PlayerRevive`
- **Publishers**: 1 (PlayerHealthRefactored)
- **Subscribers**: 1 (EnemyManager.HandlePlayerRevive)

### OnPlayerStaminaChanged
- **Type**: `Action<int>`
- **Trigger Method**: `PlayerStaminaChanged`
- **Publishers**: 0 ()
- **Subscribers**: 0 ()

### OnScoreChanged
- **Type**: `Action<int>`
- **Trigger Method**: `ScoreChanged`
- **Publishers**: 2 (ScoreManager, ScoreManager)
- **Subscribers**: 0 ()

### OnWeaponFired
- **Type**: `Action<Vector3, int>`
- **Trigger Method**: `WeaponFired`
- **Publishers**: 1 (WeaponData)
- **Subscribers**: 0 ()

## 🏗️ Class Event Interactions

### EnemyHealth
- **File**: `Enemy/EnemyHealth.cs`
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Publishes**: EnemySpawned, EnemyKilled, EnemyDestroyed

### EnemyManager
- **File**: `Managers/EnemyManager.cs`
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Subscribes To**: OnEnemySpawned(RegisterEnemy), OnEnemyDestroyed(UnregisterEnemy), OnGameStarted(HandleGameStarted), OnPlayerDeath(StopSpawning), OnPlayerRevive(HandlePlayerRevive), OnGamePaused(StopSpawning), OnGameResumed(StartSpawning)
- **Publishes**: EnemyCountChanged, EnemySpawned, EnemySpawned, EnemyCountChanged, EnemyCountChanged, EnemyCountChanged

### GameOverManager
- **File**: `Managers/GameOverManager.cs`
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Subscribes To**: OnPlayerDeath(HandlePlayerDeath), OnGameOver(HandleGameOver)

### GameStateManager
- **File**: `Core/GameStateManager.cs`
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Subscribes To**: OnPlayerDeath(HandlePlayerDeath), OnGamePaused(HandleGamePaused), OnGameResumed(HandleGameResumed)
- **Publishes**: GameOver, GamePaused, GameResumed

### MusicManager
- **File**: `UI/MusicManager.cs`
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Subscribes To**: OnGamePaused(HandleGamePaused), OnGameResumed(HandleGameResumed)

### PlayerHealth
- **File**: `Player/PlayerHealth.cs`
- **Lifecycle Methods**: Awake, Start
- **Publishes**: PlayerHealthChanged, PlayerHealthChanged, PlayerDeath

### PlayerHealthRefactored
- **File**: `Player/PlayerHealthRefactored.cs`
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Subscribes To**: OnGamePaused(HandleGamePaused), OnGameResumed(HandleGameResumed)
- **Publishes**: PlayerHealthChanged, PlayerHealthChanged, PlayerHealthChanged, PlayerDeath, PlayerRevive, PlayerHealthChanged

### RaycastWeapon
- **File**: `Weapons/RaycastWeapon.cs`
- **Lifecycle Methods**: Awake
- **Publishes**: EnemyDamaged

### ScoreManager
- **File**: `Managers/ScoreManager.cs`
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Subscribes To**: OnEnemyKilled(HandleEnemyKilled), OnGameOver(SaveHighScore), OnGameOver(CaptureRunStats)
- **Publishes**: ScoreChanged, ScoreChanged, GameStarted, HighScoreChanged

### ScoreTextBinder
- **File**: `UI/ScoreTextBinder.cs`
- **Lifecycle Methods**: Awake, OnEnable, OnDisable
- **Subscribes To**: OnScoreChanged(HandleScoreChanged)

### WeaponData
- **File**: `Weapons/WeaponSystem.cs`
- **Lifecycle Methods**: Awake, OnDestroy
- **Publishes**: WeaponFired

### WeaponManager
- **File**: `Weapons/WeaponManager.cs`
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Subscribes To**: OnGamePaused(HandleGamePaused), OnPlayerDeath(HandlePlayerDeath)

### ZombieCountBinder
- **File**: `UI/ZombieCountBinder.cs`
- **Lifecycle Methods**: Awake, OnEnable, OnDisable
- **Subscribes To**: OnEnemyCountChanged(HandleCountChanged)
