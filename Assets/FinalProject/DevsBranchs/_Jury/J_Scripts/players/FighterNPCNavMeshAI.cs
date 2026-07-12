using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Aegis.Core;
using Aegis.Player;

[RequireComponent(typeof(NavMeshAgent))]
public class FighterNPCNavMeshAI : MonoBehaviour, IDamageable
{
    public enum FighterState { Patrol, Chase, Attack, Dead }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform firePoint;
    [Tooltip("Mohammed's HealthSystem on this NPC — provides the HP and the die-on-zero.")]
    public HealthSystem health;

    [Header("Detection")]
    public float visionRange = 15f;
    public float fieldOfView = 120f;
    public LayerMask playerLayer = ~0;
    public LayerMask obstacleLayer = ~0;

    [Header("Detection Options")]
    public bool useFieldOfView = false;
    public bool requireLineOfSight = false;

    [Header("Movement")]
    [Tooltip("ON = the soldier NEVER moves. It holds its exact placed spot, turns to face the " +
             "player, and shoots — a stationary guard 'waiting for you'.")]
    public bool holdPosition = false;
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;

    [Header("Attack")]
    public float attackRange = 8f;
    public float rotationSpeed = 8f;
    public int damage = 5;
    public bool damageTargetEvenIfRayMisses = true;

    [Header("Auto Fire")]
    public bool useAutoFire = true;
    public float bulletsPerSecond = 8f;
    public float continuousFireDuration = 5f;
    public string reloadStateName = "refill";
    public float reloadDuration = 5f;

    [Header("Animator Parameters - Movement")]
    public string speedParam = "Speed";
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string isAimingParam = "IsAiming";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    [Header("Animator Parameters - Weapon")]
    public string hasWeaponParam = "HasWeapon";
    public string summonWeaponParam = "SummonWeapon";
    public string shootTriggerParam = "FireSingle";
    public string shootingBoolParam = "FireAuto";
    public string reloadParam = "Refill";

    [Header("Shoot Animation")]
    public bool forceWeaponAtStart = true;
    public bool forcePlayShootAnimation = true;
    public string autoShootAnimationStateName = "multiple shots rifle";
    public string singleShootAnimationStateName = "single shot rifle";
    public float shootAnimationFadeTime = 0.03f;

    [Header("Effects")]
    public GameObject bulletParticlePrefab;

    [Header("Health (used only if there is NO HealthSystem on this NPC)")]
    [Tooltip("Own hit points. Ignored when a Mohammed HealthSystem is present — that owns the HP instead.")]
    public float maxHealth = 40f;
    private float currentHealth;

    [Header("Death")]
    [Tooltip("Optional: exact death animation STATE name to force-play (bypasses transition setup). Leave empty to rely only on the IsDead bool.")]
    public string deathAnimationStateName = "";
    public float deathAnimationFadeTime = 0.05f;
    [Tooltip("Destroy this NPC this many seconds after it dies. 0 = never (stays as a corpse).")]
    public float destroyAfterDeath = 0f;

    private FighterState state = FighterState.Patrol;
    private Transform target;

    private float patrolTimer;
    private float nextBulletTime;
    private bool autoFirePlaying;
    private float autoFireStartTime;
    private bool isReloading;
    private float reloadEndTime;
    private bool hasEnteredReloadState;

    // After death we pin the body to this exact spot/rotation every frame so no leftover
    // animation motion (or a stray root motion) can make the corpse drift or spin.
    private bool deadPinned;
    private Vector3 deathPos;
    private Quaternion deathRot;

    private Dictionary<string, AnimatorControllerParameterType> animatorParams =
        new Dictionary<string, AnimatorControllerParameterType>();

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // Find Mohammed's HealthSystem wherever it sits (same object, a child, or a parent) so
        // we always subscribe to its Died event — otherwise the soldier's HP can hit 0 with
        // nothing handling the death, and it just stands there.
        if (health == null) health = GetComponentInChildren<HealthSystem>(true);
        if (health == null) health = GetComponentInParent<HealthSystem>();

        currentHealth = maxHealth;

        CacheAnimatorParams();

        if (animator != null) animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            if (agent.isOnNavMesh) agent.isStopped = false;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    void OnEnable()
    {
        if (health != null)
        {
            health.DamageTaken += OnHealthDamaged;
            health.Died += OnHealthDied;
        }
    }

