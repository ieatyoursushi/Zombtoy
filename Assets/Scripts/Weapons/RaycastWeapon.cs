using UnityEngine;

/// <summary>
/// Raycast-based weapon (shotgun, rifle, pistol)
/// Extends the base weapon system for hitscan weapons
/// </summary>
public class RaycastWeapon : BaseWeapon
{
    [Header("Raycast Weapon Settings")]
    [SerializeField] private LayerMask shootableMask = -1;
    [SerializeField] private bool showBulletTrail = true;
    [SerializeField] private float trailDuration = 0.1f;
    [SerializeField] private int pelletsPerShot = 1; // For shotguns
    [SerializeField] private float spreadAngle = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (shootableMask == -1)
            shootableMask = LayerMask.GetMask("Shootable");
    }
    
    protected override void DoFire(Vector3 origin, Vector3 direction)
    {
        // Fire multiple pellets for shotgun-style weapons
        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 shootDirection = direction;
            
            // Add spread for multiple pellets
            if (pelletsPerShot > 1)
            {
                float randomX = Random.Range(-spreadAngle, spreadAngle);
                float randomY = Random.Range(-spreadAngle, spreadAngle);
                shootDirection = Quaternion.Euler(randomX, randomY, 0) * direction;
            }
            
            FireRaycast(origin, shootDirection);
        }
    }
    
    private void FireRaycast(Vector3 origin, Vector3 direction)
    {
        Ray shootRay = new Ray(origin, direction);
        RaycastHit hit;
        
        Vector3 endPoint = origin + direction * Range;
        
        if (Physics.Raycast(shootRay, out hit, Range, shootableMask))
        {
            // Hit something
            endPoint = hit.point;
            
            // Damage enemy
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(Damage, hit.point, this);
                GameEvents.EnemyDamaged(enemyHealth.gameObject, Damage, hit.point);
            }
            
            // Impact effects
            if (weaponData.impactEffect != null)
            {
                Instantiate(weaponData.impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
        
        // Show bullet trail
        if (showBulletTrail && bulletTrail != null)
        {
            StartCoroutine(ShowBulletTrail(origin, endPoint));
        }
    }
    
    private System.Collections.IEnumerator ShowBulletTrail(Vector3 start, Vector3 end)
    {
        if (bulletTrail != null)
        {
            bulletTrail.enabled = true;
            bulletTrail.positionCount = 2;
            bulletTrail.SetPosition(0, start);
            bulletTrail.SetPosition(1, end);
            
            yield return new WaitForSeconds(trailDuration);
            
            bulletTrail.enabled = false;
        }
    }
    
    // Configuration methods for different weapon types
    public void ConfigureAsShotgun(int pellets, float spread)
    {
        pelletsPerShot = pellets;
        spreadAngle = spread;
    }
    
    public void ConfigureAsPistol()
    {
        pelletsPerShot = 1;
        spreadAngle = 0f;
    }
    
    public void ConfigureAsRifle()
    {
        pelletsPerShot = 1;
        spreadAngle = 0f;
    }
}
