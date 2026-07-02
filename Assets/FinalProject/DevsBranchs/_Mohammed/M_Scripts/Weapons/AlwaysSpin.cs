using UnityEngine;

namespace Aegis.Weapons
{
    /// <summary>
    /// Debug helper: continuously rotates this object around the chosen axis.
    /// Use to verify a mesh's pivot and find the right axis in isolation — no
    /// charging, no weapon setup. Drop it on the ring / claw part and press Play.
    /// Once it spins the way you want, copy the same axis + space into
    /// <see cref="ChargeWeaponView"/> and remove this component.
    /// </summary>
    public class AlwaysSpin : MonoBehaviour
    {
        [Tooltip("Axis to spin around. Try (0,0,1), (0,1,0), (1,0,0) in that order.")]
        [SerializeField] private Vector3 _axis = new Vector3(0f, 0f, 1f);
        [Tooltip("Self = around this object's own local axes. World = around global axes.")]
        [SerializeField] private Space _space = Space.Self;
        [SerializeField] private float _speedDegPerSec = 360f;
        [Tooltip("Editor-only green line showing the resolved spin axis at this object's pivot.")]
        [SerializeField] private bool _drawGizmo = true;

        private void Update()
        {
            if (_axis == Vector3.zero || _speedDegPerSec == 0f) return;
            transform.Rotate(_axis.normalized * (_speedDegPerSec * Time.deltaTime), _space);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmo || _axis == Vector3.zero) return;
            Vector3 dir = _space == Space.Self
                ? transform.TransformDirection(_axis.normalized)
                : _axis.normalized;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position - dir * 0.3f, transform.position + dir * 0.3f);
            Gizmos.DrawSphere(transform.position + dir * 0.3f, 0.02f);
        }
#endif
    }
}
