# GameEvents Enhanced Debugging Guide

This guide explains how to use the enhanced GameEvents system for debugging and monitoring event flow in Zombtoy.

## Enhanced GameEvents Features

### Debug Logging
The enhanced `GameEvents.cs` now provides automatic logging when events are fired:

```csharp
[GameEvents] EnemyKilled fired, subscribers=1, score=100
[GameEvents] EnemySpawned fired, subscribers=1, enemy=Zombunny(Clone)  
[GameEvents] PlayerDeath fired, subscribers=2
```

### Safe Invocation
Use `SafeInvoke` methods to prevent one bad event handler from breaking others:

```csharp
// Instead of: OnEnemyKilled?.Invoke(score, position);
GameEvents.SafeInvoke(OnEnemyKilled, score, position, "EnemyKilled");
```

### Subscriber Count Checking
Check how many listeners are subscribed to an event at runtime:

```csharp
int count = GameEvents.GetSubscriberCount(OnEnemyKilled);
Debug.Log($"EnemyKilled has {count} subscribers");
```

## Debugging Workflow

### 1. Generate Debug Report
Run the GameEvents analysis to find potential issues:

```bash
cd DevTools/Diagrams  
python3 generate_gameevents_debug.py
```

This creates:
- `out/gameevents_debug_report.md` - Detailed text analysis
- `out/gameevents_health.puml` - Visual health status diagram

### 2. Review Common Issues

**Memory Leak Risks** 🔥
- Classes that subscribe (`+=`) but never unsubscribe (`-=`)
- Look for missing `OnDisable()` or `OnDestroy()` methods

**Dead Events** ⚠️  
- Events with no subscribers (wasted computation)
- Events that are never fired (unused code)

**Lifecycle Issues** ❓
- Subscribing in `OnEnable()` without `OnDisable()`
- Subscribing in `Start()` without `OnDestroy()`

### 3. Monitor Runtime Behavior
Enable debug logging in GameEvents to see real-time event activity:

```csharp  
// Example output in Console:
[GameEvents] EnemySpawned fired, subscribers=1, enemy=Zombunny(Clone)
[GameEvents] EnemyKilled fired, subscribers=1, score=100
```

### 4. Fix Common Patterns

**Subscription/Unsubscription Pattern:**
```csharp
void OnEnable()
{
    GameEvents.OnEnemyKilled += HandleEnemyKilled;
}

void OnDisable() 
{
    GameEvents.OnEnemyKilled -= HandleEnemyKilled;
}
```

**Safe Event Publishing:**
```csharp
void FireEvent(int score, Vector3 position)
{
    var count = GameEvents.GetSubscriberCount(GameEvents.OnEnemyKilled);
    if (count == 0)
    {
        Debug.LogWarning("No subscribers for EnemyKilled event");
        return; 
    }
    
    GameEvents.EnemyKilled(score, position);
}
```

## Visual Analysis

### Health Color Coding
- 🟢 **Green**: Healthy events (has publishers & subscribers)
- 🔴 **Red**: Dead events (no subscribers) 
- 🟠 **Orange**: Unused events (never fired)
- 🔵 **Blue**: Potential bottlenecks (1 subscriber, many publishers)

### Reading the Report
The debug report shows:
- **Events Overview**: Each event with publisher/subscriber counts
- **Class Interactions**: Which classes use which events
- **Issues**: Specific problems found with suggestions

## Best Practices

### Event Naming
- Events: `OnSomethingHappened` (past tense)
- Triggers: `SomethingHappened()` (matches event name minus "On")

### Lifecycle Management
- Subscribe in `OnEnable()`, unsubscribe in `OnDisable()`
- Or subscribe in `Start()`, unsubscribe in `OnDestroy()`
- Never mix patterns (e.g., subscribe in `Start()`, unsubscribe in `OnDisable()`)

### Performance
- Events with many subscribers can be expensive
- Consider caching subscriber counts for hot paths
- Use `SafeInvoke` for critical events that must not fail

### Testing
- Run debug analysis regularly during development
- Monitor console for subscription warnings
- Use health visualization to spot architectural problems

## Integration with Core Architecture

The enhanced GameEvents work together with the Core architecture:
- `GameStateManager` coordinates state changes via events
- `EnemyManager` publishes spawn/destroy events  
- `ScoreManager` subscribes to score-affecting events
- All managers follow consistent subscription patterns

This creates a decoupled, observable, and debuggable event-driven architecture.
