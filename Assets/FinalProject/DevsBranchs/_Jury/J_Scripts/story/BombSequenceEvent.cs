using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Aegis.Systems;

// The whole post-boss bomb sequence in one call: a warning/monologue, opens the locked door,
// shows the objective, then starts the crisis countdown. Wire the boss enemy's "On Death"
// (EnemyRobotAI) -> BombSequenceEvent.TriggerEvent.
public class BombSequenceEvent : MonoBehaviour
{
    [Header("Warning (the reactor is overloading)")]
    [TextArea(1, 3)] public string[] dialogue = new[]
    {
        "System: Warning. Reactor overload detected.",
        "System: Core meltdown in two minutes.",
        "System: Reach the reactor and shut it down.",
        "Aegis: Then I move now."
    };

    [Header("Open the locked door when this starts")]
    [Tooltip("The door that was blocking the way — turned OFF (opened) when the sequence begins.")]
    public GameObject doorToOpen;

    [Header("Objective")]
    public string objective = "Shut down the reactor.";

    [Header("Crisis (the timer + defuse)")]
    public CrisisManager crisisManager;
    public float delayBeforeCrisis = 0.8f;

    [Header("TEST: press this key to start the bomb NOW (turn OFF for the build)")]
    public bool enableTestKey = true;
    public Key testKey = Key.B;

    private bool started;

    void Update()
    {
        if (enableTestKey && Keyboard.current != null && Keyboard.current[testKey].wasPressedThisFrame)
        {
            Debug.Log("BombSequenceEvent: TEST KEY -> starting the bomb.", this);
            TriggerEvent();
        }
    }

    // Wire the boss's On Death event to this.
    public void TriggerEvent()
    {
        if (started) return;
        started = true;

        // Always resolve a CrisisManager, even if the Inspector reference is empty/broken.
        if (crisisManager == null) crisisManager = FindFirstObjectByType<CrisisManager>();

        Debug.Log("BombSequenceEvent: TriggerEvent -> crisisManager=" +
                  (crisisManager != null ? crisisManager.name : "NULL"), this);

        // START THE TIMER IMMEDIATELY so it's guaranteed to show — the dialogue/door/objective
        // then play alongside it. (Previously it waited for you to click through the dialogue.)
        if (crisisManager != null)
        {
            Debug.Log("BombSequenceEvent: calling CrisisManager.TriggerCrisis() -> timer should appear NOW.", this);
            crisisManager.TriggerCrisis();
        }
        else
        {
            Debug.LogError("BombSequenceEvent: no CrisisManager found in the scene — the timer can't " +
                           "start. Add/enable a CrisisManager, or assign the Crisis Manager field.", this);
        }

        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // Warning dialogue / X monologue (plays while the timer is already counting down).
        if (dialogue != null && dialogue.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueLines(dialogue);
            yield return new WaitForSeconds(0.3f);
            while (DialogueManager.Instance.IsDialogueOpen())
                yield return null;
        }

        // Open the locked door.
        if (doorToOpen != null) doorToOpen.SetActive(false);

        // Objective.
        ShowObjective(objective);

        yield return null;
    }

    void ShowObjective(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        ObjectiveUIManager obj = FindFirstObjectByType<ObjectiveUIManager>(FindObjectsInactive.Include);
        if (obj != null) obj.ShowObjective(text);
    }
}
