using System.Collections.Generic;
using UnityEngine;
using Aegis.Core;
using Aegis.Input;

namespace Aegis.Player
{
    /// <summary>
    /// A close-range melee swing (V / Left-Shoulder). On input it does a short overlap check
    /// in front of the camera and damages every <see cref="IDamageable"/> caught in it — so
    /// it's an instant close hit, not a projectile. Has a cooldown so you can't spam it, and
    /// never hits the player themselves. Works alongside guns; firing isn't blocked.
    /// Tier 2/5 — depends on Tier 0 (InputReader, IDamageable).
    /// </summary>
    public class MeleeAttack : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader _inputReader;

        [Header("Aim")]
        [Tooltip("Where the swing comes from / aims — usually the camera.")]
        [SerializeField] private Transform _origin;

        [Header("Hit [TUNABLE]")]
        [SerializeField] private float _range = 2f;       // how far forward the hit reaches
        [SerializeField] private float _radius = 0.6f;    // how wide the swing is
        [SerializeField] private float _damage = 35f;
        [SerializeField] private float _cooldown = 0.6f;
        [SerializeField] private LayerMask _hitMask = ~0;

        [Header("FX (optional)")]
        [SerializeField] private GameObject _swingVFX;
        [SerializeField] private AudioClip _swingSFX;

        private float _nextTime;

        private void OnEnable()
        {
            if (_inputReader != null) _inputReader.MeleeEvent += DoMelee;
        }

        private void OnDisable()
        {
            if (_inputReader != null) _inputReader.MeleeEvent -= DoMelee;
        }

        private void DoMelee()
        {
            if (Time.time < _nextTime) return;
            _nextTime = Time.time + _cooldown;

            Transform aim = _origin != null ? _origin : transform;

            // FX first so the swing feels responsive even if it hits nothing.
            if (_swingVFX != null) Instantiate(_swingVFX, aim.position + aim.forward * 0.5f, aim.rotation);
            if (_swingSFX != null && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(_swingSFX);

            // A sphere a bit in front of the camera = the "swing zone".
            Vector3 center = aim.position + aim.forward * _range;
            Collider[] hits = Physics.OverlapSphere(center, _radius, _hitMask, QueryTriggerInteraction.Ignore);

            HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();
            foreach (Collider col in hits)
            {
                // Skip our own body.
                if (col.transform.IsChildOf(transform.root)) continue;

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target != null && alreadyHit.Add(target)) target.TakeDamage(_damage);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform aim = _origin != null ? _origin : transform;
            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.5f);
            Gizmos.DrawWireSphere(aim.position + aim.forward * _range, _radius);
        }
#endif
    }
}
