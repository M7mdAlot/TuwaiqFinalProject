using UnityEngine;
using UnityEngine.AI;

// Makes an NPC start already dead — snaps to the end of the death animation (lying on the
// ground) and disables its AI + NavMeshAgent so it just lies there. Drop on scientist
// corpses scattered through the level for atmosphere.
public class StartDead : MonoBehaviour
{
    public Animator animator;
    [Tooltip("The death state name in the controller (jumps to its END = lying pose).")]
    public string deathStateName = "death";
    public string isDeadParam = "IsDead";

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            // Flag dead...
            foreach (AnimatorControllerParameter p in animator.parameters)
                if (p.name == isDeadParam && p.type == AnimatorControllerParameterType.Bool)
                    animator.SetBool(isDeadParam, true);

            // ...and snap straight to the END of the death clip (the body already on the floor).
            if (!string.IsNullOrEmpty(deathStateName))
                animator.Play(deathStateName, 0, 1f);
        }

        // Stop it from wandering / being an active NPC.
        ScientistNPCNavMeshAI ai = GetComponent<ScientistNPCNavMeshAI>();
        if (ai != null) ai.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
    }
}
