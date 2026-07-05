using System.Collections;
using UnityEngine;

public class AegisMainEventManager : MonoBehaviour
{
    [Header("References")]
    public AegisStoryManager storyManager;
    public TinyToasterController playerController;

    [Header("Scene Objects")]
    public GameObject scientistAlive;
    public GameObject scientistDead;
    public GameObject xBoss;

    [Header("Timing")]
    public float beforeKillDelay = 1f;
    public float afterKillDelay = 1f;

    private bool played;

    public void PlayMainEvent()
    {
        if (played) return;

        played = true;
        StartCoroutine(EventRoutine());
    }

    IEnumerator EventRoutine()
    {
        if (playerController != null)
            playerController.SetMovementLocked(true);

        if (xBoss != null)
            xBoss.SetActive(true);

        yield return new WaitForSeconds(beforeKillDelay);

        if (scientistAlive != null)
            scientistAlive.SetActive(false);

        if (scientistDead != null)
            scientistDead.SetActive(true);

        yield return new WaitForSeconds(afterKillDelay);

        if (playerController != null)
            playerController.SetMovementLocked(false);

        if (storyManager != null)
            storyManager.StartBossFight();
    }
}