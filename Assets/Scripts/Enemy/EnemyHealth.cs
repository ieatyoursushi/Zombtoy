using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
/// <summary>
/// Enemy health system with event integration
/// Now properly integrated with the centralized systems
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    // General primitive dictionary for string-bool pairs
    [System.Serializable]
    //limit modularity to enemy health / bassic interactions like immunities, player debuffs, boosts.
    public struct EnemyAttributeEntry
    {
        public string keyAttribute;
        public bool isActive;
    }
    [SerializeField]
    private List<EnemyAttributeEntry> enemyAttributes = new List<EnemyAttributeEntry>() {
        new EnemyAttributeEntry { keyAttribute = "blast_immunity", isActive = false }
    };

    public int startingHealth = 100;
    public int currentHealth;
    public float sinkSpeed = 2.5f;
    public int scoreValue = 10;
    public AudioClip deathClip;
    public GameObject deathParticle;
    public Slider EnemyBar;
    Animator anim;
    AudioSource enemyAudio;
    ParticleSystem hitParticles;
    CapsuleCollider capsuleCollider;
    public bool isDead;
    bool effected = false;
    bool isSinking;
    public GameObject Camera;
    public ParticleSystem DeathParticle;
    public GameObject snowParticle;
    public GameObject HealthImage;
    NavMeshAgent navMeshAgent;
    public float NavAgent_Speed;
    float effects_Duration = 1.5f;
    float timer;
    float navSpeed;
    float size;
    public GameObject HPSlider;
    public zombieCount ZombieCount;
    public TornadoLaunch TornadoLaunch;
    public float coolDownReducer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        enemyAudio = GetComponent<AudioSource>();
        hitParticles = GetComponentInChildren<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        currentHealth = startingHealth;

        // Use safer lookups with null checks
        var zombieCountGO = GameObject.Find("ZombieCount");
        if (zombieCountGO != null)
            ZombieCount = zombieCountGO.GetComponent<zombieCount>();

        var tornadoGO = GameObject.Find("TornadoLauncher");
        if (tornadoGO != null)
            TornadoLaunch = tornadoGO.GetComponent<TornadoLaunch>();

        // Register with EnemyManager
        GameEvents.EnemySpawned(gameObject);
    }
    public float SlowEffect_Duration(float effectDuration)
    {
        effects_Duration = effectDuration;
        return effectDuration;
    }
    private void Start()
    {
        Camera = GameObject.Find("MainCamera");
        if (DeathParticle != null)
        {
            DeathParticle.Pause();
            deathParticle.SetActive(false);
        }
        if (EnemyBar != null)
        {
            EnemyBar.maxValue = currentHealth;
        }
        UnityEngine.AI.NavMeshAgent navAgent = gameObject.GetComponent<NavMeshAgent>();
        if (navAgent != null && navAgent.enabled)
        {
            NavAgent_Speed = navAgent.speed;
            navSpeed = NavAgent_Speed;
        }
        timer = effects_Duration;
        this.HPSlider.SetActive(false);
 
    }

    void Update()
    {
        if (isSinking)
        {
            transform.Translate(-Vector3.up * sinkSpeed * Time.deltaTime);
        }
        if (EnemyBar != null)
        {
            Vector3 FacingDirection = Camera.transform.eulerAngles;
            EnemyBar.transform.rotation = Quaternion.Euler(FacingDirection);
        }
        if (effected)
        {
            effects_Duration -= Time.deltaTime;
        }
        if (effects_Duration <= 0 && gameObject != null)
        {
            UnityEngine.AI.NavMeshAgent navAgent = gameObject.GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.enabled)
            {
                navAgent.speed = navSpeed;
            }
            effects_Duration = timer;
            effected = true;
        }
        if (EnemyBar != null)
        {
            EnemyBar.value = currentHealth;
        }
    }

//new parameter, weapon type, weapon type third parameter should be in an overload method
    public void TakeDamage(int amount, Vector3 hitPoint, object damageSource = null)
    {
        if (isDead || damageSource == null || amount <= 0)
            return;
        System.Type type = damageSource.GetType();
        System.Type[] interfaces = type.GetInterfaces();
        if (interfaces.Length > 0)
        {
            Debug.Log($"Damage source implements interface(s) of {type.Name}: " + string.Join(", ", interfaces.Select(i => i.Name)));
        }
        else
        {
            Debug.Log("Damage source type: " + type.Name + " (no interfaces implemented)");
        }
        if (damageSource is IBlast && GetAttribute("blast_immunity") == true)
        {
            return;
        }


        enemyAudio.Play();
        this.HPSlider.SetActive(true);
        currentHealth -= amount;
        hitParticles.transform.position = hitPoint;
        hitParticles.Play();


        if (currentHealth <= 0)
        {
            Death();
            this.HealthImage.GetComponentInChildren<Image>().color = new Color(1, 1, 1, 0.10f);
            gameObject.layer = LayerMask.GetMask("Default");
        }
    }
    public void SlowEffect(float amplifier)
    {
        effected = true; // starts the countdown
        UnityEngine.AI.NavMeshAgent navAgent = gameObject.GetComponent<NavMeshAgent>();
        if (navAgent != null && navAgent.enabled)
        {
            navAgent.speed = navSpeed * amplifier;
        }
        effects_Duration = timer;
    }
    void Death()
    {
        isDead = true;
        this.HPSlider.SetActive(false);
        capsuleCollider.isTrigger = true;
        TornadoLaunch.SetCoolDown(TornadoLaunch.timer + coolDownReducer);
        anim.SetTrigger("Dead");

        enemyAudio.clip = deathClip;
        enemyAudio.Play();
        // Monster kill will be handled by GameEvents when StartSinking is called
        if (DeathParticle != null)
        {
            Invoke("deathparticles", 0.2f);
        }
        // Counter updates are handled centrally via GameEvents and EnemyManager
    }
    void deathparticles()
    {
        deathParticle.SetActive(true);
        DeathParticle.Play();
    }

    public void StartSinking()
    {
        // Fire death event for score and kill count
        GameEvents.EnemyKilled(scoreValue, transform.position);

        UnityEngine.AI.NavMeshAgent navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        GetComponent<Rigidbody>().isKinematic = true;
        isSinking = true;

        Destroy(gameObject, 2f);
    }

    void OnDestroy()
    {
        // Ensure cleanup when destroyed
        GameEvents.EnemyDestroyed(gameObject);
    }
    //getters and setters
    public List<EnemyAttributeEntry> GetEnemyAttributesList()
    {
        return enemyAttributes;
    }
    //HasAttribute(blastImmunity)
    public bool GetAttribute(string key)
    {
        foreach (var entry in enemyAttributes)
        {
            if (entry.keyAttribute == key)
            {
                return entry.isActive;
            }
        }
        return false;
    }   
}
