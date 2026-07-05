using System.Collections.Generic;
using UnityEngine;
using Aegis.Input;
using Aegis.Weapons;

namespace Aegis.Player
{
    /// <summary>
    /// Drives X's character-model Animator from OUR gameplay systems, matching X's parameters:
    ///   Floats:   MoveX, MoveY, Speed
    ///   Bools:    IsMoving, IsRunning, IsAiming, IsJumping, IsSliding, FireAuto, HasWeapon, IsDead
    ///   Triggers: FireSingle, Damage, Interact, SummonWeapon, CloseWeapon, Refill, RightPunch, LeftPunch
    /// Only parameters that actually exist on the controller are set, so the same script also
    /// works for characters that use a subset of these names.
    /// IMPORTANT: do NOT also run Jury's TinyToaster / AegisWeapon / XPunchyHands — this replaces
    /// them and drives the same Animator from our own movement/weapons/health.
    /// Tier 5.
    /// </summary>
    public class PlayerAnimatorBridge : MonoBehaviour
    {
        [Header("References (auto-found where possible)")]
        [SerializeField] private Animator _animator;
        [SerializeField] private InputReader _inputReader;
        [SerializeField] private MovementController _movement;
        [SerializeField] private HealthSystem _health;
        [SerializeField] private WeaponHandler _weapons;

        [Header("Locomotion params")]
        [SerializeField] private string _moveXParam = "MoveX";
        [SerializeField] private string _moveYParam = "MoveY";
        [SerializeField] private string _speedParam = "Speed";
        [SerializeField] private string _isMovingParam = "IsMoving";
        [SerializeField] private string _isRunningParam = "IsRunning";
        [SerializeField] private string _isJumpingParam = "IsJumping";
        [SerializeField] private string _isSlidingParam = "IsSliding";
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _jumpAnimTime = 0.6f;

        [Header("Combat params")]
        [Tooltip("Not driven yet — we have no aim (ADS) input. Left here for when aiming is added.")]
        [SerializeField] private string _isAimingParam = "IsAiming";
        [SerializeField] private string _fireSingleParam = "FireSingle";
        [SerializeField] private string _fireAutoParam = "FireAuto";
        [SerializeField] private string _reloadParam = "Refill";
        [SerializeField] private string _damageParam = "Damage";
        [SerializeField] private string _isDeadParam = "IsDead";
        [SerializeField] private string _interactParam = "Interact";

        [Header("Weapon equip params")]
        [SerializeField] private string _hasWeaponParam = "HasWeapon";
        [SerializeField] private string _summonWeaponParam = "SummonWeapon"; // trigger: no weapon -> weapon
        [SerializeField] private string _closeWeaponParam = "CloseWeapon";   // trigger: weapon -> none

        [Header("Melee (random punch each swing)")]
        [SerializeField] private string _leftPunchParam = "LeftPunch";
        [SerializeField] private string _rightPunchParam = "RightPunch";

        private readonly HashSet<string> _paramNames = new HashSet<string>();
        private float _jumpAnimUntil = -1f;
        private bool _prevHasWeapon;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_movement == null) _movement = GetComponent<MovementController>();
            if (_health == null) _health = GetComponent<HealthSystem>();
            if (_weapons == null) _weapons = GetComponent<WeaponHandler>();
            CacheParameterNames();
        }

        private void Start()
        {
            // Sync the initial weapon state without firing summon/close on the first frame.
            _prevHasWeapon = _weapons != null && _weapons.CurrentWeapon != null;
            SetBool(_hasWeaponParam, _prevHasWeapon);
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
            if (_inputReader != null)
            {
                _inputReader.ReloadEvent += OnReload;
                _inputReader.MeleeEvent += OnMelee;
                _inputReader.InteractStartedEvent += OnInteract;
            }
            if (_movement != null) _movement.Jumped += OnJumped;
            if (_health != null)
            {
                _health.DamageTaken += OnDamage;
                _health.Died += OnDied;
            }
            if (_weapons != null) _weapons.WeaponFired += OnWeaponFired;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.ReloadEvent -= OnReload;
                _inputReader.MeleeEvent -= OnMelee;
                _inputReader.InteractStartedEvent -= OnInteract;
            }
            if (_movement != null) _movement.Jumped -= OnJumped;
            if (_health != null)
            {
                _health.DamageTaken -= OnDamage;
                _health.Died -= OnDied;
            }
            if (_weapons != null) _weapons.WeaponFired -= OnWeaponFired;
        }

        private void Update()
        {
            if (_animator == null || _inputReader == null) return;

            Vector2 move = _inputReader.MoveInput;
            bool moving = move.sqrMagnitude > 0.01f;
            bool sliding = _movement != null && _movement.IsSliding;
            bool running = _inputReader.IsSprinting && moving && !sliding;

            SetFloatSmooth(_moveXParam, move.x);
            SetFloatSmooth(_moveYParam, move.y);
            SetFloatSmooth(_speedParam, move.magnitude);
            SetBool(_isMovingParam, moving);
            SetBool(_isRunningParam, running);
            SetBool(_isSlidingParam, sliding);
            SetBool(_isJumpingParam, Time.time < _jumpAnimUntil);

            // Weapon pose + summon/close detection + auto-fire loop.
            Weapon w = _weapons != null ? _weapons.CurrentWeapon : null;
            bool hasWeapon = w != null;
            if (hasWeapon != _prevHasWeapon)
            {
                SetTrigger(hasWeapon ? _summonWeaponParam : _closeWeaponParam);
                _prevHasWeapon = hasWeapon;
            }
            SetBool(_hasWeaponParam, hasWeapon);

            bool autoFiring = hasWeapon && _inputReader.IsFiring && w.Data.fireMode == FireMode.Auto;
            SetBool(_fireAutoParam, autoFiring);
        }

        private void OnJumped() => _jumpAnimUntil = Time.time + _jumpAnimTime;
        private void OnReload() => SetTrigger(_reloadParam);
        private void OnInteract() => SetTrigger(_interactParam);
        private void OnDamage(float amount) => SetTrigger(_damageParam);
        private void OnDied() => SetBool(_isDeadParam, true);

        private void OnWeaponFired(Weapon w)
        {
            // Auto weapons animate via the FireAuto bool (in Update); everything else = one-shot trigger.
            if (w != null && w.Data.fireMode != FireMode.Auto)
                SetTrigger(_fireSingleParam);
        }

        private void OnMelee()
        {
            // Random left/right punch each swing.
            SetTrigger(Random.value < 0.5f ? _leftPunchParam : _rightPunchParam);
        }

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
