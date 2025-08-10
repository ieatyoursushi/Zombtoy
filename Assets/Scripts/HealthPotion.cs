using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthPotion : MonoBehaviour {
    public PlayerHealth playerHealth;
    public int heal;
    public AudioSource healing;
    public Transform gameObjectSpawnPoint;
    // Use this for initialization
    void Start () {
        // will find the components that are required (null-safe)
        if (playerHealth == null)
        {
            var playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                playerHealth = playerGO.GetComponent<PlayerHealth>();
                if (playerHealth == null)
                {
                    var phr = playerGO.GetComponent<PlayerHealthRefactored>();
                    if (phr != null)
                    {
                        var proxyType = System.Type.GetType("PlayerHealthProxy");
                        PlayerHealth proxy = null;
                        if (proxyType != null)
                        {
                            var mb = playerGO.AddComponent(proxyType) as MonoBehaviour;
                            proxy = mb as PlayerHealth;
                        }
                        if (proxy != null)
                        {
                            // Try to bind via SendMessage to avoid direct type ref
                            playerGO.SendMessage("Bind", phr, SendMessageOptions.DontRequireReceiver);
                            playerHealth = proxy;
                        }
                    }
                }
            }
        }

        if (healing == null)
        {
            var ha = GameObject.Find("HealthAudio");
            if (ha != null) healing = ha.GetComponent<AudioSource>();
        }

        if (itemManager == null)
        {
            var im = GameObject.Find("ItemManager");
            if (im != null) itemManager = im.GetComponent<ItemManager>();
        }

	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public ItemManager itemManager;
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Player")
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(heal);
            }
            if (healing != null) healing.Play();
            if (itemManager != null) this.itemManager.ItemAmount--;
            ScoreManager.Instance?.AddScore(3);
            StartCoroutine(DestroyAndAddSpawnPoint());
            var col2 = gameObject.GetComponent<Collider>();
            if (col2 != null) col2.enabled = false;
        }
    }
    IEnumerator DestroyAndAddSpawnPoint()
    {
        yield return new WaitForSeconds(0f);
    if (itemManager == null || itemManager.spawnPoints == null) { Destroy(gameObject); yield break; }
    for (int i = 0; i < itemManager.spawnPoints.Length; i++)
        {
            if (itemManager.spawnPoints[i] == gameObjectSpawnPoint)
            {
                itemManager.SpawnPointsList.Add(itemManager.spawnPoints[i]);
            }
        }
        Destroy(gameObject);
    }
}
