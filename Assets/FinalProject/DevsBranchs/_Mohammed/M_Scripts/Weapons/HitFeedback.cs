using UnityEngine;
using Aegis.Player;

namespace Aegis.Weapons
{
    /// <summary>
    /// Spawns visual effects when the actor it watches takes damage or dies. Drop this onto
    /// any character (player or enemy) that should react visibly to combat — it auto-finds
    /// a <see cref="HealthSystem"/> on the same/parent GameObject and listens for its events.
    /// Tier 2 — depends on Tier 1 (HealthSystem).
    /// </summary>
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] private HealthSystem _health;
        [SerializeField] private GameObject _hitVFX;
        [SerializeField] private GameObject _deathVFX;

        private void Awake()
        {
            if (_health == null) _health = GetComponentInParent<HealthSystem>();
        }

        private void OnEnable()
        {
            if (_health == null) return;
            _health.DamageTaken += OnDamage;
            _health.Died += OnDeath;
        }

        private void OnDisable()
        {
            if (_health == null) return;
            _health.DamageTaken -= OnDamage;
            _health.Died -= OnDeath;
        }

        private void OnDamage(float amount)
        {
            if (_hitVFX != null) Instantiate(_hitVFX, transform.position, Quaternion.identity);
        }

        private void OnDeath()
        {
            if (_deathVFX != null) Instantiate(_deathVFX, transform.position, Quaternion.identity);
        }
    }
}
