using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class RocketLauncher : MonoBehaviour {
    public GameObject Rocket;
    public float cooldown;
    float timer;
    public AudioSource RocketLaunch;
    public float reloadTimer;
    public Ammo ammoScript;
    public PlayerHealth playerHealth;
    // Use this for initialization
    void Start () {
        RocketLaunch = GameObject.Find("RocketSound").GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        if (reloadTimer > 0)
        {
            reloadTimer -= Time.deltaTime;
        }
        if (reloadTimer < 0)
        {
            reloadTimer = 0;
        }
        timer += Time.deltaTime;
		if(Input.GetButton("Fire1") && (ammoScript.ammo == ammoScript.maxAmmo || cooldown <= timer) && timer != 0 && ammoScript.CanShoot() && !playerHealth.isDead)
        {
            if (ammoScript.TryShoot() && ammoScript.reloadTimer == 0 )
            {
                RocketLaunch.Play();
                Instantiate(Rocket, transform.position, transform.rotation);
                timer = 0f;
            }
        } 
    }

}
