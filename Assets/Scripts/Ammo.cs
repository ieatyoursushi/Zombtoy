using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ammo : MonoBehaviour {
    public int ammo;
    public int Capacity;
    public int Max_Capacity;
    public float reload_Time;
    public int maxAmmo;
    public AudioSource reloadAudio;
    public bool reloaded;
    float timer;
    public Text ammoText;
    public float reloadTimer;
    public Ammo ammoScript;
    public reloadCheck ReloadCheck;
    // Use this for initialization
    void Start () {
        maxAmmo = ammo;
        reloaded = false;
	}
	
    // Simple method to check if shooting is allowed
    public bool CanShoot()
    {
        return ammo > 0 && !ReloadCheck.reload;
    }

    // Safe method to consume ammo
    public bool TryShoot(int ammoToUse = 1)
    {
        if (CanShoot() && ammo >= ammoToUse)
        {
            ammo -= ammoToUse;
            return true;
        }
        return false;
    }

    // Centralized reload check
    public bool CanReload()
    {
        return !ReloadCheck.reload && ammo < maxAmmo && Capacity > 0;
    }

	// Update is called once per frame
	void Update () {
        ammoText.text = ammo.ToString() + "/" + Capacity.ToString();
        
        if (Input.GetKeyDown(Keybinds.reloadBind) && CanReload())
        {
            StartCoroutine(reload());
            
            if (reloadAudio != null)
            {
                reloadAudio.Play();
            }
 
        }
        // reload if ammo is zero
        if (ammo == 0 && CanReload())
        {
            StartCoroutine(reload());

            if (reloadAudio != null)
            {
                reloadAudio.Play();
            }
        }
        if (Capacity >= Max_Capacity)
        {
            Capacity = Max_Capacity;
        }
 
 
    }
 
    IEnumerator reload()
    {
        ReloadCheck.reload = true;

        yield return new WaitForSeconds(reload_Time);
        int ammoAdd = maxAmmo - ammo;
        if (Capacity > 0 && ammo < maxAmmo && Capacity >= ammoAdd )
        {
            ammo = ammo + ammoAdd;
            Capacity = Capacity - ammoAdd;
        }  else
        {
            if (ammo < maxAmmo && Capacity < ammoAdd)
            {
                ammo = ammo + Capacity;
                Capacity -= Capacity;
                reloaded = false;
            }
        }
 
        ReloadCheck.reload = false;

        /*        
        for (int i = ammo; ammo < maxAmmo && Capacity > 0; i++) //keep adding ammo until it reaches magazine
        {
            ammo++;
            Capacity--;
            Debug.Log("Bullets_Reloaded");
        }
        */
    }
}
