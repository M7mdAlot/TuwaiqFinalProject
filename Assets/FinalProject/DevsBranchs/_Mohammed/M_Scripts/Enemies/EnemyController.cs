using UnityEngine;
using UnityEngine.AI;
using Aegis.Core;
using Aegis.Player;
using Aegis.Weapons;

namespace Aegis.Enemies
{
    /// <summary>The four moods an enemy can be in.</summary>
    public enum EnemyState { Idle, Alert, Engage, Dead }

    /// <summary>
    /// A simple NavMesh enemy with a state machine:
    ///   • Idle   → standing around, looking for the player.
    ///   • Alert  → spotted the player; chases them.
    ///   • Engage → in range and shooting Bullet prefabs from the assigned weapon.
    ///   • Dead   → HealthSystem hit zero; agent off and the body cleans up.
    /// Reads tunables (speed, ranges, weapon) from <see cref="EnemyData"/> and uses
    /// <see cref="HealthSystem"/> for HP. Faction comes from the data and is what
    /// <see cref="FactionSystem"/> uses to decide hostility.
    /// Tier 2 — depends on Tier 0 (EnemyData) + Tier 1 (HealthSystem, Bullet, FactionSystem).
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData _data;

        [Header("Refs")]
        [Tooltip("Where this enemy's bullets spawn from. Defaults to the enemy itself.")]
        [SerializeField] private Transform _muzzle;
        [Tooltip("Auto-found by tag 'Player' on Start if left empty.")]
        [SerializeField] private Transform _target;

        [Header("Vision [TUNABLE]")]
        [SerializeField] private float _eyeHeight = 1.6f;
        [Tooltip("Solid things that block line of sight.")]
        [SerializeField] private LayerMask _losBlockers = ~0;

        public EnemyState State { get; private set; } = EnemyState.Idle;
        public Faction Faction => _data != null ? _data.faction : Faction.Neutral;

        private NavMeshAgent _agent;
        private HealthSystem _health;
        private float _nextShotTime;

        private void Awake()
        {
            _agent  = GetComponent<NavMeshAgent>();
            _health = GetComponent<HealthSystem>();
            if (_data != null) _agent.speed = _data.moveSpeed;
        }

        private void Start()
        {
            if (_target == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _target = p.transform;
            }
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
            if (State == EnemyState.Dead || _data == null) return;

            switch (State)
            {
                case EnemyState.Idle:   TickIdle();   break;
                case EnemyState.Alert:  TickAlert();  break;
                case EnemyState.Engage: TickEngage(); break;
            }
        }

        private void TickIdle()
        {
            if (_target != null && CanSeeTarget()) ChangeState(EnemyState.Alert);
        }

        private void TickAlert()
        {
            if (_target == null) { ChangeState(EnemyState.Idle); return; }

            _agent.SetDestination(_target.position);

            float distance = Vector3.Distance(transform.position, _target.position);
            if (distance <= _data.rangedAttackRange && CanSeeTarget())
                ChangeState(EnemyState.Engage);
        }

        private void TickEngage()
        {
            if (_target == null) { ChangeState(EnemyState.Idle); return; }

            float distance = Vector3.Distance(transform.position, _target.position);

            // Face the target (only on the horizontal plane).
            Vector3 flat = _target.position - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, Quaternion.LookRotation(flat), Time.deltaTime * 5f);

            // Stay at attack range: close in if too far, hold position if close enough.
            if (distance > _data.rangedAttackRange * 0.9f) _agent.SetDestination(_target.position);
            else                                            _agent.ResetPath();

            // Fire if visible + in range; fall back to chasing if target slips out of detection.
            if (distance <= _data.rangedAttackRange && CanSeeTarget())
            {
                if (Time.time >= _nextShotTime) FireAtTarget();
            }
            else if (distance > _data.detectionRadius)
            {
                ChangeState(EnemyState.Alert);
            }
        }

        private bool CanSeeTarget()
        {
            if (_target == null) return false;

            Vector3 from = transform.position + Vector3.up * _eyeHeight;
            Vector3 toTarget = (_target.position + Vector3.up * 1f) - from;

            if (toTarget.magnitude > _data.detectionRadius) return false;

            if (Physics.Raycast(from, toTarget.normalized, out RaycastHit hit,
                toTarget.magnitude, _losBlockers, QueryTriggerInteraction.Ignore))
            {
                // Vision is blocked unless the ray hit the target itself.
                if (hit.transform != _target && !hit.transform.IsChildOf(_target)) return false;
            }
            return true;
        }

        private void FireAtTarget()
        {
            WeaponData weapon = _data.weapon;
            if (weapon == null || weapon.projectilePrefab == null) return;

            Transform spawn = _muzzle != null ? _muzzle : transform;
            Vector3 dir = ((_target.position + Vector3.up * 1f) - spawn.position).normalized;
            Quaternion rot = Quaternion.LookRotation(dir);

            GameObject go = Instantiate(weapon.projectilePrefab, spawn.position, rot);
            Bullet bullet = go.GetComponent<Bullet>();
            if (bullet != null) bullet.Launch(weapon.projectileSpeed, weapon.damage, null);

            if (weapon.fireSFX != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(weapon.fireSFX);

            float interval = weapon.fireRate > 0f ? 1f / weapon.fireRate : 0.5f;
            _nextShotTime = Time.time + interval;
        }

        private void OnDied() => ChangeState(EnemyState.Dead);

        private void ChangeState(EnemyState next)
        {
            if (State == next) return;
            State = next;

            if (next == EnemyState.Dead)
            {
                if (_agent != null) _agent.enabled = false;
                Destroy(gameObject, 3f); // give VFX a moment, then clean up
            }
        }
    }
}
