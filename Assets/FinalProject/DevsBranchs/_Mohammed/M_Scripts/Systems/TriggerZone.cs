using UnityEngine;
using UnityEngine.Events;
using Aegis.Core.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// A physical volume that fires an event when something (usually the player) walks into it.
    /// The classic "invisible tripwire" — used for story beats: the hallway ambush, the corpse
    /// discovery, the sirens starting, the failsafe room.
    /// The attached Collider is auto-set to Is Trigger so it never blocks movement.
    /// Tier 3 — depends on Tier 0 (event channels) only.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerZone : MonoBehaviour
    {
        [Header("Filter")]
        [Tooltip("Only fire for things with this tag. Leave empty to fire for anything.")]
        [SerializeField] private string _requiredTag = "Player";
        [Tooltip("Fire only once, then never again in this scene.")]
        [SerializeField] private bool _oneShot = true;

        [Header("Broadcast")]
        [SerializeField] private VoidEventChannelSO _channel;
        public UnityEvent OnEntered;

        [Header("Debug visuals (Editor only)")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _idleColor = new Color(0f, 0.6f, 1f, 0.2f);
        [SerializeField] private Color _firedColor = new Color(1f, 0.2f, 0.2f, 0.2f);

        private bool _fired;

        private void Reset()
        {
            // First time the script is added: force the Collider to be a trigger.
            Collider col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_oneShot && _fired) return;
            if (!string.IsNullOrEmpty(_requiredTag) && !other.CompareTag(_requiredTag)) return;

            _fired = true;
            _channel?.Raise();
            OnEntered?.Invoke();
        }

        /// <summary>Allow the trigger to fire again after it was one-shot.</summary>
        public void ResetTrigger() => _fired = false;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = _fired ? _firedColor : _idleColor;

            if (col is BoxCollider box)
            {
                Matrix4x4 old = Gizmos.matrix;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = old;
            }
            else if (col is SphereCollider sph)
            {
                float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
                Gizmos.DrawSphere(transform.TransformPoint(sph.center), sph.radius * scale);
            }
            else if (col is CapsuleCollider cap)
            {
                Gizmos.DrawSphere(transform.TransformPoint(cap.center), cap.radius);
            }
        }
#endif
    }
}
