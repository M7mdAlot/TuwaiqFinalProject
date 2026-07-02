using System;
using System.Collections.Generic;
using UnityEngine;
using Aegis.Player;

namespace Aegis.Weapons
{
    /// <summary>
    /// Visual choreography for a charge-style weapon. Drop this on the weapon's model
    /// (or on any object near the muzzle). It watches a <see cref="WeaponHandler"/> and
    /// drives the visuals while the matching weapon is charging:
    ///
    ///   • An energy BALL that grows as charge progresses (from min to max scale).
    ///   • A ROTOR — a Transform that spins continuously while charging.
    ///   • A list of STAGES — each a GameObject that turns ON when charge crosses its
    ///     threshold (e.g. lightning sparks at 40%, big lightning at 80%).
    ///
    /// When the weapon isn't equipped, isn't this weapon, or isn't charging, everything
    /// is hidden. Reset on fire is automatic (Weapon.OnFired sets IsCharging = false).
    /// Tier 2 — depends only on Tier 2 WeaponHandler.
    /// </summary>
    public class ChargeWeaponView : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Leave empty to auto-find a WeaponHandler in the parent hierarchy or the scene.")]
        [SerializeField] private WeaponHandler _handler;
        [Tooltip("Only show visuals when THIS weapon is the equipped one. Leave empty to show for any charging weapon.")]
        [SerializeField] private WeaponData _weaponFilter;

        [Header("Energy ball (optional)")]
        [SerializeField] private Transform _ball;
        [SerializeField] private Vector3 _ballMinScale = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private Vector3 _ballMaxScale = new Vector3(0.8f, 0.8f, 0.8f);
        [Tooltip("AnimationCurve from 0..1 to remap charge → ball size (default linear).")]
        [SerializeField] private AnimationCurve _ballScaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Rotating muzzle part (optional)")]
        [Tooltip("The specific piece that spins. It rotates around its OWN pivot (make sure that's centered on the mesh).")]
        [SerializeField] private Transform _rotor;
        [Tooltip("Axis to spin around. (0,0,1) = the rotor's forward, (0,1,0) = its up, (1,0,0) = its right.")]
        [SerializeField] private Vector3 _rotorAxis = Vector3.forward;
        [Tooltip("Self = around the rotor's own local axes (rotor-facing). World = around global axes.")]
        [SerializeField] private Space _rotorSpace = Space.Self;
        [Tooltip("Degrees per second at full charge.")]
        [SerializeField] private float _rotorMaxSpeed = 720f;
        [Tooltip("Multiplier for spin speed at 0% charge (so it starts slow and accelerates).")]
        [Range(0f, 1f)][SerializeField] private float _rotorStartSpeedFactor = 0.2f;
        [Tooltip("Editor-only: draws a small green line showing the rotor's spin axis at its pivot.")]
        [SerializeField] private bool _drawRotorAxisGizmo = true;

        [Header("Stages (each turns ON when charge crosses its threshold)")]
        [SerializeField] private List<ChargeStage> _stages = new List<ChargeStage>();

        [Serializable]
        public class ChargeStage
        {
            [Tooltip("Inspector label only.")]
            public string label = "Stage";
            [Range(0f, 1f)] public float threshold = 0.5f;
            [Tooltip("GameObject to turn ON when charge >= threshold (e.g. a lightning particle system).")]
            public GameObject target;
        }

        private void Awake()
        {
            if (_handler == null) _handler = GetComponentInParent<WeaponHandler>();
#if UNITY_2023_1_OR_NEWER
            if (_handler == null) _handler = FindFirstObjectByType<WeaponHandler>();
#else
            if (_handler == null) _handler = FindObjectOfType<WeaponHandler>();
#endif
            HideAll(); // start clean
        }

        private void Update()
        {
            if (_handler == null) { HideAll(); return; }

            Weapon w = _handler.CurrentWeapon;
            bool isOurWeapon = w != null && (_weaponFilter == null || w.Data == _weaponFilter);
            bool active = isOurWeapon && w.IsCharging;

            if (!active) { HideAll(); return; }

            float charge = Mathf.Clamp01(w.ChargeProgress01);

            // Ball: visible + scaled with charge.
            if (_ball != null)
            {
                if (!_ball.gameObject.activeSelf) _ball.gameObject.SetActive(true);
                float t = _ballScaleCurve.Evaluate(charge);
                _ball.localScale = Vector3.Lerp(_ballMinScale, _ballMaxScale, t);
            }

            // Rotor: accelerating spin. Space.Self = around the rotor's OWN axis (not the weapon's).
            if (_rotor != null)
            {
                float factor = Mathf.Lerp(_rotorStartSpeedFactor, 1f, charge);
                _rotor.Rotate(_rotorAxis.normalized * (_rotorMaxSpeed * factor) * Time.deltaTime, _rotorSpace);
            }

            // Stages: each one ON once its threshold is crossed.
            foreach (ChargeStage stage in _stages)
            {
                if (stage.target == null) continue;
                bool on = charge >= stage.threshold;
                if (stage.target.activeSelf != on) stage.target.SetActive(on);
            }
        }

        private void HideAll()
        {
            if (_ball != null && _ball.gameObject.activeSelf) _ball.gameObject.SetActive(false);
            foreach (ChargeStage stage in _stages)
                if (stage.target != null && stage.target.activeSelf) stage.target.SetActive(false);
            // Rotor: leave it where it stopped — it's a permanent part of the weapon.
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawRotorAxisGizmo || _rotor == null || _rotorAxis == Vector3.zero) return;

            Vector3 axis = _rotorSpace == Space.Self
                ? _rotor.TransformDirection(_rotorAxis.normalized)
                : _rotorAxis.normalized;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(_rotor.position - axis * 0.3f, _rotor.position + axis * 0.3f);
            Gizmos.DrawSphere(_rotor.position + axis * 0.3f, 0.02f);
        }
#endif
    }
}
