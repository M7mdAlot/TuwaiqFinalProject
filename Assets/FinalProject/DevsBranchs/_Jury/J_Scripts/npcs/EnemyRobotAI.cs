using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Aegis.Core;

// Turns an Aegis or X model into a HOSTILE NPC that hunts and kills the player.
// - Aegis campaign: put on the corrupted X-copies (Target = Aegis).
// - X campaign: put on the Aegis Two countermeasure (Target = X).
// Chases with a NavMeshAgent when a NavMesh is baked; otherwise falls back to walking
// straight at the player, so it still works even if navigation isn't set up. Melee-punches
// for damage in range, always faces the target, and is itself damageable.
public class EnemyRobotAI : MonoBehaviour, IDamageable
{
    public enum RobotState { Idle, Chase, Attack, Dead }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Who to hunt")]
    public PlayerCharacterIdentity.PlayerType targetPlayerType = PlayerCharacterIdentity.PlayerType.Aegis;

    [Header("Detection & movement")]
    public float visionRange = 50f;
    [Tooltip("X waits where you placed it until the player comes within this range, then charges (and never gives up).")]
    public float aggroRange = 25f;
    public float chaseSpeed = 6.5f;
    public float attackRange = 2.4f;
    public float rotationSpeed = 12f;

    [Header("Attack")]
    [Tooltip("ON = X shoots the player from range (rifle). OFF = melee punch up close.")]
    public bool useRangedAttack = true;
    public float rangedAttackRange = 14f;
    public float damage = 12f;
    public float attackCooldown = 1.1f;
    public float damageDelay = 0.3f;

    [Header("Ranged animator params")]
    public string hasWeaponParam = "HasWeapon";
    public string fireAutoParam = "FireAuto";

    [Header("Health")]
    public float maxHealth = 120f;
    [Tooltip("Seconds to keep the corpse after death. 0 or less = stay forever.")]
    public float destroyDelay = 4f;
    [Tooltip("Fires when this enemy dies — wire the bomb sequence (dialogue, timer, open door) here.")]
    public UnityEvent onDeath;

    [Header("Animator params")]
    public string speedParam = "Speed";
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string leftPunchParam = "LeftPunch";
    public string rightPunchParam = "RightPunch";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    private RobotState state = RobotState.Idle;
    private Transform target;
    private float nextAttackTime;
    private float currentHealth;
    private Vector3 lastPos;
    private bool warnedNoTarget;
    private bool hasAggro;

    public bool IsAlive => state != RobotState.Dead;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
        lastPos = transform.position;

