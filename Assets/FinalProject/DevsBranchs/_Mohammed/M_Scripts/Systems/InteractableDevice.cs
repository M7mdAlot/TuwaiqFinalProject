using UnityEngine;
using UnityEngine.Events;
using Aegis.Core;
using Aegis.Core.Events;

namespace Aegis.Systems
{
    /// <summary>
    /// A "hold F to disable" device — the reactor console (AEGIS.2) and the EMP bomb (X).
    /// The player faces it; the InteractionController fills its hold bar over
    /// <see cref="HoldDuration"/> seconds, then calls Interact and the device fires its
    /// success events. After that, it can't be used again (one-shot).
    /// Tier 2 — depends on Tier 0 (IInteractable, VoidEventChannelSO).
    /// </summary>
    public class InteractableDevice : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _prompt = "Hold F to shut down";
        [SerializeField] private float _holdDuration = 3f;
        [SerializeField] private bool _startsEnabled = true;

        [Header("Events fired on success")]
        [Tooltip("Optional event channel — anything listening (UI, crisis manager) reacts.")]
        [SerializeField] private VoidEventChannelSO _onDisabledChannel;
        public UnityEvent OnDisabled;

        public string Prompt => _prompt;
        public float HoldDuration => _holdDuration;
        public bool CanInteract { get; private set; }

        private void Awake() => CanInteract = _startsEnabled;

        public void SetEnabled(bool value) => CanInteract = value;

        public void Interact(GameObject interactor)
        {
            if (!CanInteract) return;

            CanInteract = false;          // one-shot: can't be re-triggered
            OnDisabled?.Invoke();
            _onDisabledChannel?.Raise();
        }
    }
}
