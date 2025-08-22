# Core Architecture Report
Generated: generate_core_architecture.py

## Overview
- Core Components: 5
- Managers: 9
- Singletons: 4

## Core Components

### ComponentCache
- **File**: `Core/ComponentCache.cs`
- **Type**: Component
- **MonoBehaviour**: Yes
- **Lifecycle Methods**: Awake, OnDestroy

### GameEvents
- **File**: `Core/GameEvents.cs`
- **Type**: Static
- **MonoBehaviour**: No
- **Lifecycle Methods**: None

### GameStarter
- **File**: `Core/GameStarter.cs`
- **Type**: Component
- **MonoBehaviour**: Yes
- **Lifecycle Methods**: Start
- **Dependencies**: bool, bool
- **Patterns**: Uses Singletons

### GameStateManager
- **File**: `Core/GameStateManager.cs`
- **Type**: Singleton
- **MonoBehaviour**: No
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Dependencies**: GameState, PlayerHealth, PlayerHealth
- **Patterns**: Uses GameEvents

### Singleton
- **File**: `Core/Singleton.cs`
- **Type**: Component
- **MonoBehaviour**: No
- **Lifecycle Methods**: Awake, OnDestroy
- **Patterns**: Persistent

## Managers

### EnemyManager
- **File**: `Managers/EnemyManager.cs`
- **Type**: Singleton
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Dependencies**: private, private, float, float, float, int, PlayerHealth, zombieCount, PlayerHealth, PlayerHealthRefactored
- **Patterns**: Uses GameEvents

### GameOverManager
- **File**: `Managers/GameOverManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Dependencies**: PlayerHealth, Animator, PlayerHealth
- **Patterns**: Uses GameEvents, Uses Singletons

### ItemManager
- **File**: `Managers/ItemManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Start

### MusicManager
- **File**: `UI/MusicManager.cs`
- **Type**: Singleton
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Dependencies**: AudioMixer, AudioSource, AudioSource, Slider, Toggle
- **Patterns**: Uses GameEvents

### PlayerInputManager
- **File**: `Player/PlayerInputManager.cs`
- **Type**: Component
- **Lifecycle Methods**: None
- **Dependencies**: bool, bool, string, string, KeyCode, string, string, KeyCode, KeyCode, KeyCode, KeyCode

### SFXManager
- **File**: `SFXManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Start

### ScoreManager
- **File**: `Managers/ScoreManager.cs`
- **Type**: Singleton
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Dependencies**: Text, GameObject
- **Patterns**: Uses GameEvents, Uses Singletons

### ScoreManagerDebugger
- **File**: `Debug/ScoreManagerDebugger.cs`
- **Type**: Component
- **Lifecycle Methods**: Start
- **Dependencies**: bool, bool
- **Patterns**: Uses Singletons

### WeaponManager
- **File**: `Weapons/WeaponManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Awake, Start, OnDestroy
- **Dependencies**: private, int, bool, float, private
- **Patterns**: Uses GameEvents

## Architectural Patterns

### Singleton Usage
- **EnemyManager**: `EnemyManager`
- **GameStateManager**: `GameStateManager`
- **MusicManager**: `MusicManager`
- **ScoreManager**: `ScoreManager`

### Event Bus Integration

- **GameStateManager**: Integrates with GameEvents
- **MusicManager**: Integrates with GameEvents
- **EnemyManager**: Integrates with GameEvents
- **GameOverManager**: Integrates with GameEvents
- **ScoreManager**: Integrates with GameEvents
- **WeaponManager**: Integrates with GameEvents