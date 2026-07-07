using UnityEngine;
using UnityEngine.AI;
using Aegis.Core;

// Turns an Aegis or X model into a HOSTILE NPC that hunts and kills the player.
// - In Aegis's campaign, use this on the corrupted X-copies (Target = Aegis).
// - In X's campaign, use this on the Aegis Two countermeasure (Target = X).
// Chases the chosen player with a NavMeshAgent and melee-punches for damage when close.
// Is itself damageable (implements IDamageable) so the player can kill it back.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRobotAI : MonoBehaviour, IDamageable
{
    public enum RobotState { Idle, Chase, Attack, Dead }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Who to hunt")]
    [Tooltip("The player type this robot attacks. Aegis campaign -> Aegis; X campaign -> X.")]
    public PlayerCharacterIdentity.PlayerType targetPlayerType = PlayerCharacterIdentity.PlayerType.Aegis;

    [Header("Detection & movement")]
    public float visionRange = 30f;   // large: a hunter that always comes for you
    public float chaseSpeed = 4.5f;
    public float attackRange = 2.2f;
    public float rotationSpeed = 10f;

    [Header("Attack")]
    public float damage = 12f;
    public float attackCooldown = 1.1f;
    [Tooltip("Delay after the punch starts before damage lands (sync to the punch anim).")]
    public float damageDelay = 0.35f;

    [Header("Health")]
    public float maxHealth = 120f;

    [Header("Animator params")]
    public string speedParam = "Speed";
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string isRunningParam = "IsRunning";
    public string leftPunchParam = "LeftPunch";
    public string rightPunchParam = "RightPunch";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    private RobotState state = RobotState.Idle;
    private Transform target;
    private float nextAttackTime;
    private float currentHealth;

    public bool IsAlive => state != RobotState.Dead;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;

        if (animator != null) animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.updateRotation = true;
        }
    }

    void Update()
    {
        if (state == RobotState.Dead || agent == null) return;

        FindTarget();

        switch (state)
        {
            case RobotState.Idle: HandleIdle(); break;
            case RobotState.Chase: HandleChase(); break;
            case RobotState.Attack: HandleAttack(); break;
        }

        UpdateAnimator();
    }

    void FindTarget()
    {
        if (target == null)
        {
            PlayerCharacterIdentity[] players =
                FindObjectsByType<PlayerCharacterIdentity>(FindObjectsSortMode.None);

            float closest = Mathf.Infinity;
            foreach (PlayerCharacterIdentity p in players)
            {
                if (p == null || p.playerType != targetPlayerType) continue;

                float d = Vector3.Distance(transform.position, p.transform.position);
                if (d < closest) { closest = d; target = p.transform; }
            }
        }

        if (target == null) { state = RobotState.Idle; return; }

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist > visionRange) state = RobotState.Idle;
        else if (dist <= attackRange) state = RobotState.Attack;
        else state = RobotState.Chase;
    }

    void HandleIdle()
    {
        agent.isStopped = true;
    }

    void HandleChase()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(target.position);
    }

    void HandleAttack()
    {
        agent.isStopped = true;
        FaceTarget();

        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        // Random left/right punch to match the Aegis/X punch states.
        SetTrigger(Random.value < 0.5f ? leftPunchParam : rightPunchParam);

        // Damage lands slightly after the swing starts.
        Invoke(nameof(DealDamage), damageDelay);
    }

    void DealDamage()
    {
        if (state == RobotState.Dead || target == null) return;

        // Only connect if the player is still in range when the punch lands.
        if (Vector3.Distance(transform.position, target.position) > attackRange + 0.6f) return;

        IDamageable dmg = target.GetComponentInParent<IDamageable>();
        if (dmg != null && dmg.IsAlive) dmg.TakeDamage(damage);
    }

    void FaceTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = agent.velocity.magnitude;
        Vector3 local = transform.InverseTransformDirection(agent.velocity);
        float norm = Mathf.Max(agent.speed, 0.01f);

        SetFloat(speedParam, speed);
        SetFloat(moveXParam, local.x / norm);
        SetFloat(moveYParam, local.z / norm);
        SetBool(isRunningParam, speed > 0.1f);
    }

    // ---- IDamageable: the player can kill this robot ----
    public void TakeDamage(float amount)
    {
        if (state == RobotState.Dead) return;

        currentHealth -= amount;
        SetTrigger(damageParam);

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        state = RobotState.Dead;

        if (agent != null) { agent.ResetPath(); agent.isStopped = true; }

        SetBool(isRunningParam, false);
        SetBool(isDeadParam, true);
        CancelInvoke();
    }

    // ---- safe animator setters (only touch params that exist) ----
    void SetFloat(string p, float v) { if (HasParam(p)) animator.SetFloat(p, v); }
    void SetBool(string p, bool v) { if (HasParam(p)) animator.SetBool(p, v); }
    void SetTrigger(string p) { if (HasParam(p)) animator.SetTrigger(p); }

    bool HasParam(string p)
    {
        if (animator == null || string.IsNullOrEmpty(p)) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
            if (param.name == p) return true;
        return false;
    }
}
