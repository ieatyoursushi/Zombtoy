using UnityEngine;
using System.Collections;
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private UnityEngine.AI.NavMeshAgent nav;

    void Awake()
    {
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }
        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }
        if (nav == null)
        {
            nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
    }


    void Update ()
    {
        if (enemyHealth != null && playerHealth != null && nav != null && player != null)
        {
            if (enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
            {
                // Only set destination if NavMeshAgent is enabled and on NavMesh
                if (nav.enabled && nav.isOnNavMesh)
                {
                    nav.SetDestination(player.position);
                }
            }
            else
            {
                nav.enabled = false;
            }
        }
        else if (nav != null)
        {
            nav.enabled = false;
        }
    }
}
