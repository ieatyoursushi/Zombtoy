using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItem : MonoBehaviour {
    public PlayerHealth playerHealth;
    public int AmmoMagazineMultiplyer;
    public AudioSource ammoPickup;
    public ItemManager itemManager;
    public Ammo machineGunAmmo;
    public Ammo shotgunAmmo;
    public Ammo[] barrels;
    public Ammo pistolAmmo;
    public Ammo rocketLauncherAmmo;
    public Transform gameObjectSpawnPoint;
    // Use this for initialization
    void Start () {
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
                    if (proxyType != null)
                    {
                        var mb = playerGO.AddComponent(proxyType) as MonoBehaviour;
                        var proxy = mb as PlayerHealth;
                        if (proxy != null)
                        {
                            playerGO.SendMessage("Bind", phr, SendMessageOptions.DontRequireReceiver);
                            playerHealth = proxy;
                        }
                    }
                }
            }
        }

        var im1 = GameObject.Find("ItemManager (1)");
        if (im1 != null) itemManager = im1.GetComponent<ItemManager>();
        var ammoSnd = GameObject.Find("AmmoSound");
        if (ammoSnd != null) ammoPickup = ammoSnd.GetComponent<AudioSource>();
	}
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            if(machineGunAmmo != null)
            {
                machineGunAmmo.Capacity += machineGunAmmo.maxAmmo;
 
            }
            if (shotgunAmmo != null)
            {
                shotgunAmmo.Capacity += shotgunAmmo.maxAmmo;
                foreach(Ammo Ammo in barrels)
                {
                    Ammo.Capacity += Ammo.maxAmmo;
                }
            }
            if (pistolAmmo != null)
            {
                pistolAmmo.Capacity += pistolAmmo.maxAmmo;
            }
            if (rocketLauncherAmmo != null)
            {
                rocketLauncherAmmo.Capacity += rocketLauncherAmmo.maxAmmo * 1;
            }

            if (ammoPickup != null) ammoPickup.Play();
 
            ScoreManager.Instance.AddScore(1);
            if (this.itemManager != null) this.itemManager.ItemAmount--;
            StartCoroutine(DestroyAndAddSpawnPoint());
            var col2 = gameObject.GetComponent<Collider>();
            if (col2 != null) col2.enabled = false;
             
        }
    }
    IEnumerator DestroyAndAddSpawnPoint()
    {
        yield return new WaitForSeconds(0f);
    if (itemManager == null || itemManager.spawnPoints == null) { Destroy(gameObject); yield break; }
    for(int i = 0; i < itemManager.spawnPoints.Length; i++)
        {
            if(itemManager.spawnPoints[i] == gameObjectSpawnPoint)
            {
                itemManager.SpawnPointsList.Add(itemManager.spawnPoints[i]);
            }
        }
        Destroy(gameObject);
    }
    private void Update()
    {
 
        transform.Rotate(0f,100f * Time.deltaTime, 0f, Space.Self);
 
        
    }

}
