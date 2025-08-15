using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Centralized enemy spawning and management system
/// Scalable for multiple enemy types and multiplayer
/// </summary>
public class EnemyManager : Singleton<EnemyManager>
{
    // Singleton overrides
    protected override bool AllowAutoCreate => false; // Only present in gameplay scenes
    protected override bool Persistent => true;       // Persist through result load if needed (optional)
    protected override bool LogCreation => false;
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int spawnWeight = 1;
        public float minSpawnTime = 1f;
        public float maxSpawnTime = 5f;
        public int maxConcurrent = 10;
        public bool canSpawnInGroups = false;
        [Range(1, 5)] public int groupSizeMax = 1;
    }
    
    [Header("Spawn Configuration")]
    [SerializeField] private EnemySpawnData[] enemyTypes;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float baseSpawnTime = 3f;
    [SerializeField] private float spawnTimeReduction = 0.99f;
    [SerializeField] private float minimumSpawnTime = 0.6f;
    [SerializeField] private int maxTotalEnemies = 50;
    
    [Header("Dependencies")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private zombieCount zombieCounter;
    
    // Runtime data
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Dictionary<GameObject, EnemySpawnData> enemyTypeMap = new Dictionary<GameObject, EnemySpawnData>();
    private float currentSpawnTime;
    private bool spawningEnabled = true;

    // Properties
    public int ActiveEnemyCount => activeEnemies.Count;
    public int MaxEnemies => maxTotalEnemies;
    public bool CanSpawn => spawningEnabled && 
                           ActiveEnemyCount < maxTotalEnemies && 
                           playerHealth != null && 
                           playerHealth.currentHealth > 0 &&
                           spawnPoints != null && 
                           spawnPoints.Length > 0;
    
    protected override void Awake()
    {
        Debug.Log($"[EnemyManager] Awake called on {gameObject.name} in scene {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        base.Awake();
        currentSpawnTime = baseSpawnTime;
        InitializeEnemyTypes();
    }
    
    private void Start()
    {
        InitializeDependencies();
        SubscribeToEvents();
        StartSpawning();
    }
    
    void OnEnable()
    {
        // Re-initialize dependencies when entering a new scene
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[EnemyManager] Scene loaded: {scene.name}, refreshing dependencies");
        Debug.Log($"[EnemyManager] Previous spawn points: {(spawnPoints?.Length ?? 0)}");
        
        // Clear active enemies list since they're destroyed with the old scene
        activeEnemies.Clear();
        GameEvents.EnemyCountChanged(0);
        
        // Reset references so they get re-found in the new scene
        playerHealth = null;
        zombieCounter = null;
        spawnPoints = null;
        
        // Re-initialize everything for the new scene
        InitializeDependencies();
        
        // Ensure zombie counter is reset after dependencies are found
        if (zombieCounter != null)
        {
            zombieCounter.entityCount = 0;
        }
        
        Debug.Log($"[EnemyManager] New spawn points: {(spawnPoints?.Length ?? 0)}");
        
        // Restart spawning if we were spawning before
        if (spawningEnabled)
        {
            StartSpawning();
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UnsubscribeFromEvents();
    }
    
    private void InitializeDependencies()
    {
        Debug.Log("[EnemyManager] Initializing dependencies...");
        
        if (playerHealth == null)
        {
            // Try legacy first, then refactored
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth == null)
            {
                var phr = FindObjectOfType<PlayerHealthRefactored>();
                if (phr != null)
                {
                    // Adapter: create a lightweight proxy to expose currentHealth without hard type ref
                    var proxyType = System.Type.GetType("PlayerHealthProxy");
                    if (proxyType != null)
                    {
                        var mb = gameObject.AddComponent(proxyType) as MonoBehaviour;
                        var proxy = mb as PlayerHealth;
                        if (proxy != null)
                        {
                            gameObject.SendMessage("Bind", phr, SendMessageOptions.DontRequireReceiver);
                            playerHealth = proxy;
                        }
                    }
                }
            }
        }
            
        if (zombieCounter == null)
        {
            zombieCounter = GameObject.Find("ZombieCount")?.GetComponent<zombieCount>();
            if (zombieCounter == null)
            {
                Debug.LogWarning("[EnemyManager] ZombieCount not found in scene!");
            }
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0 || !AreSpawnPointsValid())
        {
            Debug.Log("[EnemyManager] Finding spawn points...");
            var holder = GameObject.Find("EnemySpawnPoints");
            if (holder != null)
            {
                var list = new System.Collections.Generic.List<Transform>();
                foreach (Transform t in holder.transform)
                {
                    if (t != holder.transform) // Don't include the parent itself
                        list.Add(t);
                }
                spawnPoints = list.ToArray();
                Debug.Log($"[EnemyManager] Found {spawnPoints.Length} spawn points from holder");
            }
            else
            {
                Debug.LogWarning("[EnemyManager] EnemySpawnPoints holder not found in scene!");
                
                // Fallback: find any objects with "SpawnPoint" in the name
                var fallbackPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
                if (fallbackPoints.Length == 0)
                {
                    fallbackPoints = GameObject.FindObjectsOfType<GameObject>()
                        .Where(go => go.name.ToLower().Contains("spawn"))
                        .ToArray();
                }
                
                if (fallbackPoints.Length > 0)
                {
                    spawnPoints = fallbackPoints.Select(go => go.transform).ToArray();
                    Debug.Log($"[EnemyManager] Found {spawnPoints.Length} fallback spawn points");
                }
                else
                {
                    spawnPoints = new Transform[0]; // Ensure it's not null
                    Debug.LogError("[EnemyManager] No spawn points found in scene!");
                }
            }
        }
        
        Debug.Log($"[EnemyManager] Dependencies initialized - PlayerHealth: {playerHealth != null}, ZombieCounter: {zombieCounter != null}, SpawnPoints: {(spawnPoints?.Length ?? 0)}");
    }
    
    private bool AreSpawnPointsValid()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;
            
        // Check if any of the spawn points have been destroyed
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning("[EnemyManager] Found destroyed spawn point reference, refreshing spawn points");
                return false;
            }
        }
        
        return true;
    }
    
    private void InitializeEnemyTypes()
    {
        enemyTypeMap.Clear();
        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null)
            {
                enemyTypeMap[enemyData.enemyPrefab] = enemyData;
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnEnemySpawned += RegisterEnemy;
        GameEvents.OnEnemyDestroyed += UnregisterEnemy;
        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnPlayerDeath += StopSpawning;
        GameEvents.OnPlayerRevive += HandlePlayerRevive;
        GameEvents.OnGamePaused += StopSpawning;
        GameEvents.OnGameResumed += StartSpawning;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnEnemySpawned -= RegisterEnemy;
        GameEvents.OnEnemyDestroyed -= UnregisterEnemy;
        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnPlayerDeath -= StopSpawning;
        GameEvents.OnPlayerRevive -= HandlePlayerRevive;
        GameEvents.OnGamePaused -= StopSpawning;
        GameEvents.OnGameResumed -= StartSpawning;
    }
    
    private void HandleGameStarted()
    {
        Debug.Log("[EnemyManager] Game started, refreshing dependencies and resetting state");
        
        // Clear any existing enemies
        if (activeEnemies.Count > 0)
        {
            ClearAllEnemies();
        }
        
        // Reset spawn timing
        currentSpawnTime = baseSpawnTime;
        
        // Refresh dependencies
        InitializeDependencies();
        
        // Reset zombie counter
        if (zombieCounter != null)
        {
            zombieCounter.entityCount = 0;
        }
        
        // Start spawning
        StartSpawning();
    }
    
    private void HandlePlayerRevive()
    {
        Debug.Log("[EnemyManager] Player revived, refreshing dependencies and clearing enemies");
        Debug.Log($"[EnemyManager] Current spawn points before refresh: {(spawnPoints?.Length ?? 0)}");
        
        // Clear any remaining enemies from the previous game
        if (activeEnemies.Count > 0)
        {
            Debug.Log($"[EnemyManager] Clearing {activeEnemies.Count} remaining enemies");
            ClearAllEnemies();
        }
        
        // Refresh dependencies in case spawn points were destroyed/recreated
        InitializeDependencies();
        
        Debug.Log($"[EnemyManager] Spawn points after refresh: {(spawnPoints?.Length ?? 0)}");
        
        // Reset zombie counter
        if (zombieCounter != null)
        {
            zombieCounter.entityCount = 0;
        }
        
        // Start spawning
        StartSpawning();
    }
    
    private void StartSpawning()
    {
        spawningEnabled = true;
        if (IsInvoking(nameof(SpawnEnemy)))
            CancelInvoke(nameof(SpawnEnemy));
        InvokeRepeating(nameof(SpawnEnemy), currentSpawnTime, currentSpawnTime);
    }
    
    private void StopSpawning()
    {
        spawningEnabled = false;
        CancelInvoke(nameof(SpawnEnemy));
    }
    
    private void SpawnEnemy()
    {
        if (!CanSpawn || spawnPoints.Length == 0 || enemyTypes.Length == 0)
        {
            if (!spawningEnabled) return;
            
            // Log specific issues for debugging
            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogWarning("[EnemyManager] Cannot spawn: No spawn points available");
            else if (playerHealth == null)
                Debug.LogWarning("[EnemyManager] Cannot spawn: PlayerHealth is null");
            else if (ActiveEnemyCount >= maxTotalEnemies)
                Debug.LogWarning("[EnemyManager] Cannot spawn: Max enemy limit reached");
                
            return;
        }
            
        // Select enemy type based on weighted probability
        var enemyData = SelectRandomEnemyType();
        if (enemyData?.enemyPrefab == null)
            return;
            
        // Check concurrent limit for this enemy type
        int currentCount = GetActiveEnemyCount(enemyData.enemyPrefab);
        if (currentCount >= enemyData.maxConcurrent)
            return;
            
        // Validate spawn points are still valid before using them
        if (!AreSpawnPointsValid())
        {
            Debug.LogWarning("[EnemyManager] Spawn points invalid during spawning, reinitializing...");
            InitializeDependencies();
            if (!CanSpawn) return; // Re-check after dependency refresh
        }
            
        // Select spawn point and validate it
        Transform spawnPoint = null;
        int attempts = 0;
        while (spawnPoint == null && attempts < spawnPoints.Length)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform candidate = spawnPoints[randomIndex];
            if (candidate != null) // Validate the transform isn't destroyed
            {
                spawnPoint = candidate;
                break;
            }
            attempts++;
        }
        
        if (spawnPoint == null)
        {
            Debug.LogError("[EnemyManager] No valid spawn points found, cannot spawn enemy");
            return;
        }
        
        // Spawn enemy
        List<GameObject> spawned = new List<GameObject>();
        GameObject newEnemy = Instantiate(enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawned.Add(newEnemy);
        // Publish spawn so RegisterEnemy tracks it
        GameEvents.EnemySpawned(newEnemy);
        
        // Handle group spawning
        if (enemyData.canSpawnInGroups && Random.value < 0.3f)
        {
            int groupSize = Random.Range(2, enemyData.groupSizeMax + 1);
            for (int i = 1; i < groupSize && ActiveEnemyCount + spawned.Count < maxTotalEnemies; i++)
            {
                Vector3 groupPosition = spawnPoint.position + Random.insideUnitSphere * 3f;
                groupPosition.y = spawnPoint.position.y;
                var e = Instantiate(enemyData.enemyPrefab, groupPosition, spawnPoint.rotation);
                spawned.Add(e);
                GameEvents.EnemySpawned(e);
            }
        }
        
        // Update spawn timing
        UpdateSpawnTiming();
        
        // No direct zombieCounter update here; handled in RegisterEnemy per spawned enemy.
    }
    
    private EnemySpawnData SelectRandomEnemyType()
    {
        if (enemyTypes.Length == 0)
            return null;
            
        int totalWeight = 0;
        foreach (var enemy in enemyTypes)
            totalWeight += enemy.spawnWeight;
            
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;
        
        foreach (var enemy in enemyTypes)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue < currentWeight)
                return enemy;
        }
        
        return enemyTypes[0]; // Fallback
    }
    
    private int GetActiveEnemyCount(GameObject prefab)
    {
        int count = 0;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.name.StartsWith(prefab.name))
                count++;
        }
        return count;
    }
    
    private void UpdateSpawnTiming()
    {
        if (currentSpawnTime > minimumSpawnTime)
        {
            currentSpawnTime *= spawnTimeReduction;
            currentSpawnTime = Mathf.Max(currentSpawnTime, minimumSpawnTime);
            
            // Update invoke timing
            CancelInvoke(nameof(SpawnEnemy));
            InvokeRepeating(nameof(SpawnEnemy), currentSpawnTime, currentSpawnTime);
        }
    }
    
    private void RegisterEnemy(GameObject enemy)
    {
        if (enemy != null && !activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
            GameEvents.EnemyCountChanged(activeEnemies.Count);
            if (zombieCounter != null)
            {
                zombieCounter.entityCount++;
            }
        }
    }
    
    private void UnregisterEnemy(GameObject enemy)
    {
        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
            GameEvents.EnemyCountChanged(activeEnemies.Count);
            
            // Update zombie counter (backwards compatibility)
            if (zombieCounter != null && zombieCounter.entityCount > 0)
            {
                zombieCounter.entityCount--;
            }
        }
    }
    
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
    GameEvents.EnemyCountChanged(0);
        
        if (zombieCounter != null)
            zombieCounter.entityCount = 0;
    }
    
    public void SetSpawnRate(float multiplier)
    {
        currentSpawnTime = baseSpawnTime / multiplier;
        currentSpawnTime = Mathf.Max(currentSpawnTime, minimumSpawnTime);
    }
    
    public void ForceRefreshDependencies()
    {
        Debug.Log("[EnemyManager] Force refreshing dependencies");
        InitializeDependencies();
    }
    
    // Static methods for backwards compatibility
    public static void StopAllSpawning() => Instance?.StopSpawning();
    public static void StartAllSpawning() => Instance?.StartSpawning();
    public static int GetActiveCount() => Instance?.ActiveEnemyCount ?? 0;
    public static void RefreshDependencies() => Instance?.ForceRefreshDependencies();
}
