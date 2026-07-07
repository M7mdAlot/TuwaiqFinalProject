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

        if (!isPlayer) return;

        used = true;
        onPlayerEnter?.Invoke();
    }
}
