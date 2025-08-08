# Zombtoy Project - Code Refactor Plan

## Overview
This document outlines architectural flaws and planned refactorings for the Zombtoy Unity game project. The codebase was written during sophomore year of high school before formal CS education, so there are several anti-patterns and architectural issues that need addressing before implementing backend features.

---

## 🚨 Critical Architectural Issues

### 1. **Excessive GameObject.Find() Usage**
**Problem**: Runtime performance impact and fragile references
**Files Affected**: 
- `PlayerHealth.cs` (lines 40, 63, 198)
- `Pause.cs` (line 12)
- `EnemyHealth.cs` (line 44, 45)

**Current Code**:
```csharp
StaminaSliderWhole = GameObject.Find("StaminaSlider");
firstperson = GameObject.Find("FirstPerson");
GameObject.Find("Fill").GetComponent<Image>().color = Color.white;
```

**Refactor Plan**:
- Replace with proper dependency injection or SerializeField references
- Use events/observers for loose coupling
- Implement a service locator pattern for commonly accessed objects

### 2. **Monolithic Update() Methods**
**Problem**: Poor performance, hard to debug, mixed responsibilities
**Files Affected**:
- `PlayerHealth.cs` - 40+ line Update() method
- `Inventory.cs` - Massive Update() with nested conditionals
- `GameOverManager.cs` - Checking health every frame

**Current Issues**:
```csharp
// GameOverManager.cs - Checking every frame unnecessarily
void Update() {
    if (playerHealth.currentHealth <= 0) {
        anim.SetTrigger("GameOver");
    }
}
```

**Refactor Plan**:
- Use events instead of polling (PlayerHealth.OnDeath event)
- Cache frequently accessed components
- Move non-frame dependent logic out of Update()

### 3. **God Object Anti-Pattern**
**Problem**: Single classes doing too much
**Primary Offender**: `PlayerHealth.cs`

**Current Responsibilities**:
- Health management
- Stamina system
- Sprint mechanics
- Death handling
- UI updates
- Audio management
- Animation control

**Refactor Plan**:
```csharp
// Split into focused components:
- PlayerHealth (health only)
- PlayerStamina (stamina/sprint)
- PlayerAudioManager (sounds)
- PlayerAnimationController (animations)
- PlayerUIManager (health/stamina UI)
```

### 4. **Tight Coupling Issues**
**Problem**: Classes directly accessing each other's internals
**Examples**:
- `GameOverManager` directly checking `playerHealth.currentHealth`
- `Inventory` directly manipulating multiple GameObjects
- `EnemyMovement` directly accessing player transform via GameObject.Find

**Refactor Plan**:
- Implement event-driven architecture
- Use interfaces for contracts
- Create proper abstraction layers

---

## 🔧 Specific Refactoring Tasks

### **Phase 1: Dependency Injection & References**

#### 1.1 Replace GameObject.Find() Calls
```csharp
// Before
StaminaSliderWhole = GameObject.Find("StaminaSlider");

// After
[SerializeField] private GameObject staminaSliderWhole;
// Or use dependency injection container
```

#### 1.2 Component Caching
```csharp
// Create ComponentCache.cs
public class ComponentCache : MonoBehaviour {
    private readonly Dictionary<Type, Component> _cache = new();
    
    public T GetCachedComponent<T>() where T : Component {
        if (!_cache.TryGetValue(typeof(T), out var component)) {
            component = GetComponent<T>();
            _cache[typeof(T)] = component;
        }
        return component as T;
    }
}
```

### **Phase 2: Event-Driven Architecture**

#### 2.1 Health Events System
```csharp
// PlayerHealth.cs
public static event System.Action<int> OnHealthChanged;
public static event System.Action OnPlayerDeath;
public static event System.Action OnPlayerHeal;

private void TakeDamage(int damage) {
    currentHealth -= damage;
    OnHealthChanged?.Invoke(currentHealth);
    
    if (currentHealth <= 0) {
        OnPlayerDeath?.Invoke();
    }
}
```

#### 2.2 Game State Manager
```csharp
// GameStateManager.cs
public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance;
    
    void Awake() {
        if (Instance == null) Instance = this;
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }
    
    private void HandlePlayerDeath() {
        // Centralized game over logic
    }
}
```

### **Phase 3: Component Separation**

#### 3.1 Split PlayerHealth
```csharp
// PlayerHealth.cs (simplified)
public class PlayerHealth : MonoBehaviour {
    [SerializeField] private int startingHealth = 100;
    [SerializeField] private int currentHealth;
    
    public static event System.Action<int> OnHealthChanged;
    public static event System.Action OnPlayerDeath;
    
    // Only health-related logic here
}

// PlayerStaminaSystem.cs
public class PlayerStaminaSystem : MonoBehaviour {
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float currentStamina;
    
    public static event System.Action<float> OnStaminaChanged;
    
    // Only stamina-related logic here
}
```

#### 3.2 UI Management
```csharp
// PlayerUIManager.cs
public class PlayerUIManager : MonoBehaviour {
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image damageOverlay;
    
    void Awake() {
        PlayerHealth.OnHealthChanged += UpdateHealthUI;
        PlayerStaminaSystem.OnStaminaChanged += UpdateStaminaUI;
    }
}
```

---

## 🌐 Backend Integration Readiness

### **Current State Assessment**
❌ **Not Ready** - Too many tight couplings and monolithic structures

### **Required Changes for Multiplayer**

