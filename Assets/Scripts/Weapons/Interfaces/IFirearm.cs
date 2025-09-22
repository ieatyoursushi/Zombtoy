using UnityEngine;

public interface IRaycastWeapon : IPlayerWeapon
{
    string FirearmName { get; }
    int MagazineSize { get; }

}