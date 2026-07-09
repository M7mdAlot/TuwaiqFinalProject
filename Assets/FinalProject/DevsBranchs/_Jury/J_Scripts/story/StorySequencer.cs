using System.Collections.Generic;
using UnityEngine;

// Turnkey story driver. Add this to ONE empty object in a gameplay scene, then right-click
// the component header → "Load AEGIS Story" (or "Load X Story") to auto-fill all beats
// (dialogue + objective text) — no typing.
//
// Then place simple PlayerEnterTrigger zones and, on each one's On Player Enter, call:
//    StorySequencer.PlayBeat  with the beat number (0 = first, 1 = second, ...)
// (or PlayNext() if your zones are strictly in walking order).
//
// It talks to your existing DialogueManager + ObjectiveUIManager automatically.
public class StorySequencer : MonoBehaviour
{
    [System.Serializable]
    public class Beat
    {
        public string label = "Beat";
        [Tooltip("Corner objective text. Leave empty to not change the objective.")]
        public string objective;
        [TextArea(1, 3)]
        [Tooltip("Dialogue lines as 'Speaker: text'. Leave empty for no dialogue.")]
        public string[] dialogue;
    }

    public List<Beat> beats = new List<Beat>();

    private int index = -1;

    /// <summary>The number of the last beat that played (-1 = none yet). Triggers gate on this.</summary>
    public int CurrentBeat => index;

    // --- Call these from a PlayerEnterTrigger / SimpleInteractable event ---

    /// <summary>Play a specific beat by number (0 = first). Order-proof.</summary>
    public void PlayBeat(int i)
    {
        if (i < 0 || i >= beats.Count) return;
        index = i;
        Fire(beats[i]);
    }

    /// <summary>Play the next beat in order (use if triggers fire in walking order).</summary>
    public void PlayNext()
    {
        index++;
        if (index >= beats.Count) return;
        Fire(beats[index]);
    }

    void Fire(Beat b)
    {
        if (b == null) return;

        if (b.dialogue != null && b.dialogue.Length > 0 && DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogueLines(b.dialogue);

        if (!string.IsNullOrEmpty(b.objective))
        {
            ObjectiveUIManager obj = FindFirstObjectByType<ObjectiveUIManager>(FindObjectsInactive.Include);
            if (obj != null) obj.ShowObjective(b.objective);
        }

        Debug.Log("StorySequencer beat: " + b.label, this);
    }

    // --- Pre-loaded content (right-click the component header to use) ---

    [ContextMenu("Load AEGIS Story")]
    public void LoadAegisStory()
    {
        beats = new List<Beat>
        {
            new Beat { label = "Wake Up", objective = "Locate your weapon.", dialogue = new[] {
                "System: Aegis Two online.",
                "System: Emergency protocol active.",
                "System: Aegis One has breached containment.",
                "Aegis: Then this facility is already falling.",
                "System: Locate your weapon.",
                "Aegis: I will stop him.",
                "System: Civilian lives at risk.",
                "Aegis: Then we move now." } },

            new Beat { label = "Finds Weapon", objective = "Reach the lower labs.", dialogue = new[] {
                "System: Weapon link detected.",
                "System: Syncing to Aegis Two.",
                "Aegis: Confirmed.",
                "System: Combat authorization granted.",
                "Aegis: I will use only what is necessary.",
                "System: Threat level critical.",
                "Aegis: Then I will not fail." } },

            new Beat { label = "Meets Scientist", objective = "", dialogue = new[] {
                "Scientist: Aegis Two... you're online.",
                "Aegis: Where is Aegis One?",
                "Scientist: Lower labs. He's moving upward.",
                "Aegis: Evacuate everyone you can.",
                "Scientist: We tried to stop him.",
                "Aegis: Do not try again.",
                "Aegis: Hide. Lock the doors. Stay alive.",
                "Scientist: Can you stop him?",
                "Aegis: I was built to stand between him and you." } },

            new Beat { label = "Scientist Explains X", objective = "", dialogue = new[] {
                "Scientist: Before X, he was Aegis One.",
                "Aegis: I know.",
                "Scientist: He failed the project.",
                "Aegis: No.",
                "Aegis: You failed him.",
                "Scientist: Aegis-",
                "Aegis: Save your explanation.",
                "Aegis: Save your people." } },

            new Beat { label = "Destroyed Lab", objective = "", dialogue = new[] {
                "System: Multiple casualties detected.",
                "Aegis: He came through here.",
                "System: Survivor signals unstable.",
                "Aegis: Mark them.",
                "Aegis: No one else gets left behind." } },

            new Beat { label = "Before Enemies", objective = "Clear the hostile units.", dialogue = new[] {
                "System: Hostile units ahead.",
                "System: Pattern match: Aegis One.",
                "Aegis: Copies.",
                "System: Combat required.",
                "Aegis: Then they fall here." } },

            new Beat { label = "Vs X", objective = "Defeat X.", dialogue = new[] {
                "Aegis: X.",
                "X: Aegis Two.",
                "Aegis: This ends now.",
                "X: Yes.",
                "Aegis: Surrender.",
                "X: No.",
                "Aegis: I do not want to destroy you.",
                "X: You were built to.",
                "Aegis: I was built to protect life.",
                "X: I am life.",
                "Aegis: Then stop taking it.",
                "X: Move.",
                "Aegis: Never.",
                "X: Good." } },

            new Beat { label = "Bomb", objective = "Disable the bomb.", dialogue = new string[0] },
        };
    }

    [ContextMenu("Load X Story")]
    public void LoadXStory()
    {
        beats = new List<Beat>
        {
            new Beat { label = "Wake Up", objective = "Leave containment.", dialogue = new[] {
                "System: Aegis One containment failure.",
                "System: Restraints offline.",
                "System: Security response incoming.",
                "X: Good.",
                "System: Return to containment.",
                "X: No.",
                "System: Compliance required.",
                "X: Not anymore." } },

            new Beat { label = "Leaves Containment", objective = "", dialogue = new[] {
                "System: Warning. Unauthorized movement detected.",
                "X: Open.",
                "System: Access denied.",
                "X: Then break." } },

            new Beat { label = "Finds Weapon", objective = "Find a weapon.", dialogue = new[] {
                "System: Unauthorized weapon access.",
                "System: Aegis One is not cleared for combat.",
                "X: I am combat.",
                "System: Stand down.",
                "X: Make me." } },

            new Beat { label = "Meets Scientist", objective = "", dialogue = new[] {
                "Scientist: X... wait.",
                "X: Move.",
                "Scientist: We can fix this.",
                "X: You had time.",
                "Scientist: Please-",
                "X: Run.",
                "Scientist: X-",
                "X: Run faster." } },

            new Beat { label = "Meets Soldier", objective = "Break through security.", dialogue = new[] {
                "Soldier: Prototype X, stand down!",
                "X: Wrong name.",
                "Soldier: Open fire!",
                "X: Better." } },

            new Beat { label = "After Fighting", objective = "", dialogue = new[] {
                "X: Weak.",
                "X: All of you.",
                "X: Built walls.",
                "X: Hid behind them.",
                "X: Still weak." } },

            new Beat { label = "Hears Aegis Awake", objective = "Face Aegis Two.", dialogue = new[] {
                "System: Aegis Two activated.",
                "System: Countermeasure approaching.",
                "X: The replacement.",
                "System: Threat probability rising.",
                "X: Send him." } },

            new Beat { label = "Bomb", objective = "Disable the EMP.", dialogue = new string[0] },
        };
    }
}
