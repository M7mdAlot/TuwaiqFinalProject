using System;
using UnityEngine;
using Aegis.Input;

namespace Aegis.Player
{
    /// <summary>
    /// First-person ground movement using Unity's CharacterController:
    /// walk, sprint, slide (a quick low dash with brief invincibility), jump, and gravity.
    /// Reads input from the <see cref="InputReader"/>. It never blocks input, so firing is
    /// always allowed while moving / sprinting / sliding (a hard constraint in the GDD).
    /// Movement is relative to where the body faces, which the camera turns.
    /// Tier 1 — depends only on <see cref="InputReader"/> (Tier 0) + Unity.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader _inputReader;

        [Header("Speeds (metres/second) [TUNABLE]")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _sprintSpeed = 8f;
        [SerializeField] private float _slideSpeed = 11f;

        [Header("Slide [TUNABLE]")]
        [SerializeField] private float _slideDuration = 0.6f;  // how long a slide lasts
        [SerializeField] private float _slideCooldown = 0.8f;  // wait before sliding again

        [Header("Jump & gravity [TUNABLE]")]
        [SerializeField] private float _jumpHeight = 1.5f;
        [Tooltip("More negative = heavier, snappier jumps/falls.")]
        [SerializeField] private float _gravity = -20f;

        private CharacterController _controller;

        private float _verticalVelocity;   // current up/down speed (gravity + jumping)
        private bool _jumpQueued;           // a jump was pressed; act on it this frame
        private bool _slideQueued;          // a slide was pressed; act on it this frame

        private bool _isSliding;
        private float _slideTimer;          // counts down while sliding
        private float _slideCooldownTimer;  // counts down after a slide ends
        private Vector3 _slideDirection;    // direction is locked in at slide start

        /// <summary>True during a slide. The HealthSystem will read this later for i-frames.</summary>
        public bool IsInvulnerable => _isSliding;
        public bool IsSliding => _isSliding;
        public bool IsGrounded => _controller.isGrounded;

        /// <summary>Raised the moment a jump actually launches — used to drive the jump animation.</summary>
        public event Action Jumped;

        /// <summary>Raised the moment a slide begins — used to drive the slide animation.</summary>
        public event Action SlideStarted;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (_inputReader != null)
            {
                _inputReader.JumpEvent += OnJump;
                _inputReader.SlideEvent += OnSlide;
            }
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.JumpEvent -= OnJump;
                _inputReader.SlideEvent -= OnSlide;
            }
        }

        // The InputReader fires these the moment the button is pressed; we just remember it.
        private void OnJump() => _jumpQueued = true;
        private void OnSlide() => _slideQueued = true;

        private void Update()
        {
            if (_inputReader == null) return;

            TickTimers();
            TryStartSlide();

            // Pick the sideways movement: a slide overrides normal walking.
            Vector3 horizontal = _isSliding ? _slideDirection * _slideSpeed : WalkVelocity();
            ApplyGravityAndJump();

            // Combine sideways + up/down and move once.
            Vector3 velocity = horizontal + Vector3.up * _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        private void TickTimers()
        {
            if (_slideCooldownTimer > 0f) _slideCooldownTimer -= Time.deltaTime;

            if (_isSliding)
            {
                _slideTimer -= Time.deltaTime;
                if (_slideTimer <= 0f)
                {
                    _isSliding = false;
                    _slideCooldownTimer = _slideCooldown;
                }
            }
        }

        private void TryStartSlide()
        {
            // Slide only when: pressed, not already sliding, on the ground,
            // currently sprinting, and the cooldown has finished.
            bool canSlide = _slideQueued && !_isSliding && _controller.isGrounded
                            && _inputReader.IsSprinting && _slideCooldownTimer <= 0f;
            _slideQueued = false;

            if (!canSlide) return;

            Vector3 dir = MoveDirection();
            _slideDirection = dir.sqrMagnitude > 0.01f ? dir : transform.forward;
            _isSliding = true;
            _slideTimer = _slideDuration;
            SlideStarted?.Invoke();
        }

        private Vector3 WalkVelocity()
        {
            float speed = _inputReader.IsSprinting ? _sprintSpeed : _walkSpeed;
            return MoveDirection() * speed;
        }

        // Turns the stick/WASD input into a world direction based on where the body faces.
        private Vector3 MoveDirection()
        {
            Vector2 input = _inputReader.MoveInput;
            Vector3 dir = transform.right * input.x + transform.forward * input.y;
            return Vector3.ClampMagnitude(dir, 1f); // no faster diagonal movement
        }

        private void ApplyGravityAndJump()
        {
            if (_controller.isGrounded)
            {
                // Keep a small downward pull so the controller stays "stuck" to the ground.
                if (_verticalVelocity < 0f) _verticalVelocity = -2f;

                // Launch speed needed to reach _jumpHeight: v = sqrt(h * -2 * g).
                if (_jumpQueued && !_isSliding)
                {
                    _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
                    Jumped?.Invoke();
                }
            }

            _jumpQueued = false;
            _verticalVelocity += _gravity * Time.deltaTime; // gravity pulls down every frame
        }
    }
}
