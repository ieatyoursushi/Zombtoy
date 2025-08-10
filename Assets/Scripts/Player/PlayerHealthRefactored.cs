using UnityEngine;
using System.Collections;

/// <summary>
/// Comprehensive Player Health System (Refactored)
/// Event-driven, multiplayer-ready, and easily extensible
/// </summary>
public class PlayerHealthRefactored : MonoBehaviour
{
    [Header("Health Configuration")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int _currentHealth;
    [SerializeField] private float healthRegenRate = 0f; // Health per second
    [SerializeField] private float healthRegenDelay = 5f; // Delay before regen starts
    
    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float _currentStamina;
    [SerializeField] private float staminaDrainRate = 20f; // Per second when sprinting
    [SerializeField] private float staminaRegenRate = 25f; // Per second when not sprinting
    [SerializeField] private float minStaminaToSprint = 10f;
    
    [Header("UI References")]
    [SerializeField] private UnityEngine.UI.Slider healthSlider;
    [SerializeField] private UnityEngine.UI.Image damageOverlay;
    [SerializeField] private GameObject staminaSliderContainer;
    [SerializeField] private UnityEngine.UI.Slider staminaSlider;
    
    [Header("Visual Effects")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.1f);
    [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.1f);
    [SerializeField] private float flashSpeed = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip lowHealthSound;
    
    [Header("Movement Integration")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float currentMoveSpeed;
    
    // Components
    private ComponentCache componentCache;
    private Animator animator;
    private AudioSource audioSource;
    // Use a loose reference to avoid hard dependency and type collisions
    private MonoBehaviour playerMovement;
    // Support legacy controller if present
    private PlayerMovement legacyPlayerMovement;
    private CameraMovement cameraMovement;
    
    // State
    private bool _isDead;
    private bool isInvulnerable;
    private bool isSprinting;
    private bool isRegenerating;
    private float lastDamageTime;
    private float invulnerabilityDuration = 0.1f;
    
    // Effects
    private bool showDamageFlash;
    private bool showHealFlash;
    
    // Properties
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthPercentage => (float)_currentHealth / maxHealth;
    public float CurrentStamina => _currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => _currentStamina / maxStamina;
    public bool IsDead => _isDead;
    public bool CanSprint => _currentStamina >= minStaminaToSprint && !_isDead;
    public bool IsSprinting => isSprinting;
    public float CurrentMoveSpeed => currentMoveSpeed;
    
    // Events
    public System.Action<int, int> OnHealthChanged; // current, max
    public System.Action<float, float> OnStaminaChanged; // current, max
    public System.Action<int> OnDamageTaken;
    public System.Action<int> OnHealed;
    public System.Action OnPlayerDeath;
    public System.Action OnPlayerRevive;
    public System.Action<bool> OnSprintStateChanged;
    
    void Awake()
    {
        InitializeComponents();
        InitializeHealth();
        InitializeStamina();
    }
    
    void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        currentMoveSpeed = walkSpeed;
    }
    
    void Update()
    {
        HandleStamina();
        HandleHealthRegeneration();
        UpdateVisualEffects();
        UpdateUI();
        HandleSprinting();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void InitializeComponents()
    {
        componentCache = GetComponent<ComponentCache>();
        if (componentCache == null)
            componentCache = gameObject.AddComponent<ComponentCache>();
            
        animator = componentCache.GetCachedComponent<Animator>();
        audioSource = componentCache.GetCachedComponent<AudioSource>();
        
        // Try to get movement components
        // Prefer refactored movement if available (resolve by name to avoid compile-time type dependency)
        var pmrType = System.Type.GetType("PlayerMovementRefactored");
        if (pmrType != null)
        {
            var comp = GetComponent(pmrType) as MonoBehaviour;
            if (comp != null) playerMovement = comp;
        }
        // Fallback to legacy movement
        if (playerMovement == null)
            legacyPlayerMovement = componentCache.GetCachedComponent<PlayerMovement>();
        if (playerMovement == null && legacyPlayerMovement == null)
        {
            cameraMovement = componentCache.GetCachedComponent<CameraMovement>();
        }
    }
    
    private void InitializeHealth()
    {
    _currentHealth = maxHealth;
    _isDead = false;
        isInvulnerable = false;
        lastDamageTime = 0f;
    }
    
    private void InitializeStamina()
    {
    _currentStamina = maxStamina;
        isSprinting = false;
    }
    
    private void InitializeUI()
    {
        // Find UI elements if not assigned
        if (healthSlider == null)
        {
            var healthSliderGO = GameObject.Find("HealthSlider");
            if (healthSliderGO != null)
                healthSlider = healthSliderGO.GetComponent<UnityEngine.UI.Slider>();
        }
        
        if (damageOverlay == null)
        {
            var damageImageGO = GameObject.Find("DamageImage");
            if (damageImageGO != null)
                damageOverlay = damageImageGO.GetComponent<UnityEngine.UI.Image>();
        }
        
        if (staminaSliderContainer == null)
        {
            staminaSliderContainer = GameObject.Find("StaminaSlider");
        }
        
        if (staminaSlider == null && staminaSliderContainer != null)
        {
            staminaSlider = staminaSliderContainer.GetComponentInChildren<UnityEngine.UI.Slider>();
        }
        
        UpdateUI();
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnGameResumed += HandleGameResumed;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnGameResumed -= HandleGameResumed;
    }
    
    private void HandleGamePaused()
    {
        // Stop regeneration during pause
        isRegenerating = false;
    }
    
    private void HandleGameResumed()
    {
        // Resume normal operations
    }
    
    private void HandleStamina()
    {
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && IsMoving() && CanSprint;
        
        if (wantsSprint && !_isDead)
        {
            // Drain stamina
            _currentStamina -= staminaDrainRate * Time.deltaTime;
            _currentStamina = Mathf.Max(0f, _currentStamina);
            
            if (_currentStamina <= 0f)
            {
                wantsSprint = false;
            }
        }
        else
        {
            // Regenerate stamina
            _currentStamina += staminaRegenRate * Time.deltaTime;
            _currentStamina = Mathf.Min(maxStamina, _currentStamina);
        }
        
        // Update sprint state
        if (isSprinting != wantsSprint)
        {
            isSprinting = wantsSprint;
            OnSprintStateChanged?.Invoke(isSprinting);
            UpdateMoveSpeed();
        }
        
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }
    
    private void HandleHealthRegeneration()
    {
    if (healthRegenRate > 0f && _currentHealth < maxHealth && !_isDead)
        {
            if (Time.time - lastDamageTime >= healthRegenDelay)
            {
                if (!isRegenerating)
                {
                    isRegenerating = true;
                }
                
                float regenAmount = healthRegenRate * Time.deltaTime;
                Heal(Mathf.RoundToInt(regenAmount), false);
            }
        }
    }
    
    private void UpdateVisualEffects()
    {
        if (damageOverlay != null)
        {
            Color targetColor = Color.clear;
            
            if (showDamageFlash)
            {
                targetColor = damageFlashColor;
                showDamageFlash = false;
            }
            else if (showHealFlash)
            {
                targetColor = healFlashColor;
                showHealFlash = false;
            }
            
            damageOverlay.color = Color.Lerp(damageOverlay.color, targetColor, flashSpeed * Time.deltaTime);
        }
    }
    
    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = _currentHealth;
        }
        
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = _currentStamina;
        }
        
