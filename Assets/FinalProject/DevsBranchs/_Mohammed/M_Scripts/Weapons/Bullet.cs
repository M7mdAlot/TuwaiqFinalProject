using UnityEngine;
using Aegis.Core;

namespace Aegis.Weapons
{
    /// <summary>
    /// A physical bullet. Put this on an empty GameObject that has a Sphere Collider, and
    /// make your sci-fi particle effect a CHILD of it (the particle is purely visual). The
    /// bullet flies forward and damages the first thing it collides with — so the hit lands
    /// when the bullet actually reaches the target (no hitscan).
    ///
    /// A Rigidbody is required for trigger callbacks and is added/configured automatically
    /// (gravity off, continuous collision so fast bullets don't pass through thin objects).
    /// The bullet uses TRIGGER detection only (OnTriggerEnter) — it never physically pushes
    /// anything, so enemies don't get shoved when shot. Any Sphere Colliders on this object
    /// are forced to Is Trigger = true on Awake to make this foolproof.
    /// Targets just need a Collider + a HealthSystem (IDamageable).
    /// Tier 1 — depends only on Tier 0 (IDamageable, ObjectPool).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float _speed = 50f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private float _lifetime = 5f;       // despawn if it hits nothing
        [SerializeField] private GameObject _hitVFX;         // optional impact effect
        [Tooltip("Seconds before the spawned hit VFX is force-destroyed. Use -1 to leave cleanup entirely to the VFX prefab itself.")]
        [SerializeField] private float _hitVFXLifetime = 2f;

        private Rigidbody _rb;
        private ObjectPool<Bullet> _pool;
        private float _timeLeft;
        private bool _hasHit;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.isKinematic = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // anti-tunneling

            // Force every collider on this bullet to be a trigger so it can never push things.
            foreach (Collider col in GetComponents<Collider>())
                col.isTrigger = true;
        }

        private void OnEnable()
        {
            _hasHit = false;
            _timeLeft = _lifetime;
            _rb.linearVelocity = transform.forward * _speed; // fly straight ahead
        }

        /// <summary>Fire the bullet. Set position/rotation BEFORE calling so it flies the right way.</summary>
        public void Launch(float speed, float damage, ObjectPool<Bullet> pool = null)
        {
            _speed = speed;
            _damage = damage;
            _pool = pool;
            _hasHit = false;
            _timeLeft = _lifetime;
            _rb.linearVelocity = transform.forward * _speed;
        }

        /// <summary>
        /// Tell the physics engine that every collider on <paramref name="shooter"/> (and its
        /// children) should be ignored by this bullet. Use this so a player aiming straight
        /// down doesn't shoot themselves, and an enemy doesn't shoot itself either.
        /// </summary>
        public void IgnoreShooter(GameObject shooter)
        {
            if (shooter == null) return;

            Collider[] ownColliders = GetComponentsInChildren<Collider>(includeInactive: true);
            Collider[] shooterColliders = shooter.GetComponentsInChildren<Collider>(includeInactive: true);

            foreach (Collider mine in ownColliders)
            foreach (Collider theirs in shooterColliders)
                if (mine != null && theirs != null)
                    Physics.IgnoreCollision(mine, theirs, true);
        }

        private void Update()
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) Despawn(transform.position);
        }

        // Trigger-only: bullets detect contact but never push anything.
        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other, transform.position);
        }

        private void HandleHit(Collider hit, Vector3 point)
        {
            if (_hasHit) return; // never damage twice
            _hasHit = true;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null) target.TakeDamage(_damage);

            Despawn(point);
        }

        private void Despawn(Vector3 point)
        {
            if (_hitVFX != null)
            {
                GameObject vfx = Instantiate(_hitVFX, point, Quaternion.identity);
                if (_hitVFXLifetime >= 0f) Destroy(vfx, _hitVFXLifetime);
                // (If the VFX prefab has its own Stop Action: Destroy, whichever fires first wins.)
            }

            _rb.linearVelocity = Vector3.zero;

            if (_pool != null) _pool.Release(this);
            else Destroy(gameObject);
        }
    }
}
