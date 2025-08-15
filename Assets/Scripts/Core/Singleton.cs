using UnityEngine;

/// <summary>
/// Generic singleton pattern for MonoBehaviour managers
/// Thread-safe and null-safe implementation
/// </summary>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting = false;
    private static bool _hasWarnedMissing = false;

    // Override in subclasses to control behavior
    protected virtual bool Persistent => true;          // If true, survives scene loads
    protected virtual bool AllowAutoCreate => false;     // If true, will create an instance if missing - DEFAULT FALSE for safety
    protected virtual bool LogCreation => true;          // Toggle creation log
    
    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
                return null;
            }
            
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));
                    
                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError($"[Singleton] Something went really wrong - there should never be more than 1 singleton! Reopening the scene might fix it.");
                        return _instance;
                    }
                    
                    if (_instance == null)
                    {
                        // Don't auto-create - let the manager be placed explicitly in scene
                        // Only warn once per type to avoid spam
                        if (!_hasWarnedMissing)
                        {
                            Debug.LogWarning($"[Singleton] No instance of {typeof(T)} found in scene. Add one manually to the scene or enable AllowAutoCreate.");
                            _hasWarnedMissing = true;
                        }
                    }
                }
                
                return _instance;
            }
        }
    }
    
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (Persistent)
            {
                bool isUnderCanvas = GetComponentInParent<Canvas>() != null;
                if (!isUnderCanvas && transform.parent != null)
                {
                    transform.SetParent(null);
                }
                if (!isUnderCanvas)
                {
                    DontDestroyOnLoad(gameObject);
                    Debug.Log($"[Singleton] {typeof(T).Name} instance '{gameObject.name}' marked as DontDestroyOnLoad");
                }
            }
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] Multiple instances of {typeof(T)} detected. Destroying duplicate: {gameObject.name} (keeping: {_instance.gameObject.name})");
            Debug.LogWarning($"[Singleton] Duplicate found in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            Debug.LogWarning($"[Singleton] Check your Level1 scene for multiple {typeof(T).Name} components!");
            Destroy(gameObject);
        }
    }
    
    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
    
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            if (_applicationIsQuitting)
            {
                Debug.Log($"[Singleton] {typeof(T).Name} destroyed on application quit");
            }
            else
            {
                Debug.LogWarning($"[Singleton] {typeof(T).Name} instance destroyed unexpectedly! This may cause issues.");
                _instance = null;
                _hasWarnedMissing = false; // Reset warning flag
            }
        }
    }
}
