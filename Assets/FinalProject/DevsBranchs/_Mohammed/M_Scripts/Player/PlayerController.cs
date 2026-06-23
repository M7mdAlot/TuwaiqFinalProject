using UnityEngine;
using Aegis.Core;

namespace Aegis.Player
{
    /// <summary>
    /// The "glue" on the Player root. Doesn't add new gameplay — it wires the parts we've
    /// already built so they cooperate:
    ///   • Slide i-frames: while MovementController says the player is sliding, the
    ///     HealthSystem is set to invulnerable (matches the GDD's "brief i-frames").
    ///   • Player death: when HealthSystem fires Died, tell GameManager to go to Ending.
    /// Tier 2 — composes Tier 1 components on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(MovementController))]
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerController : MonoBehaviour
    {
        private MovementController _movement;
        private HealthSystem _health;
        private WeaponHandler _weapons;
        private InteractionController _interaction;

        public MovementController Movement => _movement;
        public HealthSystem Health => _health;
        public WeaponHandler Weapons => _weapons;
        public InteractionController Interaction => _interaction;

        private void Awake()
        {
            _movement    = GetComponent<MovementController>();
            _health      = GetComponent<HealthSystem>();
            _weapons     = GetComponent<WeaponHandler>();
            _interaction = GetComponent<InteractionController>();
        }

        private void OnEnable()
        {
            if (_health != null) _health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Died -= OnDied;
        }

        private void Update()
        {
            // Slide i-frames: copy the movement's "is invincible right now" into the health system.
            if (_health != null && _movement != null)
                _health.IsInvulnerable = _movement.IsInvulnerable;
        }

        private void OnDied()
        {
            if (GameManager.Instance != null) GameManager.Instance.GoToEnding();
        }
    }
}
