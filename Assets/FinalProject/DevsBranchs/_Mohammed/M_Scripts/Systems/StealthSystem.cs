using System;
using System.Collections.Generic;
using UnityEngine;
using Aegis.Core;
using Aegis.Core.Events;
using Aegis.Enemies;

namespace Aegis.Systems
{
    /// <summary>
    /// The "is anyone watching X yet?" tracker. Every <see cref="StealthGuard"/> in the scene
    /// registers with this system. Each frame, it reports the **highest** detection level
    /// among all guards and raises events when the alarm crosses the spotted threshold.
    /// Used in X's intro: subscribe to <see cref="Spotted"/> to fail the stealth, ring an
    /// alarm, or activate combat.
    /// Tier 2 — depends on Tier 0 (event channels) + Tier 2 (StealthGuard).
    /// </summary>
    public class StealthSystem : Singleton<StealthSystem>
    {
        public enum AlertLevel { Hidden, Suspicious, Spotted }

        [Header("Thresholds")]
        [Tooltip("Detection level (0..1) at which the player is considered 'noticed' but not yet busted.")]
        [Range(0f, 1f)][SerializeField] private float _suspiciousThreshold = 0.25f;
        [Tooltip("Detection level at which the player is fully spotted. Usually 1.")]
        [Range(0f, 1f)][SerializeField] private float _spottedThreshold = 1f;

        [Header("Auto-discovery")]
        [Tooltip("On Start, find every StealthGuard in the scene and watch them automatically.")]
        [SerializeField] private bool _autoFindGuardsInScene = true;

        [Header("Broadcast (optional)")]
        [Tooltip("If set, the highest detection (0..1) is sent on this channel every frame — handy for a HUD bar.")]
        [SerializeField] private FloatEventChannelSO _detectionChannel;
        [Tooltip("Raised once when alert goes from Suspicious/Hidden -> Spotted.")]
        [SerializeField] private VoidEventChannelSO _onSpottedChannel;

        /// <summary>Highest detection (0..1) across every registered guard right now.</summary>
        public float HighestDetection { get; private set; }
        public AlertLevel CurrentLevel { get; private set; } = AlertLevel.Hidden;

        public event Action<float> DetectionChanged;
        public event Action<AlertLevel> LevelChanged;
        public event Action Spotted;

        private readonly List<StealthGuard> _guards = new List<StealthGuard>();

        protected override void OnAwake()
        {
            // Stealth is a per-scene concern (X's intro) — don't carry across scenes by default.
            // (Singleton<T> defaults to persistent; uncheck "Persist Across Scenes" on this object.)
        }

        private void Start()
        {
            if (_autoFindGuardsInScene)
            {
                foreach (StealthGuard guard in FindObjectsByType<StealthGuard>(FindObjectsSortMode.None))
                    Register(guard);
            }
        }

        public void Register(StealthGuard guard)
        {
            if (guard == null || _guards.Contains(guard)) return;
            _guards.Add(guard);
        }

        public void Unregister(StealthGuard guard)
        {
            if (guard == null) return;
            _guards.Remove(guard);
        }

        private void Update()
        {
            float max = 0f;
            foreach (StealthGuard g in _guards)
            {
                if (g == null) continue;
                if (g.Detection01 > max) max = g.Detection01;
            }

            if (!Mathf.Approximately(max, HighestDetection))
            {
                HighestDetection = max;
                DetectionChanged?.Invoke(max);
                _detectionChannel?.Raise(max);
            }

            AlertLevel level =
                max >= _spottedThreshold    ? AlertLevel.Spotted :
                max >= _suspiciousThreshold ? AlertLevel.Suspicious :
                                              AlertLevel.Hidden;

            if (level != CurrentLevel)
            {
                CurrentLevel = level;
                LevelChanged?.Invoke(level);

                if (level == AlertLevel.Spotted)
                {
                    Spotted?.Invoke();
                    _onSpottedChannel?.Raise();
                }
            }
        }
    }
}
