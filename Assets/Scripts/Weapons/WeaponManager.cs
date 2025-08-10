using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized weapon management system
/// Handles weapon switching, inventory, and multiplayer synchronization
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private List<IWeapon> availableWeapons = new List<IWeapon>();
    [SerializeField] private int currentWeaponIndex = 0;
    [SerializeField] private bool allowWeaponSwitching = true;
    [SerializeField] private float switchCooldown = 0.5f;
    
    [Header("Input Settings")]
    [SerializeField] private KeyCode[] weaponKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };
    
    // Components
    private PlayerHealth playerHealth;
    private ComponentCache componentCache;
    private float lastSwitchTime;
    
    // Properties
    public IWeapon CurrentWeapon => availableWeapons.Count > 0 && currentWeaponIndex < availableWeapons.Count 
        ? availableWeapons[currentWeaponIndex] : null;
    public int CurrentWeaponIndex => currentWeaponIndex;
    public int WeaponCount => availableWeapons.Count;
    public bool CanSwitchWeapons => allowWeaponSwitching && Time.time >= lastSwitchTime + switchCooldown;
    
    // Events
    public System.Action<IWeapon, IWeapon> OnWeaponSwitched; // from, to
    public System.Action<IWeapon> OnWeaponAdded;
    public System.Action<IWeapon> OnWeaponRemoved;
    
    void Awake()
    {
        InitializeComponents();
        InitializeWeapons();
    }
    
    void Start()
    {
        SubscribeToEvents();
        SwitchToWeapon(currentWeaponIndex, false);
    }
    
    void Update()
    {
        HandleInput();
        HandleWeaponFiring();
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
            
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = GetComponentInParent<PlayerHealth>();
    }
    
    private void InitializeWeapons()
    {
        // Find all weapon components
        IWeapon[] weapons = GetComponentsInChildren<IWeapon>();
        
        foreach (var weapon in weapons)
        {
            if (!availableWeapons.Contains(weapon))
            {
                availableWeapons.Add(weapon);
            }
        }
        
        // Set all weapons inactive except the first one
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            SetWeaponActive(i, i == currentWeaponIndex);
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnGamePaused += HandleGamePaused;
        GameEvents.OnPlayerDeath += HandlePlayerDeath;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnGamePaused -= HandleGamePaused;
        GameEvents.OnPlayerDeath -= HandlePlayerDeath;
    }
    
    private void HandleGamePaused()
    {
        // Stop firing when paused
    }
    
    private void HandlePlayerDeath()
    {
        // Disable all weapons when player dies
        allowWeaponSwitching = false;
    }
    
    private void HandleInput()
    {
        if (!CanSwitchWeapons || GameStateManager.IsCurrentlyPaused())
            return;
            
        // Number key switching
        for (int i = 0; i < weaponKeys.Length && i < availableWeapons.Count; i++)
        {
            if (Input.GetKeyDown(weaponKeys[i]))
            {
                SwitchToWeapon(i);
                break;
            }
        }
        
        // Mouse wheel switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchToNextWeapon();
        }
        else if (scroll < 0f)
        {
            SwitchToPreviousWeapon();
        }
        
        // Reload input
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadCurrentWeapon();
        }
    }
    
    private void HandleWeaponFiring()
    {
        if (CurrentWeapon == null || !GameStateManager.IsCurrentlyPlaying())
            return;
            
        if (playerHealth != null && playerHealth.isDead)
            return;
            
        bool firePressed = Input.GetButton("Fire1");
        bool fireDown = Input.GetButtonDown("Fire1");
        
        // Handle automatic vs semi-automatic firing
        bool isAutomatic = false;
        if (CurrentWeapon is BaseWeapon baseWeapon && baseWeapon.Data != null)
        {
            isAutomatic = baseWeapon.Data.isAutomatic;
        }
        bool shouldFire = isAutomatic ? firePressed : fireDown;
        
        if (shouldFire && CurrentWeapon.CanFire)
        {
            Vector3 fireOrigin = transform.position;
            Vector3 fireDirection = transform.forward;
            
            CurrentWeapon.Fire(fireOrigin, fireDirection);
        }
    }
    
    public void SwitchToWeapon(int weaponIndex, bool playEffects = true)
    {
        if (!CanSwitchWeapons || weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
            return;
            
        if (weaponIndex == currentWeaponIndex)
            return;
            
        IWeapon previousWeapon = CurrentWeapon;
        
        // Deactivate current weapon
        if (previousWeapon != null)
        {
            SetWeaponActive(currentWeaponIndex, false);
        }
        
        // Switch to new weapon
        currentWeaponIndex = weaponIndex;
        IWeapon newWeapon = CurrentWeapon;
        
        // Activate new weapon
        if (newWeapon != null)
        {
            SetWeaponActive(currentWeaponIndex, true);
        }
        
        lastSwitchTime = Time.time;
        
        // Fire events
        OnWeaponSwitched?.Invoke(previousWeapon, newWeapon);
        
        if (playEffects)
        {
            // Play weapon switch sound or animation
            Debug.Log($"[WeaponManager] Switched to {newWeapon?.WeaponName ?? "None"}");
        }
    }
    
    public void SwitchToNextWeapon()
    {
        int nextIndex = (currentWeaponIndex + 1) % availableWeapons.Count;
        SwitchToWeapon(nextIndex);
    }
    
    public void SwitchToPreviousWeapon()
    {
        int prevIndex = currentWeaponIndex - 1;
        if (prevIndex < 0)
            prevIndex = availableWeapons.Count - 1;
        SwitchToWeapon(prevIndex);
    }
    
    public void AddWeapon(IWeapon weapon)
    {
        if (weapon != null && !availableWeapons.Contains(weapon))
        {
            availableWeapons.Add(weapon);
            OnWeaponAdded?.Invoke(weapon);
            
            // If this is the first weapon, switch to it
            if (availableWeapons.Count == 1)
            {
                SwitchToWeapon(0, false);
            }
        }
    }
    
    public void RemoveWeapon(IWeapon weapon)
    {
        int index = availableWeapons.IndexOf(weapon);
        if (index >= 0)
        {
            availableWeapons.RemoveAt(index);
            OnWeaponRemoved?.Invoke(weapon);
            
            // Adjust current weapon index if necessary
            if (currentWeaponIndex >= availableWeapons.Count)
            {
                currentWeaponIndex = Mathf.Max(0, availableWeapons.Count - 1);
            }
            
            SwitchToWeapon(currentWeaponIndex, false);
        }
    }
    
    public void ReloadCurrentWeapon()
    {
        CurrentWeapon?.Reload();
    }
    
    public void AddAmmoToCurrentWeapon(int amount)
    {
        CurrentWeapon?.AddAmmo(amount);
    }
    
    private void SetWeaponActive(int weaponIndex, bool active)
    {
        if (weaponIndex < 0 || weaponIndex >= availableWeapons.Count)
            return;
            
        IWeapon weapon = availableWeapons[weaponIndex];
        if (weapon is MonoBehaviour weaponMono)
        {
            weaponMono.gameObject.SetActive(active);
        }
    }
    
    // Multiplayer support
    public void SetWeaponState(int weaponIndex, int ammo, bool isReloading)
    {
        if (weaponIndex >= 0 && weaponIndex < availableWeapons.Count)
        {
            // This would be used to sync weapon state across network
            // Implementation depends on networking solution
        }
    }
    
    public WeaponState GetWeaponState(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < availableWeapons.Count)
        {
            IWeapon weapon = availableWeapons[weaponIndex];
            return new WeaponState
            {
                weaponIndex = weaponIndex,
                currentAmmo = weapon.CurrentAmmo,
                isReloading = weapon.IsReloading
            };
        }
        
        return default;
    }
}

/// <summary>
/// Weapon state for networking
/// </summary>
[System.Serializable]
public struct WeaponState
{
    public int weaponIndex;
    public int currentAmmo;
    public bool isReloading;
}
