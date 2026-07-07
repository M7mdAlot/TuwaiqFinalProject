using UnityEngine;
using Aegis.Player;
using Aegis.Systems;
using Aegis.Weapons;

// Forwards Mohammed's HealthSystem/WeaponHandler events into his UIManager singleton.
// Needed because after swapping Aegis/X to his player scripts, GameplayUIController's
// old SimpleHealth/AegisWeaponController references no longer exist on those objects.
public class PlayerHudBridge : MonoBehaviour
{
    public HealthSystem health;
    public WeaponHandler weapons;

    void Awake()
    {
        if (health == null) health = GetComponent<HealthSystem>();
        if (weapons == null) weapons = GetComponent<WeaponHandler>();
    }

    void OnEnable()
    {
        if (health != null) health.HealthChanged += OnHealthChanged;

        if (weapons != null)
        {
            weapons.AmmoChanged += OnAmmoChanged;
            weapons.WeaponEquipped += OnWeaponEquipped;
        }
    }

    void OnDisable()
    {
        if (health != null) health.HealthChanged -= OnHealthChanged;

        if (weapons != null)
        {
            weapons.AmmoChanged -= OnAmmoChanged;
            weapons.WeaponEquipped -= OnWeaponEquipped;
        }
    }

    void Start()
    {
        if (health != null)
            OnHealthChanged(health.CurrentHealth, health.MaxHealth);

        if (weapons != null && weapons.CurrentWeapon != null)
            OnWeaponEquipped(weapons.CurrentWeapon);
    }

    void OnHealthChanged(float current, float max)
    {
        if (UIManager.Instance == null) return;
        UIManager.Instance.SetHealthNormalized(max > 0f ? current / max : 0f);
    }

    void OnAmmoChanged(Weapon w)
    {
        if (UIManager.Instance == null || w == null) return;
        UIManager.Instance.SetAmmo(w.CurrentAmmo, w.Data.ammo);
    }

    void OnWeaponEquipped(Weapon w)
    {
        if (UIManager.Instance == null || w == null) return;
        UIManager.Instance.SetWeaponName(w.Data.displayName);
        OnAmmoChanged(w);
    }
}
