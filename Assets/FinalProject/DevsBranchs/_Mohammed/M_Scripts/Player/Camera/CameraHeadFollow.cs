using UnityEngine;

namespace Aegis.Player
{
    /// <summary>
    /// Makes the first-person camera follow the animated head bone's POSITION (in LateUpdate,
    /// after the Animator has posed the skeleton). This is what makes animations like the slide
    /// dip actually move the view. ROTATION is deliberately left alone — <see cref="FirstPersonCamera"/>
    /// keeps full control of mouse look, so the animation never jerks your aim around.
    ///
    /// Setup: keep the camera a child of the Player root (so it still inherits body yaw); do NOT
    /// parent it to the bone. Assign the model's head bone here. The eye offset is applied in the
    /// CAMERA's look direction, so it's independent of how the rig's bone axes are oriented.
    /// Tier 5 — pairs with FirstPersonCamera.
    /// </summary>
    [DefaultExecutionOrder(200)] // run after look/movement so we position last, over the animated pose
    public class CameraHeadFollow : MonoBehaviour
    {
        [Tooltip("The head bone of the animated model (inside the Armature, e.g. mixamorig:Head).")]
        [SerializeField] private Transform _headBone;
        [Tooltip("The camera (or camera holder) to move. Defaults to this object.")]
        [SerializeField] private Transform _camera;
        [Tooltip("Eye offset from the head bone, applied along the camera's look direction " +
                 "(+Y up, +Z forward). Nudge forward/up so the view sits at the eyes, not inside the skull.")]
        [SerializeField] private Vector3 _eyeOffset = new Vector3(0f, 0.08f, 0.12f);
        [Tooltip("0 = snap exactly to the head every frame (most accurate). " +
                 "Higher = smoother but laggier — use a little to tame walk head-bob.")]
        [SerializeField] private float _smoothing = 0f;

        private void Reset() => _camera = transform;

        private void LateUpdate()
        {
            if (_headBone == null || _camera == null) return;

            // Head bone position + a small offset in the CAMERA's orientation (look direction).
            Vector3 target = _headBone.position + _camera.rotation * _eyeOffset;

            _camera.position = _smoothing > 0f
                ? Vector3.Lerp(_camera.position, target, 1f - Mathf.Exp(-_smoothing * Time.deltaTime))
                : target;
        }
    }
}
