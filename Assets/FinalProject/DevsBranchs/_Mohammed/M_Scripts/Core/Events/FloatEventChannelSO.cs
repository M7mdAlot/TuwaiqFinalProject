using UnityEngine;

namespace Aegis.Core.Events
{
    /// <summary>
    /// A float event channel (e.g. health %, volume sliders). Concrete example of how
    /// to extend <see cref="EventChannelSO{T}"/> with its own asset-creation menu.
    /// Tier 0 — no dependencies on other game scripts.
    /// </summary>
    [CreateAssetMenu(menuName = "AEGIS/Events/Float Event Channel", fileName = "FloatEventChannel")]
    public class FloatEventChannelSO : EventChannelSO<float>
    {
    }
}
