using UnityEngine;
using Aegis.Core;
using Aegis.Weapons;
using Aegis.Systems;

namespace Aegis.Player
{
    /// <summary>
    /// Bridges live gameplay to the HUD. Pushes ammo + weapon name (from WeaponHandler), the
    /// interact prompt (from InteractionController), and flashes the damage overlay (from
    /// HealthSystem) into the <see cref="UIManager"/>.
    ///
    /// Health bar, objective banner and crisis timer are driven elsewhere (HealthSystem's
    /// event channel and CrisisManager). This binder fills the rest.
    /// Tier 5.
    /// </summary>
    public class HudBinder : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find on this/parent object.")]
        [SerializeField] private WeaponHandler _weapons;
        [SerializeField] private InteractionController _interaction;
        [SerializeField] private HealthSystem _health;

        private void Awake()
        {
            if (_weapons == null) _weapons = GetComponentInParent<WeaponHandler>();
            if (_interaction == null) _interaction = GetComponentInParent<InteractionController>();
            if (_health == null) _health = GetComponentInParent<HealthSystem>();
        }

        private void OnEnable()
        {
            if (_weapons != null)
            {
                _weapons.WeaponEquipped += OnWeaponChanged;
                _weapons.AmmoChanged += OnWeaponChanged;
                _weapons.WeaponFired += OnWeaponChanged;
                OnWeaponChanged(_weapons.CurrentWeapon); // push initial values
            }

            if (_interaction != null) _interaction.TargetChanged += OnTargetChanged;
            if (_health != null) _health.DamageTaken += OnDamage;
        }

        private void OnDisable()
        {
            if (_weapons != null)
            {
                _weapons.WeaponEquipped -= OnWeaponChanged;
                _weapons.AmmoChanged -= OnWeaponChanged;
                _weapons.WeaponFired -= OnWeaponChanged;
            }

            if (_interaction != null) _interaction.TargetChanged -= OnTargetChanged;
            if (_health != null) _health.DamageTaken -= OnDamage;
        }

        private void OnWeaponChanged(Weapon weapon)
        {
            UIManager ui = UIManager.Instance;
            if (ui == null) return;

            if (weapon == null)
            {
                ui.SetWeaponName(string.Empty);
                ui.SetAmmo(0, 0);
                return;
            }

            ui.SetWeaponName(weapon.Data.displayName);
            ui.SetAmmo(weapon.CurrentAmmo, weapon.Data.ammo);
        }

        private void OnTargetChanged(IInteractable target)
        {
            UIManager ui = UIManager.Instance;
            if (ui == null) return;

            if (target == null || string.IsNullOrEmpty(target.Prompt))
                ui.HideInteractPrompt();
            else
                ui.ShowInteractPrompt(target.Prompt);
        }

        private void OnDamage(float amount)
        {
            UIManager ui = UIManager.Instance;
            if (ui == null) return;

            // Scale the flash a bit by hit size (25 dmg = full flash).
            ui.FlashDamage(Mathf.Clamp01(amount / 25f));
        }
    }
}
