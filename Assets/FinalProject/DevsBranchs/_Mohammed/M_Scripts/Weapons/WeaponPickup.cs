using UnityEngine;
using Aegis.Core;
using Aegis.Player;

namespace Aegis.Weapons
{
    /// <summary>
    /// A weapon lying in the world. The player looks at it and presses F to pick it up,
    /// adding the assigned <see cref="WeaponData"/> to their <see cref="WeaponHandler"/>.
    /// Faction is respected: a Good-only weapon refuses an Evil player and vice versa.
    /// Tier 2 — depends on Tier 0 (IInteractable, WeaponData) + Tier 2 (WeaponHandler).
    /// </summary>
    public class WeaponPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private WeaponData _weaponData;
        [Tooltip("Override the auto prompt (e.g. \"Take Arc Lance\"). Leave empty for default.")]
        [SerializeField] private string _promptOverride;

        public string Prompt =>
            !string.IsNullOrEmpty(_promptOverride) ? _promptOverride :
            _weaponData != null ? $"Press F to pick up {_weaponData.displayName}" : "Press F";

        public float HoldDuration => 0f;          // single press, not hold
        public bool CanInteract => _weaponData != null;

        public void Interact(GameObject interactor)
        {
            if (_weaponData == null || interactor == null) return;

            WeaponHandler handler = interactor.GetComponentInParent<WeaponHandler>();
            if (handler == null) return;

            if (handler.AddWeapon(_weaponData)) Destroy(gameObject);
            // (If AddWeapon refused — wrong faction or already owned — the pickup stays.)
        }
    }
}
