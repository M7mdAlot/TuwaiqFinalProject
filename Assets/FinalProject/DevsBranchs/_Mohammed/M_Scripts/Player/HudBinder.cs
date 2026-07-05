using UnityEngine;
using Aegis.Core;
using Aegis.Weapons;
using Aegis.Systems;

namespace Aegis.Player
{
    /// <summary>
    /// The bridge between live gameplay and the real HUD. It listens to the player's
    /// <see cref="WeaponHandler"/> and <see cref="InteractionController"/> and pushes their
    /// values into the <see cref="UIManager"/> (ammo, weapon name, interact prompt).
    ///
    /// Health, objective banner and crisis timer are already driven elsewhere:
    ///   • Health → HealthSystem raises the Float event channel that UIManager listens to.
    ///   • Objective + crisis timer → CrisisManager calls UIManager directly.
    /// So this binder just fills the remaining gaps.
    /// Tier 5 — depends on Tier 2 (WeaponHandler) + Tier 1 (InteractionController, UIManager).
    /// </summary>
    public class HudBinder : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find on this/parent object.")]
        [SerializeField] private WeaponHandler _weapons;
        [Tooltip("Leave empty to auto-find on this/parent object.")]
        [SerializeField] private InteractionController _interaction;

        private void Awake()
        {
            if (_weapons == null) _weapons = GetComponentInParent<WeaponHandler>();
            if (_interaction == null) _interaction = GetComponentInParent<InteractionController>();
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

            if (_interaction != null)
                _interaction.TargetChanged += OnTargetChanged;
        }

        private void OnDisable()
        {
            if (_weapons != null)
            {
                _weapons.WeaponEquipped -= OnWeaponChanged;
                _weapons.AmmoChanged -= OnWeaponChanged;
                _weapons.WeaponFired -= OnWeaponChanged;
            }

            if (_interaction != null)
                _interaction.TargetChanged -= OnTargetChanged;
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
    }
}
