using UnityEngine;
//rocket and icebullet projectile replacement, IProjecTile can either be an IWeapon or ISpell. IProjectile: instatiates in player direction and has a velocity vector 


public interface IProjectile
{
    public void velocity();
    bool collided { get; set; }

}
