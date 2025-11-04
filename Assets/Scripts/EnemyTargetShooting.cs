using UnityEngine;

//creates a crosshair visual on the ground locking on the player if in range until
//the player moves out of range or the shoot happens where the projectile will target that point in space
//similar script to EnemyShooting.cs like using a range child and instantiation of projectile, ground target should be a like visual from unity graphics 
//differences is no audio implementation and theres a delay with the corshair visual locking onto the player til shooting happens
public class EnemyTargetShooting : MonoBehaviour
{
    public GameObject Projectile;
    public Transform shootPoint;
    public float cooldown;
    PlayerHealth playerHealth;
    public range Range;
    public GameObject Player;
    EnemyHealth enemyHealth;
    public GameObject groundTarget;
    bool targetActive = false;
    Vector3 targetPosition;
    // Use this for initialization
    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }
    void Start()
    {
        InvokeRepeating("shoot", 1f, cooldown);
        playerHealth = GameObject.Find("Player").GetComponent<PlayerHealth>();
        Player = GameObject.Find("Player");
        groundTarget.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Range.inRange && !targetActive)
        {
            targetActive = true;
            groundTarget.SetActive(true);
        }
        else if (!Range.inRange && targetActive)
        {
            targetActive = false;
            groundTarget.SetActive(false);
        }
        if (targetActive)
        {
            targetPosition = new Vector3(Player.transform.position.x, groundTarget.transform.position.y, Player.transform.position.z);
            groundTarget.transform.position = Vector3.Lerp(groundTarget.transform.position, targetPosition, Time.deltaTime * 5f);
        }
    }
    void shoot()
    {
        if (Range.inRange && enemyHealth.currentHealth > 0 && playerHealth.currentHealth > 0)
        {
            Vector3 AimLine = groundTarget.transform.position - shootPoint.position;
            
            Debug.DrawLine(groundTarget.transform.position, shootPoint.position);
            shootPoint.rotation = Quaternion.LookRotation(AimLine);
            Instantiate(Projectile, shootPoint.position, shootPoint.rotation);
        }
    }
}


//next up: rocket variant for the enemy 