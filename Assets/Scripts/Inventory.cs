using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    public GameObject MachineGunPrefab;
    public GameObject ShotgunPrefab;

    public GameObject Shotgun;
    public GameObject MachineGun;
    public GameObject RocketLaunche;
    public GameObject CryoPistol;

    public AudioSource ShotgunAU;
    public AudioSource MachineGunAU;
    public AudioSource RocketLauncherAU;
    public AudioSource CryoPistolAU;

    public reloadCheck machineGunCheck;
    public reloadCheck shotgunCheck;
    public reloadCheck[] barrels;
    public reloadCheck pistolCheck;
    public reloadCheck rocketLauncherCheck;

    public PlayerHealth playerHealth;
    // not used (kept for compatibility)
    bool played = false; //machinegun
    bool played2 = false; //shotgun
    bool played3 = false; //rocketlauncher
    bool played4 = false; // cryo pistol
    public Text GunText;

    [System.Serializable]
    public class WeaponEntry
    {
        public string displayName;
        public GameObject weaponObject;
        public AudioSource equipAudio;
        public reloadCheck reloadChecker;
        public reloadCheck[] reloadChecks;
        public int uiFontSize = 64;
    }

    [Header("Data-driven weapons (fills from old fields if empty)")]
    public List<WeaponEntry> weapons = new List<WeaponEntry>();
    public int startIndex = 0;
    private int currentIndex = -1;
    private int lastPlayedIndex = -1;
    // Use this for initialization
    void Start () {
        // If the structured weapons list is empty, populate from old fields for compatibility
        if ((weapons == null || weapons.Count == 0))
        {
            weapons = new List<WeaponEntry>();
            weapons.Add(new WeaponEntry { displayName = "Machine Gun", weaponObject = MachineGun, equipAudio = MachineGunAU, reloadChecker = machineGunCheck, uiFontSize = 69 });
            weapons.Add(new WeaponEntry { displayName = "Shotgun", weaponObject = Shotgun, equipAudio = ShotgunAU, reloadChecker = shotgunCheck, reloadChecks = barrels, uiFontSize = 69 });
            weapons.Add(new WeaponEntry { displayName = "Rocket Launcher", weaponObject = RocketLaunche, equipAudio = RocketLauncherAU, reloadChecker = rocketLauncherCheck, uiFontSize = 55 });
            weapons.Add(new WeaponEntry { displayName = "Cryo Pistol", weaponObject = CryoPistol, equipAudio = CryoPistolAU, reloadChecker = pistolCheck, uiFontSize = 69 });
        }

        // Initialize all weapons to inactive
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].weaponObject != null)
                weapons[i].weaponObject.SetActive(false);
        }

        // Equip and activate the default weapon at start
        currentIndex = Mathf.Clamp(startIndex, 0, weapons.Count - 1);
        Equip(currentIndex);
        // Ensure the equipped weapon is active and ready to fire
        if (weapons[currentIndex].weaponObject != null)
            weapons[currentIndex].weaponObject.SetActive(true);
    }

    public void EquipIfValid(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        Equip(index);
    }

    public void Equip(int index)
    {
        if (playerHealth != null && playerHealth.isDead) return;
        if (index == currentIndex) return;
        if (index < 0 || index >= weapons.Count) return;

        // Deactivate all weapon objects
        for (int i = 0; i < weapons.Count; i++)
        {
            var wobj = weapons[i].weaponObject;
            if (wobj != null) wobj.SetActive(false);
        }

        var selected = weapons[index];
        if (selected.weaponObject != null) selected.weaponObject.SetActive(true);

        if (selected.equipAudio != null)
            selected.equipAudio.Play();

        if (selected.reloadChecker != null)
            selected.reloadChecker.reload = false;
        if (selected.reloadChecks != null)
        {
            foreach (var r in selected.reloadChecks)
                r.reload = false;
        }

        if (GunText != null)
        {
            GunText.text = selected.displayName;
            GunText.fontSize = selected.uiFontSize;
        }

        // update played flags for compatibility with other systems (kept semantics)
        played = (index == 0);
        played2 = (index == 1);
        played3 = (index == 2);
        played4 = (index == 3);

        currentIndex = index;
    }

    // Update is called once per frame
    void Update () {
        if (playerHealth != null && playerHealth.isDead) return;

        if (Input.GetKeyDown(Keybinds.slot1Bind)) EquipIfValid(0);
        if (Input.GetKeyDown(Keybinds.slot2Bind)) EquipIfValid(1);
        if (Input.GetKeyDown(Keybinds.slot3Bind)) EquipIfValid(2);
        if (Input.GetKeyDown(Keybinds.slot4Bind)) EquipIfValid(3);
    }
}
