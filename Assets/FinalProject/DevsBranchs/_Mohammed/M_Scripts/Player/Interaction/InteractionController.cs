using System;
using UnityEngine;
using Aegis.Core;
using Aegis.Input;

namespace Aegis.Player
{
    /// <summary>
    /// Looks for <see cref="IInteractable"/> things in front of the camera and lets the
    /// player use them with the Interact button. Instant interactables fire on a tap;
    /// hold-style ones (e.g. "Hold F to shut down") fill up over their HoldDuration.
    /// Tier 1 — depends only on Tier 0 (IInteractable, InputReader).
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        [SerializeField] private InputReader _inputReader;
        [Tooltip("Where the ray is cast from — usually the first-person camera.")]
        [SerializeField] private Transform _rayOrigin;
        [SerializeField] private float _range = 3f;
        [SerializeField] private LayerMask _mask = ~0;

        /// <summary>The interactable currently being looked at (null = none).</summary>
        public IInteractable CurrentTarget { get; private set; }

        /// <summary>Hold progress 0..1 for hold-style interactions (for a UI ring/bar).</summary>
        public float HoldProgress01 { get; private set; }

        /// <summary>Fires when the looked-at interactable changes (null = looking at nothing).</summary>
        public event Action<IInteractable> TargetChanged;

        private float _holdTime;

        private void OnEnable()
        {
            if (_inputReader != null) _inputReader.InteractEvent += OnInteractPressed;
        }

        private void OnDisable()
        {
            if (_inputReader != null) _inputReader.InteractEvent -= OnInteractPressed;
        }

        private void Update()
        {
            UpdateTarget();
            UpdateHold();
        }

        private void UpdateTarget()
        {
            IInteractable found = null;
            Transform origin = _rayOrigin != null ? _rayOrigin : transform;

            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, _range, _mask, QueryTriggerInteraction.Ignore))
            {
                IInteractable candidate = hit.collider.GetComponentInParent<IInteractable>();
                if (candidate != null && candidate.CanInteract) found = candidate;
            }

            if (!ReferenceEquals(found, CurrentTarget))
            {
                CurrentTarget = found;
                _holdTime = 0f;
                HoldProgress01 = 0f;
                TargetChanged?.Invoke(found);
            }
        }

        private void UpdateHold()
        {
            // Only hold-style targets are handled here; taps are handled on press.
            if (CurrentTarget == null || _inputReader == null || CurrentTarget.HoldDuration <= 0f)
            {
                _holdTime = 0f;
                HoldProgress01 = 0f;
                return;
            }

            if (_inputReader.IsInteractHeld && CurrentTarget.CanInteract)
            {
                _holdTime += Time.deltaTime;
                HoldProgress01 = Mathf.Clamp01(_holdTime / CurrentTarget.HoldDuration);

                if (_holdTime >= CurrentTarget.HoldDuration)
                {
                    CurrentTarget.Interact(gameObject);
                    _holdTime = 0f;
                    HoldProgress01 = 0f;
                }
            }
            else
            {
                _holdTime = 0f;
                HoldProgress01 = 0f;
            }
        }

        private void OnInteractPressed()
        {
            // Instant interactions (HoldDuration == 0) trigger on a single press.
            if (CurrentTarget != null && CurrentTarget.CanInteract && CurrentTarget.HoldDuration <= 0f)
                CurrentTarget.Interact(gameObject);
        }
    }
}
