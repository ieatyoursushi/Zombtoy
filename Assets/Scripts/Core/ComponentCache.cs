using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// High-performance component caching system
/// Eliminates repeated GetComponent calls
/// Essential for multiplayer performance
/// </summary>
public class ComponentCache : MonoBehaviour
{
    private Dictionary<System.Type, Component> componentCache = new Dictionary<System.Type, Component>();
    private Dictionary<string, Component> namedComponentCache = new Dictionary<string, Component>();
    
    /// <summary>
    /// Get component with caching. Much faster than GetComponent in Update loops.
    /// </summary>
    public T GetCachedComponent<T>() where T : Component
    {
        System.Type type = typeof(T);
        
        if (componentCache.TryGetValue(type, out Component cachedComponent))
        {
            return cachedComponent as T;
        }
        
        T component = GetComponent<T>();
        if (component != null)
        {
            componentCache[type] = component;
        }
        
        return component;
    }
    
    /// <summary>
    /// Get component in children with caching
    /// </summary>
    public T GetCachedComponentInChildren<T>(string childName = null) where T : Component
    {
        string key = childName ?? typeof(T).Name;
        
        if (namedComponentCache.TryGetValue(key, out Component cachedComponent))
        {
            return cachedComponent as T;
        }
        
        T component;
        if (string.IsNullOrEmpty(childName))
        {
            component = GetComponentInChildren<T>();
        }
        else
        {
            Transform child = transform.Find(childName);
            component = child?.GetComponent<T>();
        }
        
        if (component != null)
        {
            namedComponentCache[key] = component;
        }
        
        return component;
    }
    
    /// <summary>
    /// Clear the cache (call when components might be destroyed)
    /// </summary>
    public void ClearCache()
    {
        componentCache.Clear();
        namedComponentCache.Clear();
    }
    
    /// <summary>
    /// Remove a specific component from cache
    /// </summary>
    public void RemoveFromCache<T>() where T : Component
    {
        componentCache.Remove(typeof(T));
    }
    
    /// <summary>
    /// Preload commonly used components
    /// </summary>
    public void PreloadComponents()
    {
        // Common components
        GetCachedComponent<Transform>();
        GetCachedComponent<Rigidbody>();
        GetCachedComponent<Collider>();
        GetCachedComponent<Renderer>();
        GetCachedComponent<Animator>();
        GetCachedComponent<AudioSource>();
    }
    
    void Awake()
    {
        PreloadComponents();
    }
    
    void OnDestroy()
    {
        ClearCache();
    }
}

/// <summary>
/// Static utility class for global component caching
/// Use for frequently accessed components across the game
/// </summary>
public static class GlobalComponentCache
{
    private static Dictionary<GameObject, ComponentCache> objectCaches = new Dictionary<GameObject, ComponentCache>();
    
    public static T GetCachedComponent<T>(this GameObject gameObject) where T : Component
    {
        if (!objectCaches.TryGetValue(gameObject, out ComponentCache cache))
        {
            cache = gameObject.GetComponent<ComponentCache>();
            if (cache == null)
            {
                cache = gameObject.AddComponent<ComponentCache>();
            }
            objectCaches[gameObject] = cache;
        }
        
        return cache.GetCachedComponent<T>();
    }
    
    public static void ClearGlobalCache()
    {
        objectCaches.Clear();
    }
    
    public static void RemoveFromGlobalCache(GameObject gameObject)
    {
        objectCaches.Remove(gameObject);
    }
}
