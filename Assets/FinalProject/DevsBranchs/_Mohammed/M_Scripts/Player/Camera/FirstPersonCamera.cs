using UnityEngine;
using Aegis.Input;

namespace Aegis.Player
{
    /// <summary>
    /// Custom first-person camera (NOT Cinemachine, per the GDD). Driven by the
    /// "Look" input from the <see cref="InputReader"/>:
    ///   • turns the whole player body left/right (yaw),
    ///   • tilts the head-anchored camera up/down (pitch), clamped so you can't flip over.
    /// Also locks the mouse cursor to the game window. Yaw is applied to the body so the
    /// body always faces where you look, which keeps movement aligned with the camera.
    /// Tier 1 — depends only on <see cref="InputReader"/> (Tier 0) + Unity.
    /// </summary>
    public class FirstPersonCamera : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader _inputReader;

        [Header("References")]
        [Tooltip("The head anchor that holds the Camera. This is what tilts up/down (pitch).")]
        [SerializeField] private Transform _cameraTransform;

        [Header("Look settings [TUNABLE]")]
        [Tooltip("Higher = faster look. Tuned for mouse; gamepad look may feel slow for now.")]
        [SerializeField] private float _lookSensitivity = 0.1f;
        [SerializeField] private float _minPitch = -85f; // how far you can look up
        [SerializeField] private float _maxPitch = 85f;  // how far you can look down (to see your body)
        [SerializeField] private bool _invertY = false;
        [SerializeField] private bool _lockCursor = true;

        private float _pitch; // current up/down angle of the camera, in degrees

        private void OnEnable()
        {
            if (_lockCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void OnDisable()
        {
            // Give the cursor back (e.g. when this is turned off for a menu).
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (_inputReader == null || _cameraTransform == null) return;

            Vector2 look = _inputReader.LookInput * _lookSensitivity;

            // Yaw: spin the whole body around the vertical (up) axis.
            transform.Rotate(Vector3.up * look.x);

            // Pitch: tilt only the camera up/down. Mouse-up should look up, so we subtract.
            float pitchDelta = _invertY ? look.y : -look.y;
            _pitch = Mathf.Clamp(_pitch + pitchDelta, _minPitch, _maxPitch);
            _cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}
