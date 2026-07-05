using UnityEngine;
using Aegis.Player;

namespace Aegis.Weapons
{
    /// <summary>
    /// Procedural firing recoil for the held weapon MODEL — a purely visual kick, not an aim
    /// change (bullets still spawn from the camera muzzle). Put this on the weapon socket (the
    /// object under the camera that holds the gun models). Every shot, it reads the fired
    /// weapon's recoil values from its <see cref="WeaponData"/>, kicks the socket back + up,
    /// then smoothly settles back to rest. Recoil is per-weapon (tune it in each WeaponData).
    /// Tier 5 — depends on Tier 2 (WeaponHandler, WeaponData).
    /// </summary>
    public class WeaponRecoil : MonoBehaviour
    {
        [Tooltip("Auto-found in parents if empty.")]
        [SerializeField] private WeaponHandler _handler;
        [Tooltip("The transform that gets kicked (usually the weapon socket = this object).")]
        [SerializeField] private Transform _target;

        private Vector3 _restPos;
        private Quaternion _restRot;

        private Vector3 _posRecoil;   // current accumulated positional kick
        private Vector3 _rotRecoil;   // current accumulated rotational kick (euler degrees)
        private float _returnSpeed = 8f;
        private float _snappiness = 14f;

        private void Awake()
        {
            if (_handler == null) _handler = GetComponentInParent<WeaponHandler>();
            if (_target == null) _target = transform;

            _restPos = _target.localPosition;
            _restRot = _target.localRotation;
        }

        private void OnEnable()
        {
            if (_handler != null) _handler.WeaponFired += OnFired;
        }

        private void OnDisable()
        {
            if (_handler != null) _handler.WeaponFired -= OnFired;
        }

        private void OnFired(Weapon weapon)
        {
            if (weapon == null) return;

            WeaponData d = weapon.Data;
            _posRecoil += d.recoilPositionKick;
            _rotRecoil += d.recoilRotationKick;
            _returnSpeed = d.recoilReturnSpeed;
            _snappiness = d.recoilSnappiness;
        }

        private void Update()
        {
            if (_target == null) return;

            // 1) Decay the kick back toward zero.
            _posRecoil = Vector3.Lerp(_posRecoil, Vector3.zero, _returnSpeed * Time.deltaTime);
            _rotRecoil = Vector3.Lerp(_rotRecoil, Vector3.zero, _returnSpeed * Time.deltaTime);

            // 2) Snap the model toward (rest + current kick).
            Vector3 targetPos = _restPos + _posRecoil;
            Quaternion targetRot = _restRot * Quaternion.Euler(_rotRecoil);

            _target.localPosition = Vector3.Lerp(_target.localPosition, targetPos, _snappiness * Time.deltaTime);
            _target.localRotation = Quaternion.Slerp(_target.localRotation, targetRot, _snappiness * Time.deltaTime);
        }
    }
}
