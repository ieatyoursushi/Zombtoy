using UnityEngine;

/// <summary>
/// Base weapon interface for all weapons
/// Enables polymorphic weapon handling and easy expansion
/// </summary>
public interface IWeapon
{
    string WeaponName { get; }
    int Damage { get; }
    float FireRate { get; }
    float Range { get; }
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    bool CanFire { get; }
    bool IsReloading { get; }
    
    void Fire(Vector3 origin, Vector3 direction);
    void Reload();
    void AddAmmo(int amount);
    void SetWeaponData(WeaponData data);
}

/// <summary>
/// Scriptable object for weapon configuration
/// Allows designers to easily create new weapons
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Zombtoy/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Properties")]
    public string weaponName = "Default Weapon";
    public Sprite weaponIcon;
    public GameObject weaponPrefab;
    
    [Header("Combat Stats")]
    public int baseDamage = 100;
    public float fireRate = 0.1f; // Time between shots
    public float range = 100f;
    public int magazineSize = 30;
    public int totalAmmo = 300;
    
    [Header("Behavior")]
    public bool isAutomatic = true;
    public bool hasReload = true;
    public float reloadTime = 2f;
    public bool penetratesEnemies = false;
    
    [Header("Effects")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    
    [Header("Projectile (for rockets, etc.)")]
    public bool usesProjectile = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    
    [Header("Multiplayer")]
    public bool synchronizeAcrossNetwork = true;
    public int weaponId = 0; // Unique ID for network sync
}

/// <summary>
/// Base weapon class implementing common weapon functionality
/// Extensible for specific weapon types
/// </summary>
public abstract class BaseWeapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Configuration")]
    [SerializeField] protected WeaponData weaponData;
    
    [Header("Components")]
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected LineRenderer bulletTrail;
    [SerializeField] protected Light muzzleLight;
    
    // Cached components
    protected ComponentCache componentCache;
    protected AudioSource audioSource;
    protected ParticleSystem particles;
    
    // Weapon state
    protected int currentAmmo;
    protected int totalAmmo;
    protected float lastFireTime;
    protected bool isReloading;
    protected Coroutine reloadCoroutine;
    
    // Properties
    public string WeaponName => weaponData?.weaponName ?? "Unknown";
    public int Damage => weaponData?.baseDamage ?? 0;
    public float FireRate => weaponData?.fireRate ?? 0.1f;
    public float Range => weaponData?.range ?? 100f;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => weaponData?.magazineSize ?? 0;
    public int TotalAmmo => totalAmmo;
    public bool CanFire => !isReloading && currentAmmo > 0 && Time.time >= lastFireTime + FireRate;
    public bool IsReloading => isReloading;
    public WeaponData Data => weaponData;
    
    // Events
    public System.Action OnWeaponFired;
    public System.Action OnWeaponEmpty;
    public System.Action OnReloadStarted;
    public System.Action OnReloadCompleted;
    public System.Action<int> OnAmmoChanged;
    
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeWeapon();
    }
    
    protected virtual void InitializeComponents()
    {
        componentCache = GetComponent<ComponentCache>();
        if (componentCache == null)
            componentCache = gameObject.AddComponent<ComponentCache>();
            
        audioSource = componentCache.GetCachedComponent<AudioSource>();
        particles = componentCache.GetCachedComponent<ParticleSystem>();
        
        if (firePoint == null)
            firePoint = transform;
    }
    
    protected virtual void InitializeWeapon()
    {
        if (weaponData != null)
        {
            currentAmmo = weaponData.magazineSize;
            totalAmmo = weaponData.totalAmmo;
        }
    }
    
    public virtual void SetWeaponData(WeaponData data)
    {
        weaponData = data;
        InitializeWeapon();
    }
    
    public virtual void Fire(Vector3 origin, Vector3 direction)
    {
        if (!CanFire)
        {
            if (currentAmmo <= 0)
            {
                PlayEmptySound();
                OnWeaponEmpty?.Invoke();
            }
            return;
        }
        
        // Update fire timing and ammo
        lastFireTime = Time.time;
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo);
        
        // Perform weapon-specific firing
        DoFire(origin, direction);
        
        // Common effects
        PlayFireEffects();
        
        // Events
        OnWeaponFired?.Invoke();
        GameEvents.WeaponFired(origin, Damage);
        
        // Auto-reload if empty
        if (currentAmmo <= 0 && totalAmmo > 0 && weaponData.hasReload)
        {
            Reload();
        }
    }
    
    protected abstract void DoFire(Vector3 origin, Vector3 direction);
    
    public virtual void Reload()
    {
        if (isReloading || currentAmmo >= MaxAmmo || totalAmmo <= 0)
            return;
            
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
            
        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }
    
    protected virtual System.Collections.IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        OnReloadStarted?.Invoke();
        
        if (weaponData.reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.reloadSound);
        }
        
        yield return new WaitForSeconds(weaponData.reloadTime);
        
        // Calculate ammo to reload
        int ammoToReload = Mathf.Min(MaxAmmo - currentAmmo, totalAmmo);
        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;
        
        isReloading = false;
        OnReloadCompleted?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    public virtual void AddAmmo(int amount)
    {
        totalAmmo += amount;
        OnAmmoChanged?.Invoke(currentAmmo);
    }
    
    protected virtual void PlayFireEffects()
    {
        // Muzzle flash
        if (particles != null)
        {
            particles.Play();
        }
        
        // Sound
        if (weaponData.fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.fireSound);
        }
        
        // Light flash
        if (muzzleLight != null)
        {
            muzzleLight.enabled = true;
            Invoke(nameof(DisableMuzzleLight), 0.1f);
        }
    }
    
    protected virtual void PlayEmptySound()
    {
        if (weaponData.emptySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(weaponData.emptySound);
        }
    }
    
    protected virtual void DisableMuzzleLight()
    {
        if (muzzleLight != null)
            muzzleLight.enabled = false;
    }
    
    protected virtual void OnDestroy()
    {
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);
    }
}
