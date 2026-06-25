using UnityEngine;

namespace Aegis.Core
{
    /// <summary>
    /// A contract for anything the player can interact with via the Interact button
    /// (hold-F): weapon pickups, the reactor console, the EMP bomb, doors, etc.
    /// Tier 0 — no dependencies.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Text to show the player, e.g. "Hold F to shut down".</summary>
        string Prompt { get; }

        /// <summary>Seconds the player must hold the button. 0 = instant.</summary>
        float HoldDuration { get; }

        /// <summary>Whether this can be interacted with right now.</summary>
        bool CanInteract { get; }

        /// <summary>Run the interaction. <paramref name="interactor"/> is who did it (usually the player).</summary>
        void Interact(GameObject interactor);
    }
}
