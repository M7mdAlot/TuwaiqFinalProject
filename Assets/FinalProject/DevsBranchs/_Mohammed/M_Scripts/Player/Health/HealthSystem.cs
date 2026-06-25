using System;
using UnityEngine;
using Aegis.Core;
using Aegis.Core.Events;

namespace Aegis.Player
{
    /// <summary>
    /// Tracks health for any entity (player or enemy) and "signs" the <see cref="IDamageable"/>
    /// contract so weapons can hurt it. Raises events when damaged or killed and plays
    /// hurt/death sounds through the <see cref="AudioManager"/>.
    /// Tier 1 — depends only on Tier 0 (IDamageable, AudioManager, event channel).
    /// </summary>
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health [TUNABLE]")]
        [SerializeField] private float _maxHealth = 100f;

        [Header("Audio (optional)")]
        [SerializeField] private AudioClip _hurtSfx;
        [SerializeField] private AudioClip _deathSfx;

        [Header("Broadcast (optional)")]
        [Tooltip("If set, normalized health (0..1) is sent on this channel — handy for the health bar.")]
        [SerializeField] private FloatEventChannelSO _healthNormalizedChannel;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        /// <summary>When true, all damage is ignored (e.g. slide i-frames; set by PlayerController).</summary>
        public bool IsInvulnerable { get; set; }

        public event Action<float> DamageTaken;          // how much damage
        public event Action<float, float> HealthChanged; // current, max
        public event Action Died;

        private void Awake() => CurrentHealth = _maxHealth;

        private void Start() => RaiseHealthChanged(); // give listeners the starting value

        public void TakeDamage(float amount)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            DamageTaken?.Invoke(amount);
            RaiseHealthChanged();
            PlaySfx(_hurtSfx);

            if (CurrentHealth <= 0f) Die();
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            RaiseHealthChanged();
        }

        public void ResetHealth()
        {
            CurrentHealth = _maxHealth;
            RaiseHealthChanged();
        }

        private void Die()
        {
            PlaySfx(_deathSfx);
            Died?.Invoke();
        }

        private void RaiseHealthChanged()
        {
            HealthChanged?.Invoke(CurrentHealth, _maxHealth);
            if (_healthNormalizedChannel != null)
                _healthNormalizedChannel.Raise(_maxHealth > 0f ? CurrentHealth / _maxHealth : 0f);
        }

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clip);
        }
    }
}
