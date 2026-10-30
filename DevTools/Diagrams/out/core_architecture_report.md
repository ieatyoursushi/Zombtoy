# Core Architecture Report
Generated: generate_core_architecture.py
Static source analysis only; scene/prefab wiring, dynamic dispatch and runtime execution order are not inferred.

## Overview
- Core Components: 2
- Managers: 6
- Singletons: 3

## Core Components

### GameEvents
- **File**: `Core/GameEvents.cs`
- **Type**: Static
- **MonoBehaviour**: No
- **Lifecycle Methods**: None

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
- **Dependencies**: private, private, float, float, float, int, PlayerHealth, zombieCount, PlayerHealth
- **Patterns**: Uses GameEvents, Persistent

### ItemManager
- **File**: `Managers/ItemManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Start

### MusicManager
- **File**: `UI/MusicManager.cs`
- **Type**: Singleton
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Dependencies**: AudioMixer, private, AudioSource, AudioSource, Slider, Toggle
- **Patterns**: Uses GameEvents, Persistent

### SFXManager
- **File**: `SFXManager.cs`
- **Type**: Component
- **Lifecycle Methods**: Start

### ScoreManager
- **File**: `Managers/ScoreManager.cs`
- **Type**: Singleton
- **Lifecycle Methods**: Awake, Start, OnEnable, OnDisable, OnDestroy
- **Dependencies**: Text, GameObject
- **Patterns**: Uses GameEvents, Persistent

### ScoreManagerDebugger
- **File**: `Debug/ScoreManagerDebugger.cs`
- **Type**: Component
- **Lifecycle Methods**: Start
- **Dependencies**: bool, bool
- **Patterns**: Uses Singletons

## Architectural Patterns

### Singleton Usage
- **EnemyManager**: `EnemyManager`
- **MusicManager**: `MusicManager`
- **ScoreManager**: `ScoreManager`

### Event Bus Integration

- **EnemyManager**: Integrates with GameEvents
- **MusicManager**: Integrates with GameEvents
- **ScoreManager**: Integrates with GameEvents