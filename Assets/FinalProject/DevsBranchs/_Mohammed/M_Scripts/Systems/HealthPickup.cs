using UnityEngine;
using Aegis.Core;
using Aegis.Player;

namespace Aegis.Systems
{
    /// <summary>
    /// A healing item (medkit / repair cell) for the side rooms off the corridors. Restores
    /// health to whoever grabs it, then destroys itself. Two modes:
    ///   • Auto Pickup ON  → heals the moment the player walks into it (collider = trigger).
    ///   • Auto Pickup OFF → heals when the player looks at it and presses F (IInteractable).
    /// The collider is auto-configured for the chosen mode on Awake.
    /// Tier 2/5 — depends on Tier 1 (HealthSystem) + Tier 0 (IInteractable, AudioManager).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HealthPickup : MonoBehaviour, IInteractable
    {
        [Header("Heal")]
        [SerializeField] private float _healAmount = 25f;
        [Tooltip("Don't consume the pickup if the player is already at full health.")]
        [SerializeField] private bool _requireMissingHealth = true;

        [Header("Mode")]
        [Tooltip("ON = walk over to grab (trigger). OFF = look at it and press F.")]
        [SerializeField] private bool _autoPickup = true;
        [SerializeField] private string _prompt = "Press F to use medkit";

        [Header("FX (optional)")]
        [SerializeField] private GameObject _pickupVFX;
        [SerializeField] private AudioClip _pickupSFX;

        // IInteractable (only relevant when Auto Pickup is OFF).
        public string Prompt => _prompt;
        public float HoldDuration => 0f;           // instant, not hold
        public bool CanInteract => !_autoPickup;   // no F-prompt for walk-over pickups

        private void Awake()
        {
            // Auto pickups need a trigger collider; F-pickups need a solid one the ray can hit.
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = _autoPickup;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_autoPickup) TryHeal(other.gameObject);
        }

        public void Interact(GameObject interactor)
        {
            if (!_autoPickup) TryHeal(interactor);
        }

        private void TryHeal(GameObject who)
        {
            if (who == null) return;

            HealthSystem hp = who.GetComponentInParent<HealthSystem>();
            if (hp == null || !hp.IsAlive) return;

            // Skip if full and we only want to heal the wounded.
            if (_requireMissingHealth && hp.CurrentHealth >= hp.MaxHealth) return;

            hp.Heal(_healAmount);

            if (_pickupVFX != null) Instantiate(_pickupVFX, transform.position, Quaternion.identity);
            if (_pickupSFX != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(_pickupSFX);

            Destroy(gameObject);
        }
    }
}
