using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRegen : MonoBehaviour {
    [SerializeField]
    private float cooldown = 1;
    [SerializeField]
    private int regenAmount = 1;
    private EnemyHealth enemyHealth;
	// Use this for initialization
	void Start () {
        enemyHealth = gameObject.GetComponent<EnemyHealth>();
        InvokeRepeating("Regenerate", 0f, cooldown);
 
	}
	void Regenerate()
    {
        if(enemyHealth.currentHealth < enemyHealth.startingHealth && !enemyHealth.isDead) {
            enemyHealth.currentHealth += regenAmount;
        }
    }
	// Update is called once per frame
	void Update () {
 

    }
}
