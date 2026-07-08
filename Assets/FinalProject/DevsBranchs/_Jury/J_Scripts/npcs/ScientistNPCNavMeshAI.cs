using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Aegis.Core;

[RequireComponent(typeof(NavMeshAgent))]
public class ScientistNPCNavMeshAI : MonoBehaviour, IDamageable
{
    public enum ScientistState
    {
        Wander,
        Flee,
        Hide,
        Dead
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Detection")]
    public float visionRange = 12f;
    public float fieldOfView = 120f;
    public LayerMask playerLayer = ~0;
    public LayerMask obstacleLayer = ~0;

    [Header("Wandering")]
    public float wanderRadius = 8f;
    public float wanderWaitTime = 2f;

    [Header("Flee")]
    public float wanderSpeed = 2.2f;
    public float fleeSpeed = 5.5f;
    public float fleeDistance = 12f;
    public float hideAfterSeconds = 4f;

    [Header("Animator Parameters")]
    public string speedParam = "Speed";
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string isMovingParam = "IsMoving";
    public string isRunningParam = "IsRunning";
    public string isScaredParam = "IsScared";
    public string isHidingParam = "IsHiding";
    public string damageParam = "Damage";
    public string isDeadParam = "IsDead";

    [Header("Health")]
    public float maxHealth = 40f;

    private ScientistState state = ScientistState.Wander;
    private Transform threat;
    private float wanderTimer;
    private float fleeTimer;
    private float currentHealth;

    private Dictionary<string, AnimatorControllerParameterType> animatorParams =
        new Dictionary<string, AnimatorControllerParameterType>();

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheAnimatorParams();

        currentHealth = maxHealth;

        if (animator != null)
            animator.applyRootMotion = false;

        if (agent != null)
        {
            agent.speed = wanderSpeed;
            agent.isStopped = false;
        }
    }

    void Update()
    {
        if (state == ScientistState.Dead) return;
        if (agent == null) return;

        FindThreat();

        switch (state)
        {
            case ScientistState.Wander:
                HandleWander();
                break;

            case ScientistState.Flee:
                HandleFlee();
                break;

            case ScientistState.Hide:
                HandleHide();
                break;
        }

        UpdateAnimator();
    }

    void FindThreat()
    {
        Transform foundThreat = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerCharacterIdentity identity = hit.GetComponentInParent<PlayerCharacterIdentity>();
            if (identity == null) continue;

            if (identity.playerType != PlayerCharacterIdentity.PlayerType.X)
                continue;

            if (CanSeeTarget(identity.transform))
            {
                foundThreat = identity.transform;
                break;
            }
        }

        if (foundThreat != null)
        {
            threat = foundThreat;

            if (state != ScientistState.Flee)
                StartFlee();
        }
    }

    bool CanSeeTarget(Transform target)
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.magnitude > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toTarget.normalized);

        if (angle > fieldOfView * 0.5f)
            return false;

        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 targetPoint = target.position + Vector3.up * 1.2f;
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

    void HandleWander()
    {
        agent.isStopped = false;
        agent.speed = wanderSpeed;

        SetBool(isScaredParam, false);
        SetBool(isHidingParam, false);

        if (!agent.hasPath || agent.remainingDistance <= 0.6f)
        {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderWaitTime)
            {
                wanderTimer = 0f;
                MoveToRandomPoint();
            }
        }
    }

    void StartFlee()
    {
        state = ScientistState.Flee;
        fleeTimer = 0f;

        agent.isStopped = false;
        agent.speed = fleeSpeed;

        SetBool(isScaredParam, true);
        SetBool(isHidingParam, false);

        MoveAwayFromThreat();
    }

    void HandleFlee()
    {
        if (threat == null)
        {
            state = ScientistState.Wander;
            return;
        }

        agent.isStopped = false;
        agent.speed = fleeSpeed;

        fleeTimer += Time.deltaTime;

        if (!agent.hasPath || agent.remainingDistance <= 1f)
            MoveAwayFromThreat();

        if (fleeTimer >= hideAfterSeconds)
            StartHide();
    }

    void StartHide()
    {
        state = ScientistState.Hide;

        agent.ResetPath();
        agent.isStopped = true;

        SetBool(isScaredParam, true);
        SetBool(isHidingParam, true);
    }

    void HandleHide()
    {
        if (threat == null)
        {
            ReturnToWander();
            return;
        }

        float distance = Vector3.Distance(transform.position, threat.position);

        if (distance > visionRange + 5f)
        {
            threat = null;
            ReturnToWander();
        }
    }

    void ReturnToWander()
    {
        state = ScientistState.Wander;

        agent.isStopped = false;
        agent.speed = wanderSpeed;

        SetBool(isScaredParam, false);
        SetBool(isHidingParam, false);
    }

    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void MoveAwayFromThreat()
    {
        if (threat == null) return;

        Vector3 awayDirection = transform.position - threat.position;
        awayDirection.y = 0f;

        if (awayDirection.sqrMagnitude < 0.01f)
            awayDirection = -transform.forward;

        awayDirection.Normalize();

        Vector3 fleeTarget = transform.position + awayDirection * fleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            MoveToRandomPoint();
    }

    void UpdateAnimator()
    {
        if (animator == null) return;

        float currentSpeed = agent.velocity.magnitude;
        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float normalizeBy = Mathf.Max(agent.speed, 0.01f);

        bool isMoving = currentSpeed > 0.1f;
        bool isRunning = state == ScientistState.Flee || currentSpeed > 3.2f;

        SetFloat(speedParam, currentSpeed);
        SetFloat(moveXParam, localVelocity.x / normalizeBy);
        SetFloat(moveYParam, localVelocity.z / normalizeBy);
        SetBool(isMovingParam, isMoving);
        SetBool(isRunningParam, isRunning);
    }

    // Bridge only: no numeric health exists yet for this NPC (it only flees on
    // hit). Real health/death-on-damage is a separate feature decision.
    public bool IsAlive => state != ScientistState.Dead;

    void IDamageable.TakeDamage(float amount)
    {
        if (state == ScientistState.Dead) return;

        currentHealth -= amount;
        SetTrigger(damageParam);

        if (currentHealth <= 0f)
            Die();          // dead: Update() bails and the agent is stopped -> frozen in place
        else
            StartFlee();    // still alive: run away

        Debug.Log(name + " scientist took " + amount + " dmg. HP=" + currentHealth, this);
    }

    public void TakeHit()
    {
        SetTrigger(damageParam);
        StartFlee();
    }

    public void Die()
    {
        state = ScientistState.Dead;

        if (agent != null)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        SetBool(isMovingParam, false);
        SetBool(isRunningParam, false);
        SetBool(isScaredParam, false);
        SetBool(isHidingParam, false);
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