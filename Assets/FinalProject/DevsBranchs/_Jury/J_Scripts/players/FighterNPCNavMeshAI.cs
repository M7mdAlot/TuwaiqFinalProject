using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Aegis.Core;

[RequireComponent(typeof(NavMeshAgent))]
public class FighterNPCNavMeshAI : MonoBehaviour, IDamageable
{
    public enum FighterState
    {
        Patrol,
        Chase,
        Attack,
        Dead
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform firePoint;

    [Header("Detection")]
    public float visionRange = 15f;
    public float fieldOfView = 120f;
    public LayerMask playerLayer = ~0;
    public LayerMask obstacleLayer = ~0;

    [Header("Detection Options")]
    public bool useFieldOfView = false;
    public bool requireLineOfSight = false;

    [Header("Movement")]
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

    private FighterState state = FighterState.Patrol;
    private Transform target;

    private float patrolTimer;
    private float nextBulletTime;
    private bool autoFirePlaying;
    private float autoFireStartTime;
    private bool isReloading;
    private float reloadEndTime;
    private bool hasEnteredReloadState;

    private Dictionary<string, AnimatorControllerParameterType> animatorParams =
        new Dictionary<string, AnimatorControllerParameterType>();

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheAnimatorParams();

        if (animator != null)
            animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.speed = patrolSpeed;
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.updatePosition = true;
        }
    }

    void Start()
    {
        if (forceWeaponAtStart)
        {
            SetBool(hasWeaponParam, true);
            SetTrigger(summonWeaponParam);
        }
    }

    void Update()
    {
        if (state == FighterState.Dead) return;
        if (agent == null) return;

        if (animator != null)
            animator.applyRootMotion = false;

        FindTarget();

        switch (state)
        {
            case FighterState.Patrol:
                HandlePatrol();
                break;

            case FighterState.Chase:
                HandleChase();
                break;

            case FighterState.Attack:
                HandleAttack();
                break;
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

            if (distance <= attackRange)
                state = FighterState.Attack;
            else
                state = FighterState.Chase;
        }
        else
        {
            target = null;

            if (state != FighterState.Patrol)
                state = FighterState.Patrol;

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

            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X)
                continue;

            if (!CanSeeTarget(identity.transform))
                continue;

            float distance = Vector3.Distance(transform.position, identity.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = identity.transform;
            }
        }

        if (closestTarget != null)
            return closestTarget;

        PlayerCharacterIdentity[] allPlayers =
            FindObjectsByType<PlayerCharacterIdentity>(FindObjectsSortMode.None);

        foreach (PlayerCharacterIdentity identity in allPlayers)
        {
            if (identity == null) continue;
            if (!identity.gameObject.activeInHierarchy) continue;

            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X)
                continue;

            float distance = Vector3.Distance(transform.position, identity.transform.position);

            if (distance > visionRange)
                continue;

            if (!CanSeeTarget(identity.transform))
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = identity.transform;
            }
        }

        return closestTarget;
    }

    bool CanSeeTarget(Transform targetTransform)
    {
        Vector3 toTarget = targetTransform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > visionRange)
            return false;

        if (useFieldOfView)
        {
            float angle = Vector3.Angle(transform.forward, toTarget.normalized);

            if (angle > fieldOfView * 0.5f)
                return false;
        }

        if (requireLineOfSight)
        {
            Vector3 eye = transform.position + Vector3.up * 1.5f;
            Vector3 targetPoint = targetTransform.position + Vector3.up * 1.2f;
            Vector3 dir = targetPoint - eye;

            if (Physics.Raycast(eye, dir.normalized, out RaycastHit hit, visionRange, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                PlayerCharacterIdentity identity = hit.collider.GetComponentInParent<PlayerCharacterIdentity>();

                if (identity == null)
                    return false;

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

            if (patrolTimer >= patrolWaitTime)
            {
                patrolTimer = 0f;
                MoveToRandomPoint();
            }
        }
    }

    void HandleChase()
    {
        StopShootingAnimation();

        if (target == null)
        {
            state = FighterState.Patrol;
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        SetBool(isAimingParam, false);

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            state = FighterState.Attack;
            return;
        }

        agent.SetDestination(target.position);
    }

    void HandleAttack()
    {
        if (target == null)
        {
            state = FighterState.Patrol;
            StopShootingAnimation();
            return;
        }

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
            if (IsReloadAnimationDone() || Time.time >= reloadEndTime)
                isReloading = false;
            else
                return;
        }

        if (useAutoFire)
            AutoFire();
        else
            SingleFire();
    }

    void AutoFire()
    {
        if (!autoFirePlaying)
            autoFireStartTime = Time.time;

        if (Time.time - autoFireStartTime >= continuousFireDuration)
        {
            StartReload();
            return;
        }

        StartAutoShootAnimation();

        float bulletDelay = 1f / Mathf.Max(1f, bulletsPerSecond);

        if (Time.time >= nextBulletTime)
        {
            nextBulletTime = Time.time + bulletDelay;
            ShootBullet();
        }
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
        if (animator == null || string.IsNullOrEmpty(reloadStateName))
            return true;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!hasEnteredReloadState)
        {
            // Wait for the trigger to actually take effect before watching for it to end,
            // otherwise we'd read the previous (still-active) state and think it's "done" instantly.
            if (stateInfo.IsName(reloadStateName))
                hasEnteredReloadState = true;

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
        if (autoFirePlaying)
            return;

        autoFirePlaying = true;

        SetBool(hasWeaponParam, true);
        SetBool(shootingBoolParam, true);

        if (animator != null && forcePlayShootAnimation && !string.IsNullOrEmpty(autoShootAnimationStateName))
            animator.CrossFadeInFixedTime(autoShootAnimationStateName, shootAnimationFadeTime, 0);

        Debug.Log(name + " auto fire animation started: " + autoShootAnimationStateName);
    }

    void StopShootingAnimation()
    {
        isReloading = false;

        if (!autoFirePlaying)
        {
            SetBool(shootingBoolParam, false);
            return;
        }

        autoFirePlaying = false;
        SetBool(shootingBoolParam, false);
    }

    void ShootBullet()
    {
        if (target == null) return;

        Vector3 origin = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 targetPosition = target.position + Vector3.up * 1.2f;
        Vector3 direction = (targetPosition - origin).normalized;

        bool damagedPlayer = false;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, visionRange, ~0, QueryTriggerInteraction.Ignore))
        {
            PlayerCharacterIdentity identity = hit.collider.GetComponentInParent<PlayerCharacterIdentity>();

            if (identity != null && identity.playerType == PlayerCharacterIdentity.PlayerType.X)
            {
                ApplyDamage(identity.transform);
                damagedPlayer = true;

                Debug.Log(name + " bullet hit X.");
            }
        }

        if (!damagedPlayer && damageTargetEvenIfRayMisses)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance <= attackRange + 1.5f)
            {
                ApplyDamage(target);
                Debug.Log(name + " bullet damaged X directly.");
            }
        }
    }

    void ApplyDamage(Transform targetTransform)
    {
        if (targetTransform == null) return;

        targetTransform.SendMessageUpwards(
            "TakeDamage",
            damage,
            SendMessageOptions.DontRequireReceiver
        );
    }

    void FaceTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
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

    // Bridge only: no numeric health exists yet for this NPC (it only plays the
    // hit-reaction). Real health/death-on-damage is a separate feature decision.
    public bool IsAlive => state != FighterState.Dead;

    void IDamageable.TakeDamage(float amount)
    {
        TakeHit();
    }

    public void TakeHit()
    {
        SetTrigger(damageParam);
    }

    public void Die()
    {
        state = FighterState.Dead;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        SetBool(isMovingParam, false);
        SetBool(isRunningParam, false);
        SetBool(isAimingParam, false);
        SetBool(shootingBoolParam, false);
        SetBool(isDeadParam, true);
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
        if (animator == null) return;
        if (string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Float) return;

        animator.SetFloat(param, value);
    }

    void SetBool(string param, bool value)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Bool) return;

        animator.SetBool(param, value);
    }

    void SetTrigger(string param)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(param)) return;
        if (!animatorParams.ContainsKey(param)) return;
        if (animatorParams[param] != AnimatorControllerParameterType.Trigger) return;

        animator.SetTrigger(param);
    }
}