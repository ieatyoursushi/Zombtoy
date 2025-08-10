using UnityEngine;

/// <summary>
/// Projectile-based weapon (rocket launcher, grenade launcher)
/// Spawns physical projectiles instead of raycasting
/// </summary>
public class ProjectileWeapon : BaseWeapon
{
    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 5f;
    [SerializeField] private bool inheritMomentum = false;
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;
    
    protected override void DoFire(Vector3 origin, Vector3 direction)
    {
        if (weaponData.projectilePrefab == null)
        {
            Debug.LogWarning($"[ProjectileWeapon] No projectile prefab assigned to {WeaponName}");
            return;
        }
        
        // Calculate spawn position with offset
        Vector3 spawnPosition = origin + transform.TransformDirection(spawnOffset);
        
        // Spawn projectile
        GameObject projectile = Instantiate(weaponData.projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        // Configure projectile
        ConfigureProjectile(projectile, direction);
    }
    
    private void ConfigureProjectile(GameObject projectile, Vector3 direction)
    {
        // Add velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = weaponData.projectileSpeed > 0 ? weaponData.projectileSpeed : projectileSpeed;
            rb.velocity = direction * speed;
            
            // Inherit shooter's momentum if enabled
            if (inheritMomentum)
            {
                Rigidbody shooterRb = GetComponentInParent<Rigidbody>();
                if (shooterRb != null)
                {
                    rb.velocity += shooterRb.velocity;
                }
            }
        }
        
        // Set damage on projectile
        IProjectile projectileComponent = projectile.GetComponent<IProjectile>();
        if (projectileComponent != null)
        {
            projectileComponent.SetDamage(Damage);
            projectileComponent.SetOwner(gameObject);
        }
        
        // Alternative: Try to configure an optional Explosion-like component dynamically
        var explosionComp = projectile.GetComponent("Explosion");
        if (explosionComp != null)
        {
            // Try common members via SendMessage to avoid hard type dependency
            projectile.SendMessage("SetDamage", Damage, SendMessageOptions.DontRequireReceiver);
            // Also attempt to set a 'damage' field/property if present
            projectile.SendMessage("SetExplosionDamage", Damage, SendMessageOptions.DontRequireReceiver);
        }
        
        // Set lifetime
        float lifetime = weaponData.projectileSpeed > 0 ? projectileLifetime : 5f;
        Destroy(projectile, lifetime);
    }
    
    // Method to configure as rocket launcher
    public void ConfigureAsRocketLauncher()
    {
        projectileSpeed = 15f;
        projectileLifetime = 3f;
        inheritMomentum = false;
    }
    
    // Method to configure as grenade launcher
    public void ConfigureAsGrenadeLauncher()
    {
        projectileSpeed = 8f;
        projectileLifetime = 4f;
        inheritMomentum = true;
        
        // Add arc to grenades by modifying gravity
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
        }
    }
}

/// <summary>
/// Interface for projectiles to receive configuration
/// </summary>
public interface IProjectile
{
    void SetDamage(int damage);
    void SetOwner(GameObject owner);
    void OnImpact(Collision collision);
}
