using System;
using UnityEngine;
using UnityEngine.Events;
using Aegis.Core;
using Aegis.Core.Events;
using Aegis.Enemies;

namespace Aegis.Systems
{
    /// <summary>
    /// The mid-fight crisis (GDD §11) — same script whether it's AEGIS.2's reactor overload
    /// or X's EMP bomb. Only the objective text, timer, and the device differ per campaign.
    ///
    /// Flow:
    ///   1. Sit idle while the fight starts.
    ///   2. Trigger — either when the assigned <see cref="EnemySpawner"/>'s kill fraction
    ///      crosses <see cref="_triggerFractionCleared"/>, or when <see cref="TriggerCrisis"/>
    ///      is called manually (e.g. from a <see cref="TriggerZone"/> or a scripted step).
    ///   3. Enable the <see cref="InteractableDevice"/> so the player can hold-F to disable it,
    ///      start the <see cref="Timer"/>, and show an objective banner + countdown via UIManager.
    ///   4. Success = device disabled before time runs out. Failure = timer hits zero.
    ///
    /// Tier 3 — depends on Tier 2 (InteractableDevice, EnemySpawner) + Tier 1 (UIManager)
    /// + Tier 0 (Timer, event channels).
    /// </summary>
    public class CrisisManager : MonoBehaviour
    {
        /// <summary>How the crisis begins.</summary>
        public enum StartMode
        {
            Trigger,            // started by an external event (a TriggerZone's OnEntered → TriggerCrisis)
            WhenEnemiesCleared  // auto-start once a fraction of a spawner's enemies have died
        }

        [Header("How the crisis starts")]
        [Tooltip("Trigger = an external event starts it (e.g. a TriggerZone calling TriggerCrisis). WhenEnemiesCleared = auto-start after clearing enough enemies.")]
        [SerializeField] private StartMode _startMode = StartMode.Trigger;

        [Header("'When enemies cleared' options (only used in that mode)")]
        [Tooltip("The spawner to watch. Ignored unless Start Mode is WhenEnemiesCleared.")]
        [SerializeField] private EnemySpawner _watchSpawner;
        [Range(0f, 1f)][SerializeField] private float _triggerFractionCleared = 0.6f;

        [Header("Countdown [TUNABLE]")]
        [Tooltip("Seconds on the clock once the crisis starts.")]
        [SerializeField] private float _duration = 60f;

        [Header("Device to disable")]
        [Tooltip("The reactor console or EMP bomb — starts hidden/inactive, enabled when the crisis triggers.")]
        [SerializeField] private InteractableDevice _device;

        [Header("UI")]
        [SerializeField] private string _objectiveText = "Shut down the reactor!";
        [SerializeField] private bool _hideBannerOnResolve = true;

        [Header("Broadcast (optional)")]
        [SerializeField] private VoidEventChannelSO _onTriggeredChannel;
        [SerializeField] private VoidEventChannelSO _onSucceededChannel;
        [SerializeField] private VoidEventChannelSO _onFailedChannel;

        [Header("Inspector-wired hooks")]
        public UnityEvent OnCrisisTriggered;
        public UnityEvent OnCrisisSucceeded;
        public UnityEvent OnCrisisFailed;

        public bool HasTriggered => _triggered;
        public bool IsResolved => _resolved;
        public bool ResolvedWithSuccess { get; private set; }
        public float TimeRemaining => _timer.Remaining;
        public float Duration => _timer.Duration;
        public float Progress01 => _timer.Progress01;
        public string ObjectiveText => _objectiveText;

        public event Action Triggered;
        public event Action Succeeded;
        public event Action Failed;

        private readonly Timer _timer = new Timer();
        private bool _triggered;
        private bool _resolved;

        private void Awake()
        {
            // Device should start disabled — the crisis enables it when it fires.
            if (_device != null) _device.SetEnabled(false);
        }

        private void Update()
        {
            // Auto-trigger only in 'when enemies cleared' mode. In Trigger mode (the default),
            // the crisis waits for an external TriggerCrisis() call — e.g. from a TriggerZone.
            if (!_triggered && _startMode == StartMode.WhenEnemiesCleared
                && _watchSpawner != null && _watchSpawner.FractionCleared >= _triggerFractionCleared)
            {
                TriggerCrisis();
            }

            if (!_triggered || _resolved) return;

            _timer.Tick(Time.deltaTime);

            // Update UI (null-safe: works even before UIManager is set up).
            UIManager ui = UIManager.Instance;
            if (ui != null) ui.SetCrisisTimer(_timer.Remaining, _timer.Duration);

            // Success: the device was disabled by the player (its CanInteract flipped to false).
            if (_device != null && !_device.CanInteract) { Resolve(true); return; }

            // Failure: countdown hit zero.
            if (_timer.IsFinished) Resolve(false);
        }

        /// <summary>Start the crisis right now. Safe to call from a UnityEvent.</summary>
        public void TriggerCrisis()
        {
            if (_triggered) return;
            _triggered = true;
            _resolved = false;

            if (_device != null) _device.SetEnabled(true);
            _timer.Start(_duration);

            UIManager ui = UIManager.Instance;
            if (ui != null)
            {
                ui.ShowObjective(_objectiveText);
                ui.SetCrisisTimer(_duration, _duration);
            }

            OnCrisisTriggered?.Invoke();
            _onTriggeredChannel?.Raise();
            Triggered?.Invoke();
        }

        private void Resolve(bool success)
        {
            _resolved = true;
            ResolvedWithSuccess = success;
            _timer.Stop();

            UIManager ui = UIManager.Instance;
            if (ui != null)
            {
                ui.HideCrisisTimer();
                if (_hideBannerOnResolve) ui.HideObjective();
            }

            if (success) { OnCrisisSucceeded?.Invoke(); _onSucceededChannel?.Raise(); Succeeded?.Invoke(); }
            else         { OnCrisisFailed?.Invoke();    _onFailedChannel?.Raise();    Failed?.Invoke(); }
        }
    }
}
