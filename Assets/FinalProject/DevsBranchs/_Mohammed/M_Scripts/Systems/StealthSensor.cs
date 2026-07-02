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

        [Header("Debug visuals (Editor only)")]
        [Tooltip("ON = cone is always drawn in the Scene view. OFF = only when this guard is selected.")]
        [SerializeField] private bool _drawGizmosAlways = false;
        [SerializeField] private Color _coneColor = new Color(1f, 0.92f, 0.016f, 0.18f); // semi-transparent yellow

        public bool CanSeeTarget { get; private set; }
        public Transform Target => _target;
        public float ViewRange => _viewRange;
        public float ViewAngle => _viewAngle;
        public Transform EyesOrFallback => _eyes != null ? _eyes : transform;

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

#if UNITY_EDITOR
        private void OnDrawGizmos()        { if (_drawGizmosAlways) DrawViewCone(); }
        private void OnDrawGizmosSelected(){ if (!_drawGizmosAlways) DrawViewCone(); }

        private void DrawViewCone()
        {
            Transform origin = EyesOrFallback;

            // Filled pie slice (Handles renders nicer arcs than Gizmos).
            UnityEditor.Handles.color = _coneColor;
            UnityEditor.Handles.DrawSolidArc(
                origin.position, Vector3.up,
                Quaternion.AngleAxis(-_viewAngle * 0.5f, Vector3.up) * origin.forward,
                _viewAngle, _viewRange);

            // Wireframe outline so the cone reads clearly.
            UnityEditor.Handles.color = new Color(_coneColor.r, _coneColor.g, _coneColor.b, 0.9f);
            UnityEditor.Handles.DrawWireArc(
                origin.position, Vector3.up,
                Quaternion.AngleAxis(-_viewAngle * 0.5f, Vector3.up) * origin.forward,
                _viewAngle, _viewRange);

            // The two edge rays.
            Vector3 left  = Quaternion.AngleAxis(-_viewAngle * 0.5f, Vector3.up) * origin.forward * _viewRange;
            Vector3 right = Quaternion.AngleAxis( _viewAngle * 0.5f, Vector3.up) * origin.forward * _viewRange;
            Gizmos.color = new Color(_coneColor.r, _coneColor.g, _coneColor.b, 0.9f);
            Gizmos.DrawRay(origin.position, left);
            Gizmos.DrawRay(origin.position, right);
        }
#endif
    }
}
