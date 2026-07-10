using System.Collections;
using UnityEngine;
using Aegis.Systems;

// The whole post-boss bomb sequence in one call: a warning/monologue, opens the locked door,
// shows the objective, then starts the crisis countdown. Wire the boss enemy's "On Death"
// (EnemyRobotAI) -> BombSequenceEvent.TriggerEvent.
public class BombSequenceEvent : MonoBehaviour
{
    [Header("Warning (X realises there's a bomb)")]
    [TextArea(1, 3)] public string[] dialogue = new[]
    {
        "System: Warning. Detonation sequence armed.",
        "System: EMP charge active. Two minutes to detonation.",
        "X: A bomb. Of course.",
        "X: Not like this. I shut it down."
    };

    [Header("Open the locked door when this starts")]
    [Tooltip("The door that was blocking the way — turned OFF (opened) when the sequence begins.")]
    public GameObject doorToOpen;

    [Header("Objective")]
    public string objective = "Disable the bomb.";

    [Header("Crisis (the timer + defuse)")]
    public CrisisManager crisisManager;
    public float delayBeforeCrisis = 0.8f;

    private bool started;

    // Wire the boss's On Death event to this.
    public void TriggerEvent()
    {
        if (started) return;
        started = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1) Warning dialogue / X monologue.
        if (dialogue != null && dialogue.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueLines(dialogue);
            yield return new WaitForSeconds(0.3f);
            while (DialogueManager.Instance.IsDialogueOpen())
                yield return null;
        }

        // 2) Open the locked door.
        if (doorToOpen != null) doorToOpen.SetActive(false);

        // 3) Objective.
        ShowObjective(objective);

        yield return new WaitForSeconds(delayBeforeCrisis);

        // 4) Start the 2-minute countdown (set the CrisisManager's Duration to 120).
        if (crisisManager != null) crisisManager.TriggerCrisis();
    }

    void ShowObjective(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        ObjectiveUIManager obj = FindFirstObjectByType<ObjectiveUIManager>(FindObjectsInactive.Include);
        if (obj != null) obj.ShowObjective(text);
    }
}
