using UnityEngine;

/// <summary>
/// Lightweight adapter that exposes legacy PlayerHealth API backed by PlayerHealthRefactored
/// Used to satisfy scripts expecting PlayerHealth when only the refactored component exists.
/// </summary>
public class PlayerHealthProxy : PlayerHealth
{
    private PlayerHealthRefactored _ref;

    public void Bind(PlayerHealthRefactored phr)
    {
        _ref = phr;
    }

    // Override relevant parts to forward to refactored component
    void Update()
    {
        if (_ref != null && healthSlider != null)
        {
            healthSlider.value = _ref.CurrentHealth;
        }
    }

    public new void Heal(int amount)
    {
        if (_ref != null)
            _ref.Heal(amount, true);
        else
            base.Heal(amount);
    }

    public new void TakeDamage(int amount)
    {
        if (_ref != null)
            _ref.TakeDamage(amount);
        else
            base.TakeDamage(amount);
    }

    public int GetCurrentHealth()
    {
        return _ref != null ? _ref.CurrentHealth : currentHealth;
    }
}
