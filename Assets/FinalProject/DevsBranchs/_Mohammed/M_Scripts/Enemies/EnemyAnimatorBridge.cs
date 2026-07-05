using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Aegis.Player;

namespace Aegis.Enemies
{
    /// <summary>
    /// Drives a fighting human's Animator from OUR enemy systems (no punches — humans shoot).
    /// Locomotion comes from the NavMeshAgent's velocity; fire/damage/death come from
    /// <see cref="EnemyController"/> + <see cref="HealthSystem"/>. Matches the humans' params:
    ///   Floats:   MoveX, MoveY, Speed
    ///   Bools:    IsMoving, IsRunning, HasWeapon, IsDead
    ///   Triggers: FireSingle, Damage
    /// Only existing parameters are touched, so it's safe if the controller uses a subset.
    /// Put this on the enemy root (next to EnemyController). Tier 5.
    /// </summary>
    public class EnemyAnimatorBridge : MonoBehaviour
    {
        [Header("References (auto-found where possible)")]
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private HealthSystem _health;
        [SerializeField] private EnemyController _enemy;

        [Header("Locomotion params")]
        [SerializeField] private string _moveXParam = "MoveX";
        [SerializeField] private string _moveYParam = "MoveY";
        [SerializeField] private string _speedParam = "Speed";
        [SerializeField] private string _isMovingParam = "IsMoving";
        [SerializeField] private string _isRunningParam = "IsRunning";
        [SerializeField] private float _smoothTime = 0.1f;
        [Range(0f, 1f)][SerializeField] private float _runThreshold = 0.6f;

        [Header("Combat params")]
        [SerializeField] private string _fireSingleParam = "FireSingle";
        [SerializeField] private string _damageParam = "Damage";
        [SerializeField] private string _isDeadParam = "IsDead";
        [SerializeField] private string _hasWeaponParam = "HasWeapon";

        private readonly HashSet<string> _paramNames = new HashSet<string>();

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_health == null) _health = GetComponent<HealthSystem>();
            if (_enemy == null) _enemy = GetComponent<EnemyController>();
            CacheParameterNames();
        }

        private void Start()
        {
            SetBool(_hasWeaponParam, true); // fighting humans are armed
        }

        private void CacheParameterNames()
        {
            _paramNames.Clear();
            if (_animator == null || _animator.runtimeAnimatorController == null) return;
            foreach (AnimatorControllerParameter p in _animator.parameters)
                _paramNames.Add(p.name);
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.DamageTaken += OnDamage;
                _health.Died += OnDied;
            }
            if (_enemy != null) _enemy.Fired += OnFired;
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.DamageTaken -= OnDamage;
                _health.Died -= OnDied;
            }
            if (_enemy != null) _enemy.Fired -= OnFired;
        }

        private void Update()
        {
            if (_animator == null || _agent == null) return;

            Vector3 worldVel = _agent.velocity;
            float maxSpeed = _agent.speed > 0.01f ? _agent.speed : 1f;

            // Convert velocity to the enemy's local space so MoveX/MoveY fit a blend tree.
            Vector3 localVel = transform.InverseTransformDirection(worldVel);
            SetFloatSmooth(_moveXParam, Mathf.Clamp(localVel.x / maxSpeed, -1f, 1f));
            SetFloatSmooth(_moveYParam, Mathf.Clamp(localVel.z / maxSpeed, -1f, 1f));

            float speed01 = worldVel.magnitude / maxSpeed;
            SetFloatSmooth(_speedParam, speed01);
            SetBool(_isMovingParam, worldVel.magnitude > 0.1f);
            SetBool(_isRunningParam, speed01 > _runThreshold);
        }

        private void OnFired() => SetTrigger(_fireSingleParam);
        private void OnDamage(float amount) => SetTrigger(_damageParam);
        private void OnDied() => SetBool(_isDeadParam, true);

        // --- Safe setters: only touch parameters the controller actually has ---
        private void SetFloatSmooth(string param, float value)
        {
            if (_paramNames.Contains(param)) _animator.SetFloat(param, value, _smoothTime, Time.deltaTime);
        }

        private void SetBool(string param, bool value)
        {
            if (_paramNames.Contains(param)) _animator.SetBool(param, value);
        }

        private void SetTrigger(string param)
        {
            if (!string.IsNullOrEmpty(param) && _paramNames.Contains(param)) _animator.SetTrigger(param);
        }
    }
}