    void OnDisable()
    {
        if (health != null)
        {
            health.DamageTaken -= OnHealthDamaged;
            health.Died -= OnHealthDied;
        }
    }

    void Start()
    {
        Debug.Log("SOLDIER SETUP '" + name + "': maxHealth=" + maxHealth
            + ", hasHealthSystem=" + (health != null)
            + ", colliders=" + GetComponentsInChildren<Collider>().Length, this);

        if (forceWeaponAtStart)
        {
            SetBool(hasWeaponParam, true);
            SetTrigger(summonWeaponParam);
        }
    }

    void Update()
    {
        if (state == FighterState.Dead) return;
        if (agent == null || !agent.enabled) return;

        // Freeze in place while dialogue is playing so it isn't chaos.
        if (DialogueManager.DialogueActive)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            SetFloat(speedParam, 0f);
            SetBool(isMovingParam, false);
            SetBool(isRunningParam, false);
            SetBool(shootingBoolParam, false);
            return;
        }

        if (animator != null) animator.applyRootMotion = false;

        FindTarget();

        // Hold Position (stationary guard) OR off the baked NavMesh -> can't chase, but the
        // soldier can still stand, face the player, and SHOOT (shooting is a raycast).
        if (holdPosition || !agent.isOnNavMesh)
        {
            if (agent.isOnNavMesh) agent.isStopped = true; // pin it exactly where it was placed

            if (target != null)
            {
                FaceTarget();
                SetBool(isAimingParam, true);
                SetBool(hasWeaponParam, true);

                if (isReloading)
                {
                    if (IsReloadAnimationDone() || Time.time >= reloadEndTime) isReloading = false;
                }
                else if (useAutoFire) AutoFire();
                else SingleFire();
            }
            else
            {
                StopShootingAnimation();
            }

            SetFloat(speedParam, 0f);
            SetBool(isMovingParam, false);
            SetBool(isRunningParam, false);
            return;
        }

        switch (state)
        {
            case FighterState.Patrol: HandlePatrol(); break;
            case FighterState.Chase: HandleChase(); break;
            case FighterState.Attack: HandleAttack(); break;
        }

