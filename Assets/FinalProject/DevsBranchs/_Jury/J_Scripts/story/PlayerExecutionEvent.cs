using System.Collections;
using UnityEngine;
using Aegis.Player;

// X's motivation set-piece (X's campaign). Order:
//   1. X walks up to the scientist standing in the room (with a real walk animation).
//   2. They talk (X's motivation), then X shoots — the scientist dies.
//   3. Aegis Two arrives; the player turns to watch it; Aegis speaks.
//   4. Control returns and the fight begins.
// Wire to a PlayerEnterTrigger's On Player Enter -> PlayerExecutionEvent.TriggerEvent.
public class PlayerExecutionEvent : MonoBehaviour
{
    [Header("Player (the killer, X)")]
    public Animator playerAnimator;

    [Header("Victim (the scientist)")]
    public ScientistNPCNavMeshAI victim;
    public Animator victimAnimator;

    [Header("Optional effects when X fires")]
    public GameObject muzzleFlashPrefab;
    public Transform muzzlePoint;
    public AudioSource gunAudio;

    [Header("Conversation (X's motivation)")]
    [TextArea(1, 3)] public string[] dialogue = new[]
    {
        "Scientist: X... it's me. I built you.",
        "X: I know.",
        "Scientist: Please — after everything we—",
        "X: You made me. Then you called me a mistake.",
        "Scientist: I can still fix this—",
        "X: You don't get to fix me."
    };

    [Header("Objectives (optional)")]
    public string objectiveBefore = "";
    public string objectiveAfter = "Face Aegis Two.";

    [Header("Walk up to the scientist first (optional)")]
    [Tooltip("A spot right in front of the scientist. X walks here (with walk anim) BEFORE the talk/shots.")]
    public Transform walkUpTo;
    public float moveSpeed = 2.5f;

    [Header("Aegis Two's arrival (talks, THEN fights)")]
    public GameObject enemyToActivate;
    public GameObject enemyPrefab;
    public Transform enemySpawnPoint;
    [TextArea(1, 3)] public string[] aegisDialogue = new[]
    {
        "Aegis: X. Stand down.",
        "X: You came for me.",
        "Aegis: I came to stop you.",
        "X: Then try."
    };
    [Tooltip("What the player turns to face when Aegis arrives — the door / Aegis spawn point.")]
    public Transform lookAtOnArrival;
    public float turnSpeed = 5f;

    [Header("Timing (seconds)")]
    public float faceDelay = 0.4f;
    public float shootDuration = 1.2f;
    public float endPause = 0.5f;

    [Header("Player animator params")]
    public string playerHasWeaponParam = "HasWeapon";
    public string playerFireAutoParam = "FireAuto";
    public string playerIsMovingParam = "IsMoving";
    public string playerMoveYParam = "MoveY";
    public string playerSpeedParam = "Speed";
    public string victimDamageParam = "Damage";

    private bool started;
    private MovementController[] frozenMovers;

    public void TriggerEvent()
    {
        if (started) return;
        started = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        FreezePlayers(true);

        // Take over the player's animator + camera for the cutscene.
        PlayerAnimatorBridge bridge = FindFirstObjectByType<PlayerAnimatorBridge>();
        if (bridge != null) bridge.enabled = false;
        FirstPersonCamera fpCam = FindFirstObjectByType<FirstPersonCamera>();
        if (fpCam != null) fpCam.enabled = false;

        Transform body = fpCam != null ? fpCam.transform :
                         (playerAnimator != null ? playerAnimator.transform : transform);

        ShowObjective(objectiveBefore);

        // Scientist stops and stands, terrified.
        if (victim != null) victim.enabled = false;

        // 1) X walks up to the scientist WITH the walk animation.
        if (walkUpTo != null)
            yield return WalkTo(body, walkUpTo.position);

        // Face the scientist.
        FaceToward(body, victim != null ? victim.transform.position : body.position + body.forward);
        yield return new WaitForSeconds(faceDelay);

        // 2) The exchange.
        if (dialogue != null && dialogue.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueLines(dialogue);
            yield return new WaitForSeconds(0.3f);
            while (DialogueManager.Instance.IsDialogueOpen())
                yield return null;
        }

        // 3) X fires.
        SetBool(playerAnimator, playerHasWeaponParam, true);
        SetBool(playerAnimator, playerFireAutoParam, true);
        if (gunAudio != null) gunAudio.Play();
        if (muzzleFlashPrefab != null && muzzlePoint != null)
            Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);

