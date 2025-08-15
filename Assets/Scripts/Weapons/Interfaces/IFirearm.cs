using UnityEngine;

public interface IFirearm : IPlayerWeapon
{
    string FirearmName { get; }
    int MagazineSize { get; }

}