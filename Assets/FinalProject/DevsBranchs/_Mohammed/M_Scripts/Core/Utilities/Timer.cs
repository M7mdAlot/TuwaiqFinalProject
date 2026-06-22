using System;
using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// A plain (non-MonoBehaviour) countdown helper. The owner calls <see cref="Tick"/>
    /// each frame with Time.deltaTime. Used later for the crisis countdown, etc.
    /// Tier 0 — no dependencies.
    /// </summary>
    public class Timer
    {
        public float Duration { get; private set; }
        public float Elapsed { get; private set; }
        public bool IsRunning { get; private set; }

        public bool IsFinished => Elapsed >= Duration;
        public float Remaining => Mathf.Max(0f, Duration - Elapsed);
        public float Progress01 => Duration <= 0f ? 1f : Mathf.Clamp01(Elapsed / Duration);

        /// <summary>Raised once when the timer reaches its duration.</summary>
        public event Action Finished;

        public void Start(float duration)
        {
            Duration = duration;
            Elapsed = 0f;
            IsRunning = true;
        }

        public void Stop() => IsRunning = false;
        public void Resume() => IsRunning = true;

        public void Reset()
        {
            Elapsed = 0f;
            IsRunning = false;
        }

        /// <summary>Advance the timer. Call every frame with Time.deltaTime.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;

            Elapsed += deltaTime;
            if (Elapsed >= Duration)
            {
                Elapsed = Duration;
                IsRunning = false;
                Finished?.Invoke();
            }
        }
    }
}
