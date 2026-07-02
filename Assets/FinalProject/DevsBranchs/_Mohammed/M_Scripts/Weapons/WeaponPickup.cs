using UnityEngine;
using UnityEngine.Events;
using Aegis.Core;
using Aegis.Player;

namespace Aegis.Weapons
{
    /// <summary>
    /// A weapon lying in the world. The player looks at it and presses F to pick it up,
    /// adding the assigned <see cref="WeaponData"/> to their <see cref="WeaponHandler"/>.
    /// Faction is respected: a Good-only weapon refuses an Evil player and vice versa.
    /// <see cref="OnPickedUp"/> fires on a successful grab — wire it to end X's stealth intro
    /// (disable the StealthSystem), open a door, play a line, etc.
    /// Tier 2 — depends on Tier 0 (IInteractable, WeaponData) + Tier 2 (WeaponHandler).
    /// </summary>
    public class WeaponPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private WeaponData _weaponData;
        [Tooltip("Override the auto prompt (e.g. \"Take Arc Lance\"). Leave empty for default.")]
        [SerializeField] private string _promptOverride;

        [Tooltip("Fires once when this weapon is successfully picked up. Great for ending X's stealth.")]
        public UnityEvent OnPickedUp;

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

            if (handler.AddWeapon(_weaponData))
            {
                OnPickedUp?.Invoke();
                Destroy(gameObject);
            }
            // (If AddWeapon refused — wrong faction or already owned — the pickup stays.)
        }
    }
}
