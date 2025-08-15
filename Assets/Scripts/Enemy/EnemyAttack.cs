using UnityEngine;
using System.Collections;

public class EnemyAttack : MonoBehaviour
{
    public float timeBetweenAttacks = 0.5f;
    public int attackDamage = 10;
    [SerializeField] private EnemyHealth enemyHealth;

    Animator anim;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerHealth playerHealth;
    //EnemyHealth enemyHealth;
    bool playerInRange;
    float timer;

    void Awake ()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
        }
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
    }

    private void Start()
    {
        if(enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }
        if(anim == null)
        {
            anim = GetComponentInParent<Animator>();
        }
    }
    void OnTriggerEnter (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = true;
        }
    }


    void OnTriggerExit (Collider other)
    {
        if(other.gameObject == player)
        {
            playerInRange = false;
        }
    }


    void Update ()
    {
        timer += Time.deltaTime;

    if(timer >= timeBetweenAttacks && playerInRange && enemyHealth != null && enemyHealth.currentHealth > 0)
        {
            Attack ();
        }

    if(playerHealth != null && playerHealth.currentHealth <= 0)
        {
            anim.SetTrigger ("PlayerDead");
        }
    }


    void Attack ()
    {
        timer = 0f;

    if(playerHealth != null && playerHealth.currentHealth > 0)
        {
            playerHealth.TakeDamage (attackDamage);
        }
    }
}