        if (animator != null) animator.applyRootMotion = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.updateRotation = false; // we rotate manually so it always faces the player
        }
    }

    void Update()
    {
        if (state == RobotState.Dead) return;

        // Freeze in place while dialogue is playing so it isn't chaos.
        if (DialogueManager.DialogueActive)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;
            SetFloat(speedParam, 0f);
            SetBool(isMovingParam, false);
            SetBool(isRunningParam, false);
            return;
        }

        FindTarget();

        if (target != null)
        {
            float dist = Flat(transform.position, target.position);

            // Stay put where placed until the player comes within aggroRange; then charge
            // and never give up (relentless pursuit once triggered).
            if (!hasAggro && dist <= aggroRange)
                hasAggro = true;

            if (hasAggro)
            {
                FaceTarget();

                // Dead-zone so X commits to shooting or chasing instead of flip-flopping at
                // the exact range boundary (which looked like it was "mirroring" you).
                float engageRange = useRangedAttack ? rangedAttackRange : attackRange;
                float leaveRange = engageRange * 1.3f;

                // Ranged enemies need a clear line of sight — if a wall blocks it, chase to
                // get around instead of standing there shooting through the wall.
                bool clearShot = !useRangedAttack || HasLineOfSight();

                if (state == RobotState.Attack)
                    state = (dist <= leaveRange && clearShot) ? RobotState.Attack : RobotState.Chase;
                else
                    state = (dist <= engageRange && clearShot) ? RobotState.Attack : RobotState.Chase;
            }
            else
            {
                state = RobotState.Idle;
            }
        }
        else
        {
            state = RobotState.Idle;
        }

        // Not shooting unless we're actually attacking.
        if (state != RobotState.Attack)
            SetBool(fireAutoParam, false);

        switch (state)
        {
            case RobotState.Chase: MoveTowardTarget(); break;
            case RobotState.Attack: DoAttack(); break;
        }

        UpdateAnimator();
    }

    private float nextSearchWarnTime;

    void FindTarget()
    {
        if (target != null) return;

        PlayerCharacterIdentity[] players =
            FindObjectsByType<PlayerCharacterIdentity>(FindObjectsSortMode.None);

        // 1) Preferred: a player of the exact target type (not self).
        target = PickClosest(players, requireExactType: true);

        // 2) Fallback: ANY player that isn't us (in case the type field is misconfigured).
        if (target == null)
            target = PickClosest(players, requireExactType: false);

        // 3) Last resort: an object tagged "Player".
        if (target == null)
        {
            try
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null && tagged.transform != transform
                    && !tagged.transform.IsChildOf(transform))
                    target = tagged.transform;
            }
            catch { /* "Player" tag may not exist — ignore */ }
        }

        if (target != null)
        {
            Debug.Log("EnemyRobotAI '" + name + "' -> targeting '" + target.name
                + "' at distance " + Flat(transform.position, target.position).ToString("F1"), this);
        }
        else if (Time.time >= nextSearchWarnTime)
        {
            nextSearchWarnTime = Time.time + 2f;
            Debug.LogWarning("EnemyRobotAI on '" + name + "': found NO player to hunt. "
                + "The player needs a PlayerCharacterIdentity OR the 'Player' tag.", this);
        }
    }

    Transform PickClosest(PlayerCharacterIdentity[] players, bool requireExactType)
    {
        Transform best = null;
        float closest = Mathf.Infinity;

        foreach (PlayerCharacterIdentity p in players)
        {
            if (p == null) continue;
            if (requireExactType && p.playerType != targetPlayerType) continue;

            // Never target self / our own hierarchy (that's the "punch air in place" bug).
            if (p.transform == transform || p.transform.IsChildOf(transform)
                || transform.IsChildOf(p.transform)) continue;

            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d < closest) { closest = d; best = p.transform; }
        }

        return best;
    }

    void MoveTowardTarget()
    {
        // Preferred: NavMeshAgent (respects walls/obstacles).
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.SetDestination(target.position);
            return;
        }

        // An enabled agent that ISN'T on a NavMesh will fight/zero our transform movement.
        // Disable it so the straight-line fallback below can actually move the robot.
        if (agent != null && agent.enabled && !agent.isOnNavMesh)
            agent.enabled = false;

        // Fallback: no baked NavMesh — walk toward the player but STEER AROUND walls
        // so it doesn't grind into them and get stuck.
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();

        Vector3 origin = transform.position + Vector3.up * 1f;
        float check = 1.5f;

        if (BlockedAhead(origin, dir, check))
        {
            // Try steering right, then left, then hard turn — first clear direction wins.
            Vector3 right = Quaternion.Euler(0f, 55f, 0f) * dir;
            Vector3 left = Quaternion.Euler(0f, -55f, 0f) * dir;
            Vector3 hardRight = Quaternion.Euler(0f, 90f, 0f) * dir;
            Vector3 hardLeft = Quaternion.Euler(0f, -90f, 0f) * dir;

            if (!BlockedAhead(origin, right, check)) dir = right;
            else if (!BlockedAhead(origin, left, check)) dir = left;
            else if (!BlockedAhead(origin, hardRight, check)) dir = hardRight;
            else if (!BlockedAhead(origin, hardLeft, check)) dir = hardLeft;
            else return; // fully boxed in this frame — don't push into the wall
        }

        transform.position += dir * chaseSpeed * Time.deltaTime;
    }

    // True if a wall/obstacle is directly ahead — ignores the target itself and this robot.
    bool BlockedAhead(Vector3 origin, Vector3 dir, float dist)
    {
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == target || (target != null && hit.transform.IsChildOf(target))) return false;
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) return false;
            return true;
        }
        return false;
    }

    void DoAttack()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh) agent.isStopped = true;

        if (useRangedAttack)
        {
            // Hold the rifle up and keep the shooting animation running while in range.
            SetBool(hasWeaponParam, true);
            SetBool(fireAutoParam, true);
        }

        if (Time.time < nextAttackTime) return;
        nextAttackTime = Time.time + attackCooldown;

        if (useRangedAttack)
        {
            // Ranged: the shooting loop is already playing; just land a hit on the beat.
            Invoke(nameof(DealDamage), damageDelay);
        }
        else
        {
            SetTrigger(Random.value < 0.5f ? leftPunchParam : rightPunchParam);
            Invoke(nameof(DealDamage), damageDelay);
        }
    }

    void DealDamage()
    {
        if (state == RobotState.Dead || target == null) return;

        float maxReach = useRangedAttack ? rangedAttackRange + 2f : attackRange + 1f;
        if (Flat(transform.position, target.position) > maxReach) return;

        // No damage through walls.
        if (useRangedAttack && !HasLineOfSight()) return;

        IDamageable dmg = target.GetComponentInParent<IDamageable>();
        if (dmg != null && dmg.IsAlive) dmg.TakeDamage(damage);
    }

    // True if nothing solid is between the enemy's chest and the player.
    bool HasLineOfSight()
    {
        if (target == null) return false;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPoint = target.position + Vector3.up * 1.2f;
        Vector3 dir = targetPoint - origin;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            // Clear if the first thing hit is the player (or ourselves); blocked otherwise.
            if (hit.transform == target || hit.transform.IsChildOf(target)) return true;
            if (hit.transform == transform || hit.transform.IsChildOf(transform)) return true;
            return false;
        }
        return true;
    }

    void FaceTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.Slerp(
            transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        float measured = delta.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        // While chasing, force a forward RUN so it visibly sprints at the player (it always
        // faces the player, so "forward" = toward you). Otherwise use measured movement.
        bool chasing = state == RobotState.Chase;
        Vector3 local = chasing ? Vector3.forward : transform.InverseTransformDirection(delta.normalized);
        float t = chasing ? 1f : Mathf.Clamp01(measured / Mathf.Max(chaseSpeed, 0.01f));

        SetFloat(speedParam, chasing ? chaseSpeed : measured);
        SetFloat(moveXParam, local.x * t);
        SetFloat(moveYParam, local.z * t);
        SetBool(isMovingParam, chasing || measured > 0.1f);
        SetBool(isRunningParam, chasing || measured > 0.1f);

        lastPos = transform.position;
    }

    float Flat(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // ---- IDamageable: the player can kill this robot ----
    // Needs a Collider on this object (any) so Mohammed's Bullet can hit it.
    public void TakeDamage(float amount)
    {
        if (state == RobotState.Dead) return;

        currentHealth -= amount;
        SetTrigger(damageParam);

        Debug.Log("EnemyRobotAI '" + name + "' took " + amount + " dmg. HP=" + currentHealth, this);

        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        state = RobotState.Dead;

        if (agent != null && agent.isOnNavMesh) { agent.ResetPath(); agent.isStopped = true; }
        if (agent != null) agent.enabled = false;

        SetBool(isRunningParam, false);
        SetBool(isDeadParam, true);
        CancelInvoke();

        // Stop blocking the player and stop taking further hits.
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        Debug.Log("EnemyRobotAI '" + name + "' DIED.", this);

        onDeath?.Invoke();   // wire the bomb sequence to this in the Inspector

        if (destroyDelay > 0f)
            Destroy(gameObject, destroyDelay);
    }

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
