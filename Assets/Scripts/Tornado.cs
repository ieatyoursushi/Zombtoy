using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tornado : MonoBehaviour {
    bool inTornado = false;
    // Toggle for new pull effect (set to false to disable)
    public bool pullEnemies = true;
    // Track enemies being pulled by this tornado
    private HashSet<GameObject> pulledEnemies = new HashSet<GameObject>();
    public GameObject coll;
    public float lifespan;
    public float speed = 4f;
    public bool inZone;
    public int damageDrain;
    public float pullStrength = 15f; // Adjustable pull force
    GameObject fireBall;
	// Use this for initialization
	void Start () {
        InvokeRepeating("Twist", 0.1f, 0.20f);
        Destroy(gameObject, lifespan);
        GetComponent<AudioSource>().Play();
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        if(inZone && fireBall != null)
        {
            Vector3 lookDirection = transform.position - fireBall.transform.position;
            lookDirection.y = 0f;
            fireBall.transform.localRotation = Quaternion.LookRotation(lookDirection);
        }else
        {

        }
	}
    private void OnTriggerEnter(Collider col)
    {
        EnemyProjectile enemyProjectile = col.GetComponent<EnemyProjectile>();
        fireBall = col.gameObject;
        if (enemyProjectile != null) {
            enemyProjectile.speed = 2.7f;
            inZone = true;
        }
        
        // Handle enemy entering tornado pull range
        if (pullEnemies)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.startingHealth <= 200) // Non-boss enemies only
            {
                GameObject enemy = col.gameObject;
                if (pulledEnemies.Add(enemy)) // Only process if not already tracked
                {
                    // Disable NavMeshAgent to allow physics control
                    UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && agent.enabled && agent.isOnNavMesh)
                    {
                        agent.enabled = false;
                    }
                       // Remove Y position and X/Z rotation constraints
                       Rigidbody rb = enemy.GetComponent<Rigidbody>();
                       if (rb != null)
                       {
                           rb.constraints &= ~(RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ);
                       }
                }
            }
        }
    }
    private void OnTriggerExit(Collider col)
    {
        EnemyProjectile enemyProjectile = col.GetComponent<EnemyProjectile>();
        if (enemyProjectile != null)
        {
            enemyProjectile.speed = 3.8f;
            inZone = false;
        }
        
        // Handle enemy leaving tornado pull range
        if (pullEnemies)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.startingHealth <= 200)
            {
                GameObject enemy = col.gameObject;
                if (pulledEnemies.Remove(enemy)) // Only process if was being tracked
                {
                    // Re-enable NavMeshAgent
                    UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                    if (agent != null && !agent.enabled)
                    {
                        agent.enabled = true;
                    }
                       // Restore Y position and X/Z rotation constraints
                       Rigidbody rb = enemy.GetComponent<Rigidbody>();
                       if (rb != null)
                       {
                           rb.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                       }
                }
            }
        }
    }
    void Twist()
    {
        LayerMask mask = LayerMask.GetMask("Shootable");
        Collider[] enemies = Physics.OverlapSphere(transform.position, 4f, mask);
        foreach(Collider col in enemies)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // Always deal damage to all enemies
                enemyHealth.TakeDamage(damageDrain, col.transform.position);
                enemyHealth.SlowEffect_Duration(0.3f);
                enemyHealth.SlowEffect(0.2f);
            }
        }
        
        // Apply pull forces to tracked enemies
        if (pullEnemies)
        {
            foreach(GameObject enemy in new List<GameObject>(pulledEnemies))
            {
                if (enemy != null)
                {
                    ApplyPullForce(enemy, pullStrength);
                }
            }
        }
        
        Collider[] ndSphere = Physics.OverlapSphere(transform.position, 10f, mask);
        foreach(Collider col in ndSphere)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(2, col.transform.position);
            }
        }
    }
    
    private void ApplyPullForce(GameObject enemy, float pullStrength)
    {
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 pullDir = (transform.position - enemy.transform.position).normalized;
            rb.AddForce(pullDir * pullStrength, ForceMode.Acceleration);
        }
    }
    
    // Re-enable NavMeshAgent on all tracked enemies when tornado is destroyed
    private void OnDestroy()
    {
        if (!pullEnemies) return;
        
        foreach (GameObject enemy in pulledEnemies)
        {
            if (enemy != null)
            {
                UnityEngine.AI.NavMeshAgent agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null && !agent.enabled)
                {
                    agent.enabled = true;
                }
                   // Restore Y position and X/Z rotation constraints
                   Rigidbody rb = enemy.GetComponent<Rigidbody>();
                   if (rb != null)
                   {
                       rb.constraints |= RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                   }
            }
        }
        
        pulledEnemies.Clear();
    }
}
public class Vornado : MonoBehaviour 
{
     
}
