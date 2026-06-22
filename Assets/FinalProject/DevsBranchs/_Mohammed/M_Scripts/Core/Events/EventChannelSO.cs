using System;
using UnityEngine;

namespace Aegis.Core.Events
{
    /// <summary>
    /// Generic base for a typed event channel. Unity cannot create assets from a
    /// generic ScriptableObject directly, so subclass it with a concrete type and
    /// add <c>[CreateAssetMenu]</c> (see <see cref="FloatEventChannelSO"/> for the pattern).
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        public event Action<T> OnRaised;

        public void Raise(T value) => OnRaised?.Invoke(value);
    }
}
