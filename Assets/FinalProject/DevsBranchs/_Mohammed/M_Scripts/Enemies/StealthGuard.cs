using System;
using UnityEngine;
using Aegis.Systems;

namespace Aegis.Enemies
{
    /// <summary>
    /// A patrol/guard's detection meter. Lives on the same GameObject as a
    /// <see cref="StealthSensor"/> (the binary "can I see you?" check) and turns that into a
    /// rising/falling 0..1 detection value — so a brief glance doesn't instantly bust X,
    /// but standing in view long enough does.
    ///
    /// Detection ramps **faster the closer X is** to the guard, and **drains** to zero when
    /// X breaks line of sight (after a short grace period). When it reaches 1, the guard
    /// "spots" X and raises <see cref="FullySpotted"/>. The StealthSystem watches all guards
    /// and decides what happens next (alarm, game over, etc.).
    /// Tier 2 — depends on Tier 1 (StealthSensor) only.
    /// </summary>
    [RequireComponent(typeof(StealthSensor))]
    public class StealthGuard : MonoBehaviour
    {
        [Header("Detection [TUNABLE]")]
        [Tooltip("Seconds of continuous sight (at the EDGE of view range) to fully detect the target.")]
        [SerializeField] private float _timeToSpotAtMaxRange = 3f;
        [Tooltip("How fast detection drains when the target is hidden, in units/second.")]
        [SerializeField] private float _decayPerSecond = 0.4f;
        [Tooltip("Seconds after losing sight before decay starts (linger). Adds suspense.")]
        [SerializeField] private float _decayDelay = 0.5f;
        [Tooltip("Minimum rate multiplier even at the edge of view range (1 = no distance falloff).")]
        [Range(0.1f, 1f)][SerializeField] private float _minRateAtMaxRange = 0.4f;

        public StealthSensor Sensor { get; private set; }

        /// <summary>0 = unseen, 1 = fully spotted. Drives the meter UI.</summary>
        public float Detection01 { get; private set; }
        public bool IsFullySpotted => Detection01 >= 1f;

        public event Action FullySpotted;     // fired once when the meter first hits 1
        public event Action LostCompletely;   // fired once when the meter drops back to 0

        private float _lostTimer;
        private bool _spottedRaised;
        private bool _lostRaised = true;

        private void Awake()
        {
            Sensor = GetComponent<StealthSensor>();
        }

        private void Update()
        {
            if (Sensor == null || Sensor.Target == null) return;

            if (Sensor.CanSeeTarget)
            {
                _lostTimer = 0f;
                Detection01 = Mathf.Clamp01(Detection01 + GainPerSecond() * Time.deltaTime);
            }
            else
            {
                _lostTimer += Time.deltaTime;
                if (_lostTimer >= _decayDelay)
                    Detection01 = Mathf.Clamp01(Detection01 - _decayPerSecond * Time.deltaTime);
            }

            // Edge events: fire each one once per "spot / lose" cycle.
            if (Detection01 >= 1f && !_spottedRaised)
            {
                _spottedRaised = true;
                _lostRaised = false;
                FullySpotted?.Invoke();
            }
            else if (Detection01 <= 0f && !_lostRaised)
            {
                _lostRaised = true;
                _spottedRaised = false;
                LostCompletely?.Invoke();
            }
        }

        private float GainPerSecond()
        {
            // Base rate = 1 / time-to-spot. Scaled up when the target is close, down when far.
            float baseRate = _timeToSpotAtMaxRange > 0f ? 1f / _timeToSpotAtMaxRange : 1f;

            float distance = Vector3.Distance(transform.position, Sensor.Target.position);
            float t = Sensor.ViewRange > 0f ? 1f - Mathf.Clamp01(distance / Sensor.ViewRange) : 0f;
            float multiplier = Mathf.Lerp(_minRateAtMaxRange, 1f, t);

            return baseRate * multiplier;
        }

        public void ResetDetection()
        {
            Detection01 = 0f;
            _lostTimer = 0f;
            _spottedRaised = false;
            _lostRaised = true;
        }
    }
}
