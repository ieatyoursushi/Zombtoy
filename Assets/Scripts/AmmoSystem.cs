using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple, centralized ammo system that reduces coupling between weapons and ammo management
/// Backwards compatible - works alongside existing Ammo.cs
/// </summary>
public class AmmoSystem : MonoBehaviour 
{
    [Header("Ammo Configuration")]
    public int currentAmmo = 30;
    public int maxAmmoInClip = 30;
    public int totalAmmo = 120;
    public float reloadTime = 2.0f;

    [Header("UI")]
    public Text ammoDisplay;

    [Header("Audio")]
    public AudioSource reloadSound;

    // State
    public bool IsReloading { get; private set; }
    public bool HasAmmo => currentAmmo > 0;
    public bool CanReload => !IsReloading && totalAmmo > 0 && currentAmmo < maxAmmoInClip;

    // Events for decoupling
    public System.Action<int, int> OnAmmoChanged; // current, total
    public System.Action OnReloadStarted;
    public System.Action OnReloadCompleted;
    public System.Action OnAmmoEmpty;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // Handle reload input
        if (Input.GetKeyDown(Keybinds.reloadBind) && CanReload)
        {
            StartCoroutine(ReloadRoutine());
        }

        // Auto-reload when empty
        if (currentAmmo <= 0 && CanReload)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    /// <summary>
    /// Try to consume ammo. Returns true if successful.
    /// </summary>
    public bool TryShoot(int ammoToConsume = 1)
    {
        if (IsReloading || currentAmmo < ammoToConsume)
            return false;

        currentAmmo -= ammoToConsume;
        
        if (currentAmmo <= 0)
        {
            OnAmmoEmpty?.Invoke();
        }

        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        UpdateUI();
        return true;
    }

    IEnumerator ReloadRoutine()
    {
        if (IsReloading) yield break;

        IsReloading = true;
        OnReloadStarted?.Invoke();

        if (reloadSound != null)
            reloadSound.Play();

        yield return new WaitForSeconds(reloadTime);

        // Calculate how much ammo to reload
        int ammoNeeded = maxAmmoInClip - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, totalAmmo);

        currentAmmo += ammoToReload;
        totalAmmo -= ammoToReload;

        IsReloading = false;
        OnReloadCompleted?.Invoke();
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoDisplay != null)
        {
            ammoDisplay.text = $"{currentAmmo}/{totalAmmo}";
        }
    }

    // Utility methods for backwards compatibility
    public void SetAmmo(int clip, int total)
    {
        currentAmmo = clip;
        totalAmmo = total;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        UpdateUI();
    }

    public void AddAmmo(int amount)
    {
        totalAmmo += amount;
        OnAmmoChanged?.Invoke(currentAmmo, totalAmmo);
        UpdateUI();
    }
}
