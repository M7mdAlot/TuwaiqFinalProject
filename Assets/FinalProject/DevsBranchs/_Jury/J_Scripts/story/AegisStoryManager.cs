using UnityEngine;
using UnityEngine.Events;

public class AegisStoryManager : MonoBehaviour
{
    public enum AegisStep
    {
        WakeUp,
        FindWeapon,
        ReachLowerLabs,
        FirstFight,
        FindMainLab,
        MainEvent,
        BossFight,
        BombRun,
        FinalCopy,
        DisableBomb,
        GoodEnding,
        BadEnding
    }

    [Header("Current Step")]
    public AegisStep currentStep = AegisStep.WakeUp;

    [Header("UI")]
    public GameObject objectiveUIObject;

    [Header("Player / Weapon")]
    public AegisWeaponController weaponController;

    [Header("Story Objects")]
    public GameObject bossX;
    public GameObject finalCopy;
    public GameObject bombDevice;

    [Header("Start Settings")]
    public bool startAutomatically = true;
    public bool disableWeaponAtStart = true;

    [Header("Events - Connect Core Systems Here")]
    public UnityEvent onAegisPathStarted;
    public UnityEvent onWeaponPickedUp;
    public UnityEvent onFirstFightStarted;
    public UnityEvent onFirstWaveCleared;
    public UnityEvent onMainEventStarted;
    public UnityEvent onBossFightStarted;
    public UnityEvent onBossDefeated;
    public UnityEvent onBombRunStarted;
    public UnityEvent onFinalCopyFightStarted;
    public UnityEvent onFinalCopyDefeated;
    public UnityEvent onBombDisabled;
    public UnityEvent onBombExpired;
    public UnityEvent onGoodEnding;
    public UnityEvent onBadEnding;

    void Start()
    {
        if (bossX != null)
            bossX.SetActive(false);

        if (finalCopy != null)
            finalCopy.SetActive(false);

        if (bombDevice != null)
            bombDevice.SetActive(false);

        if (disableWeaponAtStart && weaponController != null)
            weaponController.enabled = false;

        if (startAutomatically)
            BeginAegisPath();
    }

    public void BeginAegisPath()
    {
        currentStep = AegisStep.FindWeapon;

        ShowObjective("Locate your weapon.");

        onAegisPathStarted?.Invoke();
    }

    public void OnWeaponPickedUp()
    {
        currentStep = AegisStep.ReachLowerLabs;

        if (weaponController != null)
            weaponController.enabled = true;

        ShowObjective("Reach the lower labs.");

        onWeaponPickedUp?.Invoke();
    }

    public void StartFirstFight()
    {
        if (currentStep == AegisStep.FirstFight)
            return;

        currentStep = AegisStep.FirstFight;

        ShowObjective("Clear the hostile units.");

        onFirstFightStarted?.Invoke();
    }

    public void OnFirstWaveCleared()
    {
        currentStep = AegisStep.FindMainLab;

        ShowObjective("Find the main lab.");

        onFirstWaveCleared?.Invoke();
    }

    public void StartMainEvent()
    {
        currentStep = AegisStep.MainEvent;

        ShowObjective("Defeat X.");

        if (bossX != null)
            bossX.SetActive(true);

        onMainEventStarted?.Invoke();
    }

    public void StartBossFight()
    {
        currentStep = AegisStep.BossFight;

        ShowObjective("Defeat X.");

        if (bossX != null)
            bossX.SetActive(true);

        onBossFightStarted?.Invoke();
    }

    public void OnBossDefeated()
    {
        currentStep = AegisStep.BombRun;

        ShowObjective("Return to the activation chamber.");

        if (finalCopy != null)
            finalCopy.SetActive(true);

        onBossDefeated?.Invoke();
        onBombRunStarted?.Invoke();
    }

    public void StartFinalCopyFight()
    {
        currentStep = AegisStep.FinalCopy;

        ShowObjective("Eliminate the final copy.");

        if (finalCopy != null)
            finalCopy.SetActive(true);

        onFinalCopyFightStarted?.Invoke();
    }

    public void OnFinalCopyDefeated()
    {
        currentStep = AegisStep.DisableBomb;

        ShowObjective("Disable the bomb.");

        if (bombDevice != null)
            bombDevice.SetActive(true);

        onFinalCopyDefeated?.Invoke();
    }

    public void OnBombDisabled()
    {
        currentStep = AegisStep.GoodEnding;

        ShowObjective("Bomb disabled.");

        onBombDisabled?.Invoke();
        onGoodEnding?.Invoke();
    }

    public void OnBombExpired()
    {
        currentStep = AegisStep.BadEnding;

        ShowObjective("Detonation failed to stop.");

        onBombExpired?.Invoke();
        onBadEnding?.Invoke();
    }

    public void ShowGoodEnding()
    {
        currentStep = AegisStep.GoodEnding;

        onGoodEnding?.Invoke();
    }

    public void ShowBadEnding()
    {
        currentStep = AegisStep.BadEnding;

        onBadEnding?.Invoke();
    }

    public void ShowObjective(string objectiveText)
    {
        Debug.Log("OBJECTIVE: " + objectiveText);

        if (objectiveUIObject != null)
        {
            objectiveUIObject.SendMessage(
                "ShowObjective",
                objectiveText,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }
}