        UpdateAnimator();
    }

    void FindTarget()
    {
        Transform foundTarget = FindClosestXTarget();

        if (foundTarget != null)
        {
            target = foundTarget;
            float distance = Vector3.Distance(transform.position, target.position);
            state = distance <= attackRange ? FighterState.Attack : FighterState.Chase;
        }
        else
        {
            target = null;
            if (state != FighterState.Patrol) state = FighterState.Patrol;
            StopShootingAnimation();
        }
    }

    Transform FindClosestXTarget()
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerCharacterIdentity identity = hit.GetComponentInParent<PlayerCharacterIdentity>();
            if (identity == null) continue;
            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X) continue;
            if (!CanSeeTarget(identity.transform)) continue;

            float distance = Vector3.Distance(transform.position, identity.transform.position);
            if (distance < closestDistance) { closestDistance = distance; closestTarget = identity.transform; }
        }

        if (closestTarget != null) return closestTarget;

        PlayerCharacterIdentity[] allPlayers =
            FindObjectsByType<PlayerCharacterIdentity>(FindObjectsSortMode.None);

        foreach (PlayerCharacterIdentity identity in allPlayers)
        {
            if (identity == null) continue;
            if (!identity.gameObject.activeInHierarchy) continue;
            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X) continue;

            float distance = Vector3.Distance(transform.position, identity.transform.position);
            if (distance > visionRange) continue;
            if (!CanSeeTarget(identity.transform)) continue;

            if (distance < closestDistance) { closestDistance = distance; closestTarget = identity.transform; }
        }

        return closestTarget;
    }

    bool CanSeeTarget(Transform targetTransform)
    {
        Vector3 toTarget = targetTransform.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.magnitude > visionRange) return false;

        if (useFieldOfView)
        {
            float angle = Vector3.Angle(transform.forward, toTarget.normalized);
            if (angle > fieldOfView * 0.5f) return false;
        }

        if (requireLineOfSight)
        {
            Vector3 eye = transform.position + Vector3.up * 1.5f;
            Vector3 targetPoint = targetTransform.position + Vector3.up * 1.2f;
            Vector3 dir = targetPoint - eye;

            if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, visionRange, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                PlayerCharacterIdentity identity = hit.collider.GetComponentInParent<PlayerCharacterIdentity>();
                if (identity == null) return false;
                return identity.playerType == PlayerCharacterIdentity.PlayerType.X;
            }
        }

        return true;
    }

    void HandlePatrol()
    {
        StopShootingAnimation();
        agent.isStopped = false;
        agent.speed = patrolSpeed;
        SetBool(isAimingParam, false);

        if (!agent.hasPath || agent.remainingDistance <= 0.6f)
        {
            patrolTimer += Time.deltaTime;
            if (patrolTimer >= patrolWaitTime) { patrolTimer = 0f; MoveToRandomPoint(); }
        }
    }

    void HandleChase()
    {
        StopShootingAnimation();

        if (target == null) { state = FighterState.Patrol; return; }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        SetBool(isAimingParam, false);

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange) { state = FighterState.Attack; return; }

        agent.SetDestination(target.position);
    }

    void HandleAttack()
    {
        if (target == null) { state = FighterState.Patrol; StopShootingAnimation(); return; }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange + 2f)
        {
            agent.isStopped = false;
            state = FighterState.Chase;
            StopShootingAnimation();
            return;
        }

        agent.ResetPath();
        agent.isStopped = true;

        SetBool(isAimingParam, true);
        SetBool(hasWeaponParam, true);

        FaceTarget();

        if (isReloading)
        {
            if (IsReloadAnimationDone() || Time.time >= reloadEndTime) isReloading = false;
            else return;
        }

        if (useAutoFire) AutoFire();
        else SingleFire();
    }

    void AutoFire()
    {
        if (!autoFirePlaying) autoFireStartTime = Time.time;

        if (Time.time - autoFireStartTime >= continuousFireDuration) { StartReload(); return; }

        StartAutoShootAnimation();

        float bulletDelay = 1f / Mathf.Max(1f, bulletsPerSecond);
        if (Time.time >= nextBulletTime) { nextBulletTime = Time.time + bulletDelay; ShootBullet(); }
    }

    void StartReload()
    {
        StopShootingAnimation();
        isReloading = true;
        hasEnteredReloadState = false;
        reloadEndTime = Time.time + reloadDuration;
        SetTrigger(reloadParam);
        Debug.Log(name + " reloading.");
    }

    bool IsReloadAnimationDone()
    {
        if (animator == null || string.IsNullOrEmpty(reloadStateName)) return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!hasEnteredReloadState)
        {
            if (stateInfo.IsName(reloadStateName)) hasEnteredReloadState = true;
            return false;
        }

        return !stateInfo.IsName(reloadStateName);
    }

    void SingleFire()
    {
        if (Time.time >= nextBulletTime)
        {
            nextBulletTime = Time.time + 0.8f;
            SetBool(hasWeaponParam, true);
            SetTrigger(shootTriggerParam);

            if (animator != null && forcePlayShootAnimation && !string.IsNullOrEmpty(singleShootAnimationStateName))
                animator.CrossFadeInFixedTime(singleShootAnimationStateName, shootAnimationFadeTime, 0);

            ShootBullet();
        }
    }

    void StartAutoShootAnimation()
    {
        if (autoFirePlaying) return;
        autoFirePlaying = true;

        SetBool(hasWeaponParam, true);
        SetBool(shootingBoolParam, true);

        if (animator != null && forcePlayShootAnimation && !string.IsNullOrEmpty(autoShootAnimationStateName))
            animator.CrossFadeInFixedTime(autoShootAnimationStateName, shootAnimationFadeTime, 0);
    }

    void StopShootingAnimation()
    {
        isReloading = false;
        autoFirePlaying = false;
        SetBool(shootingBoolParam, false);
    }

    void ShootBullet()
    {
        if (target == null) return;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1.5f;
        Vector3 targetPosition = target.position + Vector3.up * 1.2f;
        Vector3 direction = (targetPosition - origin).normalized;

        if (bulletParticlePrefab != null)
            Instantiate(bulletParticlePrefab, origin, Quaternion.LookRotation(direction));

        bool damagedPlayer = false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, visionRange, ~0, QueryTriggerInteraction.Ignore))
        {
            PlayerCharacterIdentity identity = hit.collider.GetComponentInParent<PlayerCharacterIdentity>();
            if (identity != null && identity.playerType == PlayerCharacterIdentity.PlayerType.X)
            {
                ApplyDamage(identity.transform);
                damagedPlayer = true;
            }
        }

        if (!damagedPlayer && damageTargetEvenIfRayMisses)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange + 1.5f) ApplyDamage(target);
        }
    }

    void ApplyDamage(Transform targetTransform)
    {
        if (targetTransform == null) return;
        targetTransform.SendMessageUpwards("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
    }

    void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float currentSpeed = agent.velocity.magnitude;
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float normalizeBy = Mathf.Max(agent.speed, 0.01f);

        bool isMoving = currentSpeed > 0.1f;
        bool isRunning = state == FighterState.Chase || currentSpeed > 3.2f;

        SetFloat(speedParam, currentSpeed);
        SetFloat(moveXParam, localVelocity.x / normalizeBy);
        SetFloat(moveYParam, localVelocity.z / normalizeBy);
        SetBool(isMovingParam, isMoving);
        SetBool(isRunningParam, isRunning);
    }

    public bool IsAlive => state != FighterState.Dead;

    // Bullets call this (IDamageable). Pure internal HP — the exact same reliable pattern the
    // scientist uses. We do NOT defer to a HealthSystem here (that was the flaky part). If a
    // HealthSystem is ALSO present and the bullet hits IT, OnHealthDied still routes to Die().
    void IDamageable.TakeDamage(float amount)
    {
        if (state == FighterState.Dead) return;

        currentHealth -= amount;
        Debug.Log("SOLDIER '" + name + "' hit for " + amount + ". HP now " + currentHealth, this);

        if (currentHealth <= 0f) Die();
        else TakeHit();
    }

    private void OnHealthDamaged(float amount)
    {
        if (state == FighterState.Dead) return;
        if (health != null && !health.IsAlive) return; // fatal hit -> skip the flinch, go straight to death
        TakeHit();
    }

    private void OnHealthDied()
    {
        Debug.Log("SOLDIER '" + name + "' HealthSystem reported DEATH -> dying.", this);
        Die();
    }

    public void TakeHit()
    {
        SetTrigger(damageParam);
    }

    public void Die()
    {
        if (state == FighterState.Dead) return; // never die twice

        state = FighterState.Dead;
        Debug.Log("SOLDIER '" + name + "' DIED -> playing death.", this);

        // Remember exactly where/how it died so LateUpdate can pin it there forever.
        deathPos = transform.position;
        deathRot = transform.rotation;
        deadPinned = true;

        // Hard stop so the corpse can't keep sliding or shooting.
        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            agent.enabled = false; // detach completely — nothing moves the corpse now
        }
        if (animator != null) animator.applyRootMotion = false;

        SetFloat(speedParam, 0f);
        SetFloat(moveXParam, 0f);
        SetFloat(moveYParam, 0f);
        SetBool(isMovingParam, false);
        SetBool(isRunningParam, false);
        SetBool(isAimingParam, false);
        SetBool(shootingBoolParam, false);
        SetBool(isDeadParam, true);

        // Force-play the death state (works even if the IsDead transition isn't wired up).
        string deathState = string.IsNullOrEmpty(deathAnimationStateName) ? "death" : deathAnimationStateName;
        if (animator != null) animator.CrossFadeInFixedTime(deathState, deathAnimationFadeTime, 0);

        // Corpse ALWAYS stays on the ground — never auto-destroyed (per your request: bodies remain).
    }

    void LateUpdate()
    {
        // Runs AFTER the Animator writes the transform, so it beats any motion baked into the
        // death clip: the dead soldier is frozen exactly where it fell — no drift, no spin.
        if (!deadPinned) return;

        transform.position = deathPos;
        transform.rotation = deathRot;

        // Never let it leave the death state (some controllers transition death -> idle/walk,
        // which made the "dead" body get up and move). If it left, snap back to the death pose.
        if (animator != null && !animator.IsInTransition(0))
        {
            string deathState = string.IsNullOrEmpty(deathAnimationStateName) ? "death" : deathAnimationStateName;
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName(deathState))
                animator.Play(deathState, 0, 1f);
        }
    }

    void CacheAnimatorParams()
    {
        if (animator == null) return;
        animatorParams.Clear();
        foreach (AnimatorControllerParameter param in animator.parameters)
            animatorParams[param.name] = param.type;
    }

    void SetFloat(string param, float value)
    {
        if (animator == null || string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Float) return;
        animator.SetFloat(param, value);
    }

    void SetBool(string param, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Bool) return;
        animator.SetBool(param, value);
    }

    void SetTrigger(string param)
    {
        if (animator == null || string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Trigger) return;
        animator.SetTrigger(param);
    }
}