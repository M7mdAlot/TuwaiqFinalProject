using UnityEngine;
using Aegis.Core;

namespace Aegis.Weapons
{
    /// <summary>
    /// A bullet/bolt that flies straight, deals damage to the first <see cref="IDamageable"/>
    /// it hits, then returns to its <see cref="ObjectPool{T}"/> for reuse. It checks for hits
    /// with a short raycast each frame, so even fast shots can't pass through thin walls.
    /// Tier 1 — depends only on Tier 0 (IDamageable, ObjectPool).
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float _lifetime = 5f;          // self-destruct if it hits nothing
        [SerializeField] private LayerMask _hitMask = ~0;       // what it can hit (~0 = everything)
        [SerializeField] private GameObject _hitVFX;            // optional impact effect

        private float _speed;
        private float _damage;
        private float _timeLeft;
        private ObjectPool<Projectile> _pool;

        /// <summary>Fire the projectile. Set its position/rotation before calling this.</summary>
        public void Launch(float speed, float damage, ObjectPool<Projectile> pool = null)
        {
            _speed = speed;
            _damage = damage;
            _pool = pool;
            _timeLeft = _lifetime;
        }

        private void Update()
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                Despawn(null);
                return;
            }

            float step = _speed * Time.deltaTime;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, step, _hitMask, QueryTriggerInteraction.Ignore))
            {
                IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
                target?.TakeDamage(_damage);
                Despawn(hit.point);
                return;
            }

            transform.position += transform.forward * step;
        }

        private void Despawn(Vector3? hitPoint)
        {
            if (_hitVFX != null && hitPoint.HasValue)
                Instantiate(_hitVFX, hitPoint.Value, Quaternion.identity);

            if (_pool != null) _pool.Release(this);
            else gameObject.SetActive(false);
        }
    }
}
