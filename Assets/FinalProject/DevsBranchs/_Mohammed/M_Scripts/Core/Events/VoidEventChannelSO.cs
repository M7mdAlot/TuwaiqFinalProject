using System;
using UnityEngine;

namespace Aegis.Core.Events
{
    /// <summary>
    /// A parameterless event channel. Systems raise and listen via a shared
    /// ScriptableObject asset, so they stay decoupled (no hard references).
    /// Create assets via Assets ▸ Create ▸ AEGIS ▸ Events ▸ Void Event Channel.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Events/Void Event Channel", fileName = "VoidEventChannel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        /// <summary>Subscribers are invoked when <see cref="Raise"/> is called.</summary>
        public event Action OnRaised;

        public void Raise() => OnRaised?.Invoke();
    }
}
