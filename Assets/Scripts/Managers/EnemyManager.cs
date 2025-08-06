using UnityEngine;
using UnityEngine.UI;
public class EnemyManager : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public GameObject enemy;
    public float spawnTime;
    public Transform[] spawnPoints;
    public zombieCount ZombieCount;
    private float ShortestSpawnTime;
    void Start ()
    {
        InvokeRepeating ("Spawn", spawnTime, spawnTime);
        ZombieCount = GameObject.Find("ZombieCount").GetComponent<zombieCount>();
        ShortestSpawnTime = spawnTime * 0.60f;
         
        //gameObject.transform.localScale = new Vector3((0.5f * enemy.transform.localScale.x), (0.5f * enemy.transform.localScale.y), (0.5f * enemy.transform.localScale.z));
        Debug.Log(gameObject.transform.localScale);
    }
    private void Update()
    {
    }
    void Spawn ()
    {
        if(playerHealth.currentHealth <= 0f)
        {
            return;
        }

        int spawnPointIndex = Random.Range (0, spawnPoints.Length);
        if (ZombieCount.entityCount <= ZombieCount.maximumEntities)
        {
            Instantiate(enemy, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
            ZombieCount.entityCount++;
            if (gameObject.tag != "GIANT" && spawnTime >= ShortestSpawnTime)
            {
                spawnTime = spawnTime * 0.99f;
            }
        }
    }
}