#### 1. **Data Serialization**
```csharp
// Create serializable data structures
[System.Serializable]
public struct PlayerState {
    public Vector3 position;
    public Quaternion rotation;
    public int health;
    public float stamina;
    public int activeWeaponIndex;
}

[System.Serializable]
public struct GameState {
    public PlayerState[] players;
    public EnemyState[] enemies;
    public float gameTime;
    public int waveNumber;
}
```

#### 2. **Network-Safe Components**
```csharp
// NetworkPlayerHealth.cs
public class NetworkPlayerHealth : PlayerHealth, INetworkBehaviour {
    [SerializeField] private bool isLocalPlayer;
    
    public override void TakeDamage(int damage) {
        if (isLocalPlayer) {
            // Send to server
            NetworkManager.SendDamageEvent(damage);
        }
        base.TakeDamage(damage);
    }
}
```

#### 3. **State Synchronization**
```csharp
// PlayerStateSynchronizer.cs
public class PlayerStateSynchronizer : MonoBehaviour {
    private PlayerHealth health;
    private PlayerStaminaSystem stamina;
    
    public PlayerState GetCurrentState() {
        return new PlayerState {
            position = transform.position,
            health = health.CurrentHealth,
            stamina = stamina.CurrentStamina,
            // etc.
        };
    }
    
    public void ApplyState(PlayerState state) {
        transform.position = state.position;
        health.SetHealth(state.health);
        stamina.SetStamina(state.stamina);
    }
}
```

---

## 📋 Implementation Priority

### **High Priority** (Before Backend)
1. ✅ Remove all GameObject.Find() calls
2. ✅ Implement event-driven health system
3. ✅ Split PlayerHealth monolith
4. ✅ Create proper UI management layer
5. ✅ Fix Update() method performance issues

### **Medium Priority** (During Backend)
1. 🔄 Implement network-safe state management
2. 🔄 Create serializable data structures
3. 🔄 Add input validation and sanitization
4. 🔄 Implement client-side prediction

### **Low Priority** (Post-Backend)
1. ⏳ Advanced networking features
2. ⏳ Performance optimizations
3. ⏳ Advanced anti-cheat measures

---

## 🎯 Performance Improvements

### **Current Issues**
- Multiple Update() methods checking conditions every frame
- Excessive GetComponent() calls
- No object pooling for projectiles/enemies
- UI updates happening in Update() instead of events

### **Optimization Plan**
```csharp
// ObjectPoolManager.cs
public class ObjectPoolManager : MonoBehaviour {
    private Dictionary<string, Queue<GameObject>> pools = new();
    
    public GameObject GetPooledObject(string tag) {
        if (pools[tag].Count > 0) {
            return pools[tag].Dequeue();
        }
        return null; // Create new if pool empty
    }
}

// PerformanceMonitor.cs
public class PerformanceMonitor : MonoBehaviour {
    private void Update() {
        if (1f / Time.deltaTime < 30f) {
            Debug.LogWarning("Frame rate dropped below 30 FPS");
        }
    }
}
```

---

## 🧪 Testing Strategy

### **Unit Tests Needed**
```csharp
// PlayerHealthTests.cs
[TestFixture]
public class PlayerHealthTests {
    [Test]
    public void TakeDamage_ReducesHealth() {
        // Test health reduction logic
    }
    
    [Test]
    public void TakeDamage_TriggersDeathWhenHealthZero() {
        // Test death event triggering
    }
}
```

### **Integration Tests**
- Health system + UI updates
- Weapon switching + inventory management
- Enemy AI + player interaction

---

## 📝 Code Quality Improvements

### **Naming Conventions**
```csharp
// Bad
public reloadCheck machineGunCheck; // lowercase class name
bool played = false; // vague variable name
public GameObject RocketLaunche; // typo

// Good
public ReloadCheck machineGunReloadCheck;
private bool hasMachineGunSoundPlayed = false;
public GameObject rocketLauncher;
```

### **Documentation Standards**
```csharp
/// <summary>
/// Manages player health, including damage, healing, and death events.
/// Triggers appropriate UI updates and game state changes.
/// </summary>
public class PlayerHealth : MonoBehaviour {
    /// <summary>
    /// The maximum health the player can have.
    /// </summary>
    [SerializeField] private int maxHealth = 100;
}
```

---

## 🎮 Backend-Specific Considerations

### **Data That Needs Synchronization**
- Player position/rotation
- Health/stamina values
- Active weapon
- Inventory state
- Score/progression
- Enemy states (in multiplayer)

### **Client-Server Architecture**
- **Client**: Input handling, prediction, rendering
- **Server**: Authoritative game state, validation, physics
- **Communication**: Real-time updates via SignalR/WebSockets

### **Security Considerations**
- Server-side validation for all player actions
- Anti-cheat measures for movement/shooting
- Secure score submission
- Rate limiting for API calls

---

## 📊 Estimated Refactor Timeline

| Phase | Duration | Dependencies |
|-------|----------|--------------|
| Dependency Injection | 1-2 days | None |
| Event System | 2-3 days | DI complete |
| Component Splitting | 3-4 days | Events ready |
| Network Layer Prep | 2-3 days | Architecture solid |
| **Total** | **8-12 days** | Progressive |

---

*This refactor plan should be implemented progressively, with each phase building on the previous one. The goal is to create a clean, maintainable, and network-ready codebase suitable for multiplayer backend integration.*
