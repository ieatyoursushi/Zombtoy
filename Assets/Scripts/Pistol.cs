using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class Pistol : MonoBehaviour {
    public GameObject IceBullet;
    public float cooldown;
    float timer;
    public AudioSource Shoot;
    public Ammo ammoScript;
    public PlayerHealth playerHealth;
    // Use this for initialization

 
    // Update is called once per frame
    void Update () {
        timer += Time.deltaTime;
        if(Input.GetButton("Fire1") && (ammoScript.ammo == ammoScript.maxAmmo || cooldown <= timer) && timer != 0)
        {
            if (ammoScript.TryShoot() && ammoScript.reloadTimer == 0 && !playerHealth.isDead)
            {
                Instantiate(IceBullet, transform.position, transform.rotation);
                Shoot.Play();
                timer = 0f;
            }
        }
    }
}