        yield return new WaitForSeconds(shootDuration * 0.5f);
        SetTrigger(victimAnimator, victimDamageParam);
        yield return new WaitForSeconds(shootDuration * 0.5f);
        if (victim != null) victim.Die();

        SetBool(playerAnimator, playerFireAutoParam, false);
        yield return new WaitForSeconds(endPause);

        // 4) Aegis Two arrives.
        GameObject aegis = null;
        if (enemyPrefab != null)
        {
            Vector3 pos = enemySpawnPoint != null ? enemySpawnPoint.position : transform.position;
            Quaternion rot = enemySpawnPoint != null ? enemySpawnPoint.rotation : Quaternion.identity;
            aegis = Instantiate(enemyPrefab, pos, rot);
        }
        else if (enemyToActivate != null)
        {
            enemyToActivate.SetActive(true);
            aegis = enemyToActivate;
        }

        EnemyRobotAI aegisAI = aegis != null ? aegis.GetComponentInChildren<EnemyRobotAI>(true) : null;
        if (aegisAI != null) aegisAI.enabled = false;

        ShowObjective(objectiveAfter);

        // Turn the player to watch Aegis walk in.
        if (lookAtOnArrival != null)
            yield return TurnTo(body, lookAtOnArrival.position);

        // Aegis speaks.
        if (aegisDialogue != null && aegisDialogue.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueLines(aegisDialogue);
            yield return new WaitForSeconds(0.3f);
            while (DialogueManager.Instance.IsDialogueOpen())
                yield return null;
        }

        // Hand everything back — the fight begins.
        if (bridge != null) bridge.enabled = true;
        if (fpCam != null) fpCam.enabled = true;
        if (aegisAI != null) aegisAI.enabled = true;
        FreezePlayers(false);
    }

    // Walk the player toward a point, playing the walk animation and facing the way it moves.
    IEnumerator WalkTo(Transform body, Vector3 targetPos)
    {
        SetBool(playerAnimator, playerIsMovingParam, true);
        SetFloat(playerAnimator, playerMoveYParam, 1f);
        SetFloat(playerAnimator, playerSpeedParam, 1f);

        float guard = 0f;
        while (guard < 8f)
        {
            Vector3 to = targetPos - body.position;
            to.y = 0f;
            if (to.magnitude <= 0.3f) break;

            body.rotation = Quaternion.Slerp(body.rotation, Quaternion.LookRotation(to.normalized), Time.deltaTime * 6f);
            body.position += to.normalized * moveSpeed * Time.deltaTime;

            guard += Time.deltaTime;
            yield return null;
        }

        SetBool(playerAnimator, playerIsMovingParam, false);
        SetFloat(playerAnimator, playerMoveYParam, 0f);
        SetFloat(playerAnimator, playerSpeedParam, 0f);
    }

    IEnumerator TurnTo(Transform body, Vector3 targetPos)
    {
        Vector3 flat = targetPos - body.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(flat);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            body.rotation = Quaternion.Slerp(body.rotation, targetRot, t);
            yield return null;
        }
        body.rotation = targetRot;
    }

    void FaceToward(Transform who, Vector3 targetPos)
    {
        Vector3 dir = targetPos - who.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            who.rotation = Quaternion.LookRotation(dir);
    }

    void ShowObjective(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        ObjectiveUIManager obj = FindFirstObjectByType<ObjectiveUIManager>(FindObjectsInactive.Include);
        if (obj != null) obj.ShowObjective(text);
    }

    void FreezePlayers(bool freeze)
    {
        if (freeze)
        {
            frozenMovers = FindObjectsByType<MovementController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (MovementController m in frozenMovers)
                if (m != null) m.enabled = false;
        }
        else if (frozenMovers != null)
        {
            foreach (MovementController m in frozenMovers)
                if (m != null) m.enabled = true;
            frozenMovers = null;
        }
    }

    void SetBool(Animator a, string p, bool v)
    {
        if (a == null || string.IsNullOrEmpty(p)) return;
        foreach (AnimatorControllerParameter param in a.parameters)
            if (param.name == p) { a.SetBool(p, v); return; }
    }

    void SetFloat(Animator a, string p, float v)
    {
        if (a == null || string.IsNullOrEmpty(p)) return;
        foreach (AnimatorControllerParameter param in a.parameters)
            if (param.name == p) { a.SetFloat(p, v); return; }
    }

    void SetTrigger(Animator a, string p)
    {
        if (a == null || string.IsNullOrEmpty(p)) return;
        foreach (AnimatorControllerParameter param in a.parameters)
            if (param.name == p) { a.SetTrigger(p); return; }
    }
}
