using UnityEngine;

public interface ISpell
{
    string SpellName { get; }
    int ManaCost { get; }
    float CastTime { get; }
    float Cooldown { get; }

    void Cast(Vector3 origin);
}