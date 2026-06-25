using System;
using UnityEngine;

namespace Aegis.Systems
{
    /// <summary>
    /// A simple "can this guard see the target?" check for X's optional stealth intro
    /// (GDD §6, §18). Sees the target only if it is within range, inside the view cone,
    /// and not hidden behind something. Raises events when the target is spotted or lost.
    /// Tier 1 — depends only on Unity.
    /// </summary>
    public class StealthSensor : MonoBehaviour
    {
        [Tooltip("Where vision starts (eyes/head). Defaults to this object.")]
        [SerializeField] private Transform _eyes;
        [Tooltip("Who to look for — usually the player.")]
        [SerializeField] private Transform _target;

        [Header("Vision [TUNABLE]")]
        [SerializeField] private float _viewRange = 12f;
        [Range(1f, 360f)][SerializeField] private float _viewAngle = 90f;
        [Tooltip("Solid things that block line of sight.")]
        [SerializeField] private LayerMask _obstacleMask = ~0;

        public bool CanSeeTarget { get; private set; }

        public event Action TargetSpotted;
        public event Action TargetLost;

        public void SetTarget(Transform target) => _target = target;

        private void Update()
        {
            bool sees = Check();

            if (sees && !CanSeeTarget)
            {
                CanSeeTarget = true;
                TargetSpotted?.Invoke();
            }
            else if (!sees && CanSeeTarget)
            {
                CanSeeTarget = false;
                TargetLost?.Invoke();
            }
        }

        private bool Check()
        {
            if (_target == null) return false;

            Transform origin = _eyes != null ? _eyes : transform;
            Vector3 toTarget = _target.position - origin.position;

            if (toTarget.magnitude > _viewRange) return false;                        // too far
            if (Vector3.Angle(origin.forward, toTarget) > _viewAngle * 0.5f) return false; // outside the cone

            // Line of sight: if something solid is in the way, vision is blocked.
            if (Physics.Raycast(origin.position, toTarget.normalized, out RaycastHit hit, _viewRange, _obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != _target && !hit.transform.IsChildOf(_target)) return false;
            }

            return true;
        }
    }
}
