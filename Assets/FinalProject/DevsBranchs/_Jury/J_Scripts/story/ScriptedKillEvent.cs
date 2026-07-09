using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Aegis.Player;

// Scripted set-piece: the player walks in and finds X executing a scientist.
// Flow: freeze player -> X and scientist face each other -> (optional) conversation ->
// X raises weapon and opens fire -> scientist flinches then dies (full death anim) ->
// player regains control and X turns hostile so you can move and shoot X.
//
// Wire to a PlayerEnterTrigger's "On Player Enter" -> ScriptedKillEvent.TriggerEvent.
public class ScriptedKillEvent : MonoBehaviour
{
    [Header("X (the killer)")]
    public Animator xAnimator;
    public EnemyRobotAI xAI;
    public Transform xTransform;

    [Header("Victim (the scientist)")]
    public ScientistNPCNavMeshAI victim;
    public Animator victimAnimator;
    public Transform victimTransform;

    [Header("Conversation (optional — plays before the shots)")]
    [Tooltip("Lines as 'Speaker: text'. Leave empty to skip the talk.")]
    [TextArea(1, 3)] public string[] dialogue = new[]
    {
        "Scientist: X... please, we can still fix this.",
        "Aegis: Step away from him, X.",
        "X: You had your chance.",
        "Scientist: No- wait-",
        "Aegis: Don't!",
        "X: Too late."
    };

    [Header("Timing (seconds)")]
    public float faceDelay = 0.5f;
    public float weaponRaiseDelay = 0.5f;
    public float shootDuration = 1.4f;
    public float endPause = 0.5f;

    [Header("Animator params")]
    public string xHasWeaponParam = "HasWeapon";
    public string xFireAutoParam = "FireAuto";
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
        if (xAI != null) xAI.enabled = false;

        // Stop the scientist wandering for the whole scene — it just stands and reacts.
        if (victim != null)
        {
            victim.enabled = false;
            NavMeshAgent vAgent = victim.GetComponent<NavMeshAgent>();
            if (vAgent != null && vAgent.isOnNavMesh) vAgent.isStopped = true;
        }

        // Face each other.
        FaceToward(xTransform, victimTransform);
        FaceToward(victimTransform, xTransform);

        // X raises the rifle early so the shooting pose is ready by the time it fires.
        SetBool(xAnimator, xHasWeaponParam, true);

        yield return new WaitForSeconds(faceDelay);

        // Conversation.
        if (dialogue != null && dialogue.Length > 0 && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogueLines(dialogue);
            yield return new WaitForSeconds(0.3f);
            while (DialogueManager.Instance.IsDialogueOpen())
                yield return null;
        }

        yield return new WaitForSeconds(weaponRaiseDelay);

        // X opens fire.
        SetBool(xAnimator, xFireAutoParam, true);

        // Scientist flinches from the shots partway through.
        yield return new WaitForSeconds(shootDuration * 0.4f);
        SetTrigger(victimAnimator, victimDamageParam);
        yield return new WaitForSeconds(shootDuration * 0.3f);
        SetTrigger(victimAnimator, victimDamageParam);
        yield return new WaitForSeconds(shootDuration * 0.3f);

        // Scientist dies (full death animation) and X stops firing.
        if (victim != null) victim.Die();
        SetBool(xAnimator, xFireAutoParam, false);

        yield return new WaitForSeconds(endPause);

        // Hand control back and let X come for the player.
        FreezePlayers(false);
        if (xAI != null) xAI.enabled = true;
    }

    void FaceToward(Transform who, Transform target)
    {
        if (who == null || target == null) return;
        Vector3 dir = target.position - who.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            who.rotation = Quaternion.LookRotation(dir);
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

    void SetTrigger(Animator a, string p)
    {
        if (a == null || string.IsNullOrEmpty(p)) return;
        foreach (AnimatorControllerParameter param in a.parameters)
            if (param.name == p) { a.SetTrigger(p); return; }
    }
}
