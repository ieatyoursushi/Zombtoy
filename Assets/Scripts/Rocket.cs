using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Video;

public class Rocket : MonoBehaviour, IBlast, IProjectile {
    Vector3 ExplosionPosition;
    public float speed;
    public GameObject playerTransform;
    public GameObject Rockets;
    public Transform rocketLauncher;
    public ParticleSystem explosion;
    public ParticleSystem Trail;
    public LayerMask shootable;
    public bool collided { get; set; }
    bool exploded = false;
    bool absorbed;
    public AudioSource ExplosionSound;
    public int directHitDMG;
    public int radiusDMG;
    // Outer blast: lower damage but larger radius
    public int outerRadiusDMG;
    public float outerRadius = 0f;
    bool stopMovement = false;
    bool hit;
    bool ExplosiveHit;
    RaycastHit shootHit;
    Ray ray = new Ray();
    EnemyHealth enemyHealth;
    public GameObject ExplideLight;
    float LightRange = 0f;
    public AudioSource block;
    public float DestroyTime;
    public float explodeRadius;
    // Use this for initialization
    void Start () {
        playerTransform = GameObject.Find("Player");
        rocketLauncher = GameObject.Find("RocketLauncher").GetComponent<Transform>();
 
        explosion.Pause();
        ExplosionSound = GameObject.Find("ExplosionSound").GetComponent<AudioSource>();
        hit = false;
        collided = false;
        Trail = GetComponentInChildren<ParticleSystem>();
        block = GameObject.Find("block").GetComponent<AudioSource>();
        Destroy(gameObject, 30f);
    }
    public void velocity()
    {
        Vector3 rocketMovement = new Vector3(0f, 0f, speed);
        if (!stopMovement)
        {
            gameObject.transform.Translate(rocketMovement * speed * Time.deltaTime);
        }
    }
	// Update is called once per frame
	void FixedUpdate() {
        velocity();
    }
    private void Update()
    {
        RocketCollisionMonitor();

        if (collided)
        {

            if (!exploded)
            {
                Explode();
                exploded = true;
            }

            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
            explosion.Play();
            if (absorbed)
            {
                explosion.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
            Destroy(gameObject, DestroyTime);
            stopMovement = true;
            InvokeRepeating("LightLerp", 0f, 0.03f);
        }

    }
    bool lighting = false;

    void LightLerp()
    {

        ExplideLight.GetComponent<Light>().range = LightRange;
        if (lighting == false)
        {
            LightRange+=4;
            
        } else
        {
            if (LightRange != 0)
            {
                LightRange--;
            }
        }
        if(LightRange >= 20)
        {
            lighting = true;
        }
    }
    void RocketCollisionMonitor()
    {
        //for raycast
        ray.origin = transform.position;
        ray.direction = Vector3.forward;
        //for spherecast

        LayerMask mask = LayerMask.GetMask("Shootable");
        if (Physics.SphereCast(transform.position, 0.3f, transform.forward, out shootHit,0.3f, mask) && hit == false)
        {
            hit = true;
            ExplosionSound.Play();
            EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

            //checks if there is an enemyhealth script
            if (enemyHealth != null && !enemyHealth.GetAttribute("blast_immunity"))
            {
                enemyHealth.TakeDamage(directHitDMG, shootHit.point, this);
            }
            else if (enemyHealth != null)
            {
                if (enemyHealth.GetAttribute("blast_immunity"))
                {
                    ExplosionSound.Stop();
                    absorbed = true;
                    block.Play();
                }
            }

            collided = true;
        }
        else
        {
            collided = false;
        }
    }
 
    public void Explode()
    {
        LayerMask mask = LayerMask.GetMask("Shootable");
        PlayerHealth playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        ExplosionPosition = gameObject.transform.position;
        // Inner blast (full damage)
        Collider[] innerHits = Physics.OverlapSphere(transform.position, explodeRadius, mask);
        var damagedEnemies = new System.Collections.Generic.HashSet<int>();
        foreach (Collider col in innerHits)
        {
            if (absorbed)
                return;
            if (col == null) continue;

            // Use GetComponentInParent to handle multi-collider enemies (child colliders)
            EnemyHealth enemyHealth = col.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null) continue;

            int eid = enemyHealth.GetInstanceID();
            if (damagedEnemies.Contains(eid))
                continue; // already damaged by another collider on same enemy

            damagedEnemies.Add(eid);

            if (ExplosiveHit == false && !enemyHealth.GetAttribute("blast_immunity"))
            {
                enemyHealth.TakeDamage(radiusDMG, ExplosionPosition, this);
            }
            else
            {
                if (enemyHealth.GetAttribute("blast_immunity"))
                {
                    block.Play();
                }
            }
        }

        // Outer blast (reduced damage) - apply only to targets not already hit by inner blast
        if (outerRadius > explodeRadius && outerRadiusDMG > 0)
        {
            Collider[] outerHits = Physics.OverlapSphere(transform.position, outerRadius, mask);
            foreach (Collider col in outerHits)
            {
                if (absorbed)
                    return;
                if (col == null) continue;

                EnemyHealth enemyHealth = col.GetComponentInParent<EnemyHealth>();
                if (enemyHealth == null) continue;

                int eid = enemyHealth.GetInstanceID();
                if (damagedEnemies.Contains(eid))
                    continue; // already damaged by inner blast

                damagedEnemies.Add(eid);

                if (!enemyHealth.GetAttribute("blast_immunity"))
                {
                    enemyHealth.TakeDamage(outerRadiusDMG, ExplosionPosition, this);
                }
                else
                {
                    if (enemyHealth.GetAttribute("blast_immunity"))
                    {
                        block.Play();
                    }
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawSphere(transform.position, explodeRadius);
        if (outerRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.08f);
            Gizmos.DrawSphere(transform.position, outerRadius);
        }
    }
}
