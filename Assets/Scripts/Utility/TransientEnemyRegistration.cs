using UnityEngine;

/// <summary>
/// Lightweight helper to register/deregister GameObjects that don't have an EnemyHealth
/// so the centralized EnemyManager (and zombieCount) stays correct.
/// Attach this to runtime-spawned objects that need to be tracked but lack EnemyHealth.
/// </summary>
public class TransientEnemyRegistration : MonoBehaviour
{
    void Awake()
    {
        GameEvents.EnemySpawned(gameObject);
    }

    void OnDestroy()
    {
        GameEvents.EnemyDestroyed(gameObject);
    }
}
