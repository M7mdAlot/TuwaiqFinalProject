using UnityEngine;
using UnityEngine.Events;

// A generic "player walked into this zone" trigger. Put it on an empty GameObject with a
// trigger Collider, placed anywhere in the level. In the Inspector, wire OnPlayerEnter to
// whatever should happen — e.g. AegisDialogueLibrary.PlayAegisMeetsScientist (dialogue is
// stored ONCE in the library, never retyped here), or a story/objective/crisis method.
// Detects the player via PlayerCharacterIdentity (on both Aegis and X).
[RequireComponent(typeof(Collider))]
public class PlayerEnterTrigger : MonoBehaviour
{
    [Header("What happens when the player walks in")]
    public UnityEvent onPlayerEnter;

    [Header("Options")]
    public bool onlyOnce = true;

    [Header("Gate (optional — for order-dependent triggers)")]
    [Tooltip("If set, this trigger only fires once the StorySequencer has played at least the beat below. Use it so event 7 can't happen before event 6, or the bomb room only opens after earlier beats.")]
    public StorySequencer requireSequencer;
    [Tooltip("Minimum beat number that must have played. -1 = no gate.")]
    public int requireBeatAtLeast = -1;

    private bool used;

    void Reset()
    {
        // Auto-make the collider a trigger when the component is first added in the editor.
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && used) return;

        bool isPlayer = other.GetComponentInParent<PlayerCharacterIdentity>() != null
                        || other.CompareTag("Player");

        // Ignore bullets and everything else SILENTLY — no console spam.
        if (!isPlayer) return;

        // Gate: don't fire (and don't consume the trigger) until the prerequisite beat played.
        if (requireBeatAtLeast >= 0 && requireSequencer != null
            && requireSequencer.CurrentBeat < requireBeatAtLeast)
        {
            Debug.Log("TRIGGER gated: needs beat " + requireBeatAtLeast
                + ", sequencer is at " + requireSequencer.CurrentBeat + " — ignored for now.", this);
            return;
        }

        used = true;
        Debug.Log("TRIGGER: player entered -> invoking OnPlayerEnter.", this);
        onPlayerEnter?.Invoke();
    }
}
