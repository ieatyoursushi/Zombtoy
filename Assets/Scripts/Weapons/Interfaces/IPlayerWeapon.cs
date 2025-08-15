using UnityEngine;

public interface IPlayerWeapon
{
    string WeaponName { get; }
    int AmmoCount { get; }
    float FireRate { get; }
    float ReloadTime { get; }

    void Fire(Vector3 origin, Vector3 direction);
    void Reload();
}
