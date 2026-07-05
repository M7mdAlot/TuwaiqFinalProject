using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FighterNPCNavMeshAI : MonoBehaviour
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

    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;

    [Header("Attack")]
    public float attackRange = 8f;
    public float attackCooldown = 1.2f;
    public float rotationSpeed = 8f;
    public int damage = 10;

    [Header("Animator Parameters")]
    public string speedParam = "Speed";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string isAimingParam = "IsAiming";
    public string shootParam = "Shoot";
    public string reloadParam = "Reload";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    private FighterState state = FighterState.Patrol;
    private Transform target;

    private float patrolTimer;
    private float nextAttackTime;

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
        }
    }

    void Update()
    {
        if (state == FighterState.Dead) return;
        if (agent == null) return;

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
        Transform foundTarget = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerCharacterIdentity identity = hit.GetComponentInParent<PlayerCharacterIdentity>();
            if (identity == null) continue;

            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X)
                continue;

            if (CanSeeTarget(identity.transform))
            {
                foundTarget = identity.transform;
                break;
            }
        }

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
            {
                state = FighterState.Patrol;
                SetBool(isAimingParam, false);
            }
        }
    }

    bool CanSeeTarget(Transform targetTransform)
    {
        Vector3 toTarget = targetTransform.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toTarget.normalized);

        if (angle > fieldOfView * 0.5f)
            return false;

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

        return true;
    }

    void HandlePatrol()
    {
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
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackRange + 2f)
        {
            agent.isStopped = false;
            state = FighterState.Chase;
            return;
        }

        agent.ResetPath();
        agent.isStopped = true;

        SetBool(isAimingParam, true);

        FaceTarget();

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        SetTrigger(shootParam);

        Vector3 origin = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * 1.5f;

        Vector3 targetPosition = target.position + Vector3.up * 1.2f;
        Vector3 direction = (targetPosition - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, visionRange, ~0, QueryTriggerInteraction.Ignore))
        {
            PlayerCharacterIdentity identity = hit.collider.GetComponentInParent<PlayerCharacterIdentity>();

            if (identity != null && identity.playerType == PlayerCharacterIdentity.PlayerType.X)
            {
                Debug.Log(name + " shot X.");

                SimpleHealth health = identity.GetComponent<SimpleHealth>();
                if (health != null)
                    health.TakeDamage(damage);
            }
        }
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

        bool isMoving = currentSpeed > 0.1f;
        bool isRunning = state == FighterState.Chase || currentSpeed > 3.2f;

        SetFloat(speedParam, currentSpeed);
        SetBool(isMovingParam, isMoving);
        SetBool(isRunningParam, isRunning);
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
        if (!animatorParams.ContainsKey(param)) return;
        animator.SetFloat(param, value);
    }

    void SetBool(string param, bool value)
    {
        if (animator == null) return;
        if (!animatorParams.ContainsKey(param)) return;
        animator.SetBool(param, value);
    }

    void SetTrigger(string param)
    {
        if (animator == null) return;
        if (!animatorParams.ContainsKey(param)) return;
        animator.SetTrigger(param);
    }
}