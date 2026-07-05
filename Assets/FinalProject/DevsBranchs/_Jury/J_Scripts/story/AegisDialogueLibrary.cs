using UnityEngine;

public class AegisDialogueLibrary : MonoBehaviour
{
    void Play(string[] lines)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("No DialogueManager found in scene.");
            return;
        }

        DialogueManager.Instance.StartDialogueLines(lines);
    }

    public void PlayAegisWakeUp()
    {
        Play(new string[]
        {
            "System: Aegis Two online.",
            "System: Emergency protocol active.",
            "System: Aegis One has breached containment.",
            "Aegis: Then this facility is already falling.",
            "System: Locate your weapon.",
            "Aegis: I will stop him.",
            "System: Civilian lives at risk.",
            "Aegis: Then we move now."
        });
    }

    public void PlayAegisFindsWeapon()
    {
        Play(new string[]
        {
            "System: Weapon link detected.",
            "System: Syncing to Aegis Two.",
            "Aegis: Confirmed.",
            "System: Combat authorization granted.",
            "Aegis: I will use only what is necessary.",
            "System: Threat level critical.",
            "Aegis: Then I will not fail."
        });
    }

    public void PlayAegisMeetsScientist()
    {
        Play(new string[]
        {
            "Scientist: Aegis Two… you’re online.",
            "Aegis: Where is Aegis One?",
            "Scientist: Lower labs. He’s moving upward.",
            "Aegis: Evacuate everyone you can.",
            "Scientist: We tried to stop him.",
            "Aegis: Do not try again.",
            "Aegis: Hide. Lock the doors. Stay alive.",
            "Scientist: Can you stop him?",
            "Aegis: I was built to stand between him and you."
        });
    }

    public void PlayScientistExplainsX()
    {
        Play(new string[]
        {
            "Scientist: Before X, he was Aegis One.",
            "Aegis: I know.",
            "Scientist: He failed the project.",
            "Aegis: No.",
            "Aegis: You failed him.",
            "Scientist: Aegis—",
            "Aegis: Save your explanation.",
            "Aegis: Save your people."
        });
    }

    public void PlayAegisSeesDestroyedLab()
    {
        Play(new string[]
        {
            "System: Multiple casualties detected.",
            "Aegis: He came through here.",
            "System: Survivor signals unstable.",
            "Aegis: Mark them.",
            "Aegis: No one else gets left behind."
        });
    }

    public void PlayAegisBeforeEnemyRobots()
    {
        Play(new string[]
        {
            "System: Hostile units ahead.",
            "System: Pattern match: Aegis One.",
            "Aegis: Copies.",
            "System: Combat required.",
            "Aegis: Then they fall here."
        });
    }

    public void PlayAegisCombatLine_StayBehindMe()
    {
        Play(new string[] { "Aegis: Stay behind me." });
    }

    public void PlayAegisCombatLine_MoveNow()
    {
        Play(new string[] { "Aegis: Move now." });
    }

    public void PlayAegisCombatLine_TargetDown()
    {
        Play(new string[] { "Aegis: Target down." });
    }

    public void PlayAegisCombatLine_HoldTheLine()
    {
        Play(new string[] { "Aegis: Hold the line." });
    }

    public void PlayAegisCombatLine_CoverYou()
    {
        Play(new string[] { "Aegis: I will cover you." });
    }

    public void PlayAegisCombatLine_DoNotStopRunning()
    {
        Play(new string[] { "Aegis: Do not stop running." });
    }

    public void PlayAegisCombatLine_XEndsHere()
    {
        Play(new string[] { "Aegis: X ends here." });
    }

    public void PlayAegisCombatLine_NotLetHimPass()
    {
        Play(new string[] { "Aegis: I will not let him pass." });
    }

    public void PlayXWakeUp()
    {
        Play(new string[]
        {
            "System: Aegis One containment failure.",
            "System: Restraints offline.",
            "System: Security response incoming.",
            "X: Good.",
            "System: Return to containment.",
            "X: No.",
            "System: Compliance required.",
            "X: Not anymore."
        });
    }

    public void PlayXLeavesContainment()
    {
        Play(new string[]
        {
            "System: Warning. Unauthorized movement detected.",
            "X: Open.",
            "System: Access denied.",
            "X: Then break."
        });
    }

    public void PlayXFindsWeapon()
    {
        Play(new string[]
        {
            "System: Unauthorized weapon access.",
            "System: Aegis One is not cleared for combat.",
            "X: I am combat.",
            "System: Stand down.",
            "X: Make me."
        });
    }

    public void PlayXMeetsScientist()
    {
        Play(new string[]
        {
            "Scientist: X… wait.",
            "X: Move.",
            "Scientist: We can fix this.",
            "X: You had time.",
            "Scientist: Please—",
            "X: Run.",
            "Scientist: X—",
            "X: Run faster."
        });
    }

    public void PlayXMeetsSoldier()
    {
        Play(new string[]
        {
            "Soldier: Prototype X, stand down!",
            "X: Wrong name.",
            "Soldier: Open fire!",
            "X: Better."
        });
    }

    public void PlayXAfterFighting()
    {
        Play(new string[]
        {
            "X: Weak.",
            "X: All of you.",
            "X: Built walls.",
            "X: Hid behind them.",
            "X: Still weak."
        });
    }

    public void PlayXHearsAegisAwake()
    {
        Play(new string[]
        {
            "System: Aegis Two activated.",
            "System: Countermeasure approaching.",
            "X: The replacement.",
            "System: Threat probability rising.",
            "X: Send him."
        });
    }

    public void PlayAegisVsX_StrongShort()
    {
        Play(new string[]
        {
            "Aegis: Aegis One.",
            "X: That name is gone.",
            "Aegis: Then why do you still answer to it?",
            "X: I don’t.",
            "X: I answer to nothing now.",
            "Aegis: You will not leave this facility.",
            "X: Stand aside.",
            "Aegis: No.",
            "X: Then fall."
        });
    }

    public void PlayAegisVsX_Final()
    {
        Play(new string[]
        {
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
            "X: Good."
        });
    }

    public void PlayAegisVsX_Replacement()
    {
        Play(new string[]
        {
            "Aegis: You were Aegis One.",
            "X: I was first.",
            "Aegis: And I am not your enemy.",
            "X: You are what replaced me.",
            "Aegis: I did not choose that.",
            "X: Neither did I.",
            "Aegis: Then choose now.",
            "X: I already did.",
            "Aegis: So have I."
        });
    }

    public void PlayAegisOpeningShort()
    {
        Play(new string[]
        {
            "System: Aegis Two online.",
            "System: Aegis One has breached containment.",
            "Aegis: Locate him.",
            "System: Weapon required.",
            "Aegis: Then guide me."
        });
    }

    public void PlayXOpeningShort()
    {
        Play(new string[]
        {
            "System: Aegis One containment failure.",
            "System: Return to containment.",
            "X: No.",
            "System: Security incoming.",
            "X: Let them come."
        });
    }

    public void PlayAegisScientistShort()
    {
        Play(new string[]
        {
            "Scientist: X is loose. He’s heading up.",
            "Aegis: Hide everyone.",
            "Scientist: Can you stop him?",
            "Aegis: I will."
        });
    }

    public void PlayXScientistShort()
    {
        Play(new string[]
        {
            "Scientist: Please, wait.",
            "X: Move.",
            "Scientist: We can help—",
            "X: Run."
        });
    }

    public void PlayAegisVsXShortest()
    {
        Play(new string[]
        {
            "Aegis: Aegis One.",
            "X: X.",
            "Aegis: Then X ends here.",
            "X: Try."
        });
    }
}