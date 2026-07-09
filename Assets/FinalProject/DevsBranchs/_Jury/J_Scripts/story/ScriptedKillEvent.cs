using System.Collections;
using UnityEngine;
using Aegis.Player;

// A scripted set-piece: the player walks in and finds X executing a scientist.
// Sequence: freeze the player to watch -> X turns to the scientist and opens fire ->
// the scientist dies (full death animation) -> the player regains control and X turns
// hostile so you can move and shoot X.
//
// Wire this to a PlayerEnterTrigger's "On Player Enter" -> ScriptedKillEvent.TriggerEvent.
public class ScriptedKillEvent : MonoBehaviour
{
    [Header("X (the killer)")]
    public Animator xAnimator;
    [Tooltip("X's EnemyRobotAI — disabled during the scripted kill, re-enabled after so X hunts you.")]
    public EnemyRobotAI xAI;
    public Transform xTransform;

    [Header("Victim (the scientist)")]
    public ScientistNPCNavMeshAI victim;
    public Transform victimTransform;

    [Header("Timing (seconds)")]
    public float aimDelay = 0.7f;       // X turns + raises weapon
    public float shootDuration = 1.3f;  // how long X fires before the scientist drops
    public float endPause = 0.4f;       // small beat after the kill before you get control

    [Header("X animator params")]
    public string hasWeaponParam = "HasWeapon";
    public string fireAutoParam = "FireAuto";

    private bool started;
    private MovementController[] frozenMovers;

    // Call this from a PlayerEnterTrigger -> On Player Enter.
    public void TriggerEvent()
    {
        if (started) return;
        started = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        FreezePlayers(true);              // player watches, can't move
        if (xAI != null) xAI.enabled = false; // X focuses the scientist, not the player

        // X turns to face the scientist.
        if (xTransform != null && victimTransform != null)
        {
            Vector3 dir = victimTransform.position - xTransform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                xTransform.rotation = Quaternion.LookRotation(dir);
        }

        yield return new WaitForSeconds(aimDelay);

        // X opens fire (rifle shooting animation).
        SetBool(hasWeaponParam, true);
        SetBool(fireAutoParam, true);

        yield return new WaitForSeconds(shootDuration);

        // Scientist dies with its full death animation and freezes.
        if (victim != null) victim.Die();

        // X stops firing.
        SetBool(fireAutoParam, false);

        yield return new WaitForSeconds(endPause);

        // Hand control back and let X come for the player.
        FreezePlayers(false);
        if (xAI != null) xAI.enabled = true;
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

    void SetBool(string p, bool v)
    {
        if (xAnimator == null || string.IsNullOrEmpty(p)) return;
        foreach (AnimatorControllerParameter param in xAnimator.parameters)
            if (param.name == p) { xAnimator.SetBool(p, v); return; }
    }
}
