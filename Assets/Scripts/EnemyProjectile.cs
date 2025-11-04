using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
//selection for type like rocket or arrow or fireball or poision ball
[System.Serializable]
public struct ProjectileType
{
    public string projectileName;
    public bool isActive;
}
public class EnemyProjectile : MonoBehaviour
{
    public int directHitDamage = 40;
    public int areaDamage = 20;
    private bool hasExploded = false;
    Vector3 bulletMovement;
    public float speed;
    public int DamagePerShot;
    RaycastHit shot;
    RaycastHit shootHit;
    public bool hit = false;
    bool collided = false;
    public GameObject particle;
    [SerializeField]
    private List<ProjectileType> projectileTypes = new List<ProjectileType>()
    {
        new ProjectileType { projectileName = "Rocket", isActive = false },
        new ProjectileType { projectileName = "Fireball", isActive = false }
    };
    // Use this for initialization
    void Start()
    {
        GetComponentInChildren<ParticleSystem>().Play();
        Destroy(gameObject, 10f);
    }
    void attack()
    {
        if (projectileTypes[1].isActive)
        {
            Collider[] collider = Physics.OverlapSphere(transform.position, 0.6f);
            foreach (Collider col in collider)
            {
                if (hit == false)
                {
                    collided = true;
                    PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                    if (playerHealth != null && !playerHealth.isDead)
                    {
                        playerHealth.TakeDamage(DamagePerShot);
                        hit = true;
                        Destroy(gameObject);
                    }
                }
            }
        } else if (projectileTypes[0].isActive)
        {
            float directHitRadius = 0.3f;
            float explosionRadius = 3.0f;
            LayerMask mask = LayerMask.GetMask("Shootable");
            ParticleSystem explosion = null;
            Transform explosionParticleSystem = gameObject.transform.Find("CFX_Explosion_B_Smoke+Text");
            if (explosionParticleSystem != null)
                explosion = explosionParticleSystem.GetComponent<ParticleSystem>();

            if (hasExploded) return;

            // Check for any collision with Shootable layer (player or environment)
            Collider[] directHits = Physics.OverlapSphere(transform.position, directHitRadius, mask);
            bool didDirectHit = false;
            PlayerHealth directPlayer = null;
            foreach (Collider col in directHits)
            {
                PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
                if (playerHealth != null && !playerHealth.isDead)
                {
                    didDirectHit = true;
                    directPlayer = playerHealth;
                    break;
                }
            }
            // If any collision (player or environment), explode
            if (directHits.Length > 0)
            {
                hasExploded = true;
                if (explosion != null)
                {
                    explosion.transform.parent = null; // Detach from rocket
                    explosion.transform.position = transform.position;
                    explosion.Stop();
                    explosion.Clear();
                    explosion.Play();
                    Destroy(explosion.gameObject, explosion.main.duration);
                }
                var mesh = GetComponent<MeshRenderer>();
                if (mesh != null) mesh.enabled = false;
                speed = 0f;
                // Case 1: Direct hit to player
                if (didDirectHit && directPlayer != null)
                {
                    directPlayer.TakeDamage(directHitDamage);
                }
                // Area of effect damage (case 1 and 2)
                Collider[] areaHits = Physics.OverlapSphere(transform.position, explosionRadius, mask);
                var damaged = new HashSet<int>();
                if (didDirectHit && directPlayer != null)
                    damaged.Add(directPlayer.GetInstanceID());
                foreach (Collider areaCol in areaHits)
                {
                    PlayerHealth areaPH = areaCol.GetComponent<PlayerHealth>();
                    if (areaPH != null && !areaPH.isDead && !damaged.Contains(areaPH.GetInstanceID()))
                    {
                        areaPH.TakeDamage(areaDamage);
                        damaged.Add(areaPH.GetInstanceID());
                    }
                }
                Destroy(gameObject, 0.1f);
                hit = true;
                collided = true;
                return;
            }
            // Case 3: No hit at all, rocket keeps flying
        }

    }
    // Update is called once per frame
    void FixedUpdate()
    {
        bulletMovement.Set(0f, 0f, speed);
        transform.Translate(bulletMovement * speed * Time.deltaTime);
        attack();
    }
}