        // Show/hide stamina slider based on stamina level
        if (staminaSliderContainer != null)
        {
            bool showStamina = StaminaPercentage < 1f || isSprinting;
            staminaSliderContainer.SetActive(showStamina);
        }
    }
    
    private void HandleSprinting()
    {
        UpdateMoveSpeed();
    }
    
    private void UpdateMoveSpeed()
    {
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        currentMoveSpeed = targetSpeed;
        
        // Update movement components
        if (playerMovement != null)
        {
            // SendMessage to call SetMoveSpeed if present without a hard reference
            playerMovement.SendMessage("SetMoveSpeed", targetSpeed, SendMessageOptions.DontRequireReceiver);
        }
        else if (legacyPlayerMovement != null)
        {
            legacyPlayerMovement.speed = targetSpeed;
        }
        else if (cameraMovement != null)
        {
            // Fallback to direct field if refactored method isn't available
            cameraMovement.playerSpeed = targetSpeed;
        }
    }
    
    private bool IsMoving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return h != 0f || v != 0f;
    }
    
    public void TakeDamage(int damageAmount)
    {
    if (_isDead || isInvulnerable || damageAmount <= 0)
            return;
            
    _currentHealth -= damageAmount;
    _currentHealth = Mathf.Max(0, _currentHealth);
        
        lastDamageTime = Time.time;
        isRegenerating = false;
        showDamageFlash = true;
        
        // Brief invulnerability
        StartCoroutine(InvulnerabilityCoroutine());
        
        // Audio
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Events
        OnDamageTaken?.Invoke(damageAmount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEvents.PlayerHealthChanged(_currentHealth);
        
        // Check for death
        if (_currentHealth <= 0 && !_isDead)
        {
            Die();
        }
        
        // Low health warning
        if (_currentHealth <= maxHealth * 0.2f && lowHealthSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lowHealthSound);
        }
    }
    
    public void Heal(int healAmount, bool showEffect = true)
    {
        if (_isDead || healAmount <= 0 || _currentHealth >= maxHealth)
            return;
            
        _currentHealth += healAmount;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth);
        
        if (showEffect)
        {
            showHealFlash = true;
            
            if (healSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(healSound);
            }
        }
        
        OnHealed?.Invoke(healAmount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEvents.PlayerHealthChanged(_currentHealth);
    }
    
    public void SetHealth(int health)
    {
        _currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEvents.PlayerHealthChanged(_currentHealth);
        
        if (_currentHealth <= 0 && !_isDead)
        {
            Die();
        }
        else if (_currentHealth > 0 && _isDead)
        {
            Revive();
        }
    }
    
    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        if (_currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    }
    
    public void RestoreStamina(float amount)
    {
        _currentStamina += amount;
        _currentStamina = Mathf.Min(maxStamina, _currentStamina);
        OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
    }
    
    private void Die()
    {
        if (_isDead)
            return;
            
        _isDead = true;
        isSprinting = false;
        isRegenerating = false;
        
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
        
        OnPlayerDeath?.Invoke();
        GameEvents.PlayerDeath();
        
        Debug.Log("[PlayerHealth] Player died");
    }
    
    public void Revive()
    {
    if (!_isDead)
            return;
            
    _isDead = false;
    _currentHealth = maxHealth;
    _currentStamina = maxStamina;
        
        if (animator != null)
        {
            animator.SetTrigger("Revive");
        }
        
        OnPlayerRevive?.Invoke();
    OnHealthChanged?.Invoke(_currentHealth, maxHealth);
    OnStaminaChanged?.Invoke(_currentStamina, maxStamina);
        GameEvents.PlayerRevive();
    GameEvents.PlayerHealthChanged(_currentHealth);
        
        Debug.Log("[PlayerHealth] Player revived");
    }
    
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvulnerable = false;
    }
    
    // Static accessor for this refactored component
    public static PlayerHealthRefactored GetPlayerHealth()
    {
        return FindObjectOfType<PlayerHealthRefactored>();
    }
}
