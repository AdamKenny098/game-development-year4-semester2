using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(DemoHealth))]
public class SimpleWardenAI : MonoBehaviour
{
    public enum WardenState
    {
        Dormant,
        Patrol,
        InvestigateNoise,
        Chase,
        Attack,
        SearchLastKnownPosition,
        Flee,
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private DemoHealth playerHealth;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform[] searchPoints;
    [SerializeField] private Transform fleePoint;

    [Header("State")]
    [SerializeField] private WardenState currentState = WardenState.Patrol;
    [SerializeField] private bool activateOnStart = true;

    [Header("Vision")]
    [SerializeField] private float visionRange = 16f;
    [SerializeField] private float visionAngle = 85f;
    [SerializeField] private float eyeHeight = 1.6f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;

    [Header("Hearing")]
    [SerializeField] private float hearingRange = 15f;
    [SerializeField] private float hearingMemorySeconds = 3f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.4f;
    [SerializeField] private float investigateSpeed = 3.2f;
    [SerializeField] private float chaseSpeed = 4.2f;
    [SerializeField] private float fleeSpeed = 4.5f;
    [SerializeField] private float waypointReachDistance = 0.8f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 4f;
    [SerializeField] private float searchTurnSpeed = 180f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.2f;

    [Header("Recovery")]
    [SerializeField] private float stuckCheckInterval = 1f;
    [SerializeField] private float stuckDistanceThreshold = 0.15f;
    [SerializeField] private float stuckTimeRequired = 2.5f;

    private NavMeshAgent agent;
    private DemoHealth health;
    private int patrolIndex;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastHeardNoisePosition;
    private bool hasLastKnownPlayerPosition;
    private bool hasLastHeardNoisePosition;
    private float searchTimer;
    private float nextAttackTime;
    private float stuckCheckTimer;
    private float stuckTimer;
    private Vector3 lastStuckCheckPosition;

    public WardenState CurrentState => currentState;
    public bool CanSeePlayer { get; private set; }
    public bool CanHearNoise { get; private set; }
    public bool IsStuck { get; private set; }
    public bool HasLastKnownPlayerPosition => hasLastKnownPlayerPosition;
    public Vector3 LastKnownPlayerPosition => lastKnownPlayerPosition;
    public Vector3 LastHeardNoisePosition => lastHeardNoisePosition;
    public Vector3 CurrentDestination => agent != null && agent.hasPath ? agent.destination : transform.position;
    public float RemainingDistance => agent != null && agent.hasPath ? agent.remainingDistance : 0f;
    public string CurrentGoal { get; private set; } = "None";
    public string BlackboardSummary => $"CanSeePlayer={CanSeePlayer}, CanHearNoise={CanHearNoise}, HasLastKnown={hasLastKnownPlayerPosition}, LowHealth={(health != null && health.IsLowHealth)}";

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<DemoHealth>();
        lastStuckCheckPosition = transform.position;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player != null && playerHealth == null)
            playerHealth = player.GetComponent<DemoHealth>();

        if (!activateOnStart)
            ChangeState(WardenState.Dormant);
        else
            ChangeState(WardenState.Patrol);
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            ChangeState(WardenState.Dead);
            return;
        }

        if (currentState == WardenState.Dormant || currentState == WardenState.Dead)
            return;

        UpdatePerception();
        UpdateStuckDetection();
        DecideInterrupts();
        TickState();
    }

    private void UpdatePerception()
    {
        CanSeePlayer = CheckVision();
        CanHearNoise = CheckHearing();

        if (CanSeePlayer && player != null)
        {
            lastKnownPlayerPosition = player.position;
            hasLastKnownPlayerPosition = true;
        }

        if (CanHearNoise)
        {
            lastHeardNoisePosition = SimpleDemoPlayerController.LastNoisePosition;
            hasLastHeardNoisePosition = true;
        }
    }

    private bool CheckVision()
    {
        if (player == null || playerHealth != null && playerHealth.IsDead)
            return false;

        Vector3 eyePosition = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPosition = player.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = targetPosition - eyePosition;

        if (toPlayer.magnitude > visionRange)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        if (angle > visionAngle * 0.5f)
            return false;

        if (Physics.Raycast(eyePosition, toPlayer.normalized, out RaycastHit hit, visionRange, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }

    private bool CheckHearing()
    {
        float noiseAge = Time.time - SimpleDemoPlayerController.LastNoiseTime;

        if (noiseAge > hearingMemorySeconds)
            return false;

        float distance = Vector3.Distance(transform.position, SimpleDemoPlayerController.LastNoisePosition);
        float effectiveRange = Mathf.Min(hearingRange, SimpleDemoPlayerController.LastNoiseRadius);

        return distance <= effectiveRange;
    }

    private void DecideInterrupts()
    {
        if (health != null && health.IsLowHealth && currentState != WardenState.Flee)
        {
            ChangeState(WardenState.Flee);
            return;
        }

        if (CanSeePlayer)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= attackRange)
                ChangeState(WardenState.Attack);
            else
                ChangeState(WardenState.Chase);

            return;
        }

        if (CanHearNoise && currentState != WardenState.InvestigateNoise)
        {
            ChangeState(WardenState.InvestigateNoise);
            return;
        }
    }

    private void TickState()
    {
        switch (currentState)
        {
            case WardenState.Patrol:
                TickPatrol();
                break;
            case WardenState.InvestigateNoise:
                TickInvestigateNoise();
                break;
            case WardenState.Chase:
                TickChase();
                break;
            case WardenState.Attack:
                TickAttack();
                break;
            case WardenState.SearchLastKnownPosition:
                TickSearchLastKnownPosition();
                break;
            case WardenState.Flee:
                TickFlee();
                break;
        }
    }

    private void TickPatrol()
    {
        CurrentGoal = "Patrolling station route";

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.speed = patrolSpeed;

        if (!agent.hasPath || agent.remainingDistance <= waypointReachDistance)
        {
            Transform nextPoint = patrolPoints[patrolIndex];
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;

            if (nextPoint != null)
                agent.SetDestination(nextPoint.position);
        }
    }

    private void TickInvestigateNoise()
    {
        CurrentGoal = "Investigating last heard noise";
        agent.speed = investigateSpeed;

        if (hasLastHeardNoisePosition && (!agent.hasPath || agent.destination != lastHeardNoisePosition))
            agent.SetDestination(lastHeardNoisePosition);

        if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
        {
            lastKnownPlayerPosition = lastHeardNoisePosition;
            hasLastKnownPlayerPosition = true;
            ChangeState(WardenState.SearchLastKnownPosition);
        }
    }

    private void TickChase()
    {
        CurrentGoal = "Chasing visible player";
        agent.speed = chaseSpeed;

        if (player != null)
            agent.SetDestination(player.position);

        if (!CanSeePlayer && hasLastKnownPlayerPosition)
            ChangeState(WardenState.SearchLastKnownPosition);
    }

    private void TickAttack()
    {
        CurrentGoal = "Attacking player";

        if (player == null)
        {
            ChangeState(WardenState.Patrol);
            return;
        }

        agent.ResetPath();

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 8f);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange * 1.2f)
        {
            ChangeState(WardenState.Chase);
            return;
        }

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (playerHealth != null)
                playerHealth.Damage(attackDamage);

            Debug.Log($"[Warden] Attacked player for {attackDamage}");
        }
    }

    private void TickSearchLastKnownPosition()
    {
        CurrentGoal = "Searching last known position";
        agent.speed = investigateSpeed;

        if (hasLastKnownPlayerPosition && !agent.hasPath)
            agent.SetDestination(lastKnownPlayerPosition);

        if (!agent.pathPending && agent.remainingDistance <= waypointReachDistance)
        {
            searchTimer += Time.deltaTime;
            transform.Rotate(Vector3.up * searchTurnSpeed * Time.deltaTime);

            if (searchTimer >= searchDuration)
            {
                searchTimer = 0f;
                hasLastKnownPlayerPosition = false;
                hasLastHeardNoisePosition = false;
                ChangeState(WardenState.Patrol);
            }
        }
    }

    private void TickFlee()
    {
        CurrentGoal = "Fleeing due to low health";
        agent.speed = fleeSpeed;

        if (fleePoint != null)
        {
            if (!agent.hasPath || Vector3.Distance(agent.destination, fleePoint.position) > 0.2f)
                agent.SetDestination(fleePoint.position);
        }
        else if (player != null)
        {
            Vector3 away = (transform.position - player.position).normalized;
            Vector3 target = transform.position + away * 8f;
            agent.SetDestination(target);
        }
    }

    private void ChangeState(WardenState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (newState == WardenState.SearchLastKnownPosition)
            searchTimer = 0f;

        if (newState == WardenState.Dead && agent != null)
        {
            agent.ResetPath();
            agent.enabled = false;
            CurrentGoal = "Dead / disabled";
        }

        Debug.Log($"[Warden] State changed to {newState}");
    }

    private void UpdateStuckDetection()
    {
        stuckCheckTimer -= Time.deltaTime;

        if (stuckCheckTimer > 0f)
            return;

        stuckCheckTimer = stuckCheckInterval;

        bool shouldMove = agent.enabled && agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        float moved = Vector3.Distance(transform.position, lastStuckCheckPosition);

        if (shouldMove && moved < stuckDistanceThreshold)
            stuckTimer += stuckCheckInterval;
        else
            stuckTimer = 0f;

        IsStuck = stuckTimer >= stuckTimeRequired;

        if (IsStuck)
        {
            agent.ResetPath();
            stuckTimer = 0f;
            IsStuck = false;

            if (hasLastKnownPlayerPosition)
                agent.SetDestination(lastKnownPlayerPosition);
            else
                ChangeState(WardenState.Patrol);
        }

        lastStuckCheckPosition = transform.position;
    }

    public void AssignPlayer(Transform value)
    {
        player = value;
        playerHealth = value != null ? value.GetComponent<DemoHealth>() : null;
    }

    public void AssignPatrolPoints(Transform[] points)
    {
        patrolPoints = points;
    }

    public void AssignSearchPoints(Transform[] points)
    {
        searchPoints = points;
    }

    public void AssignFleePoint(Transform point)
    {
        fleePoint = point;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hearingRange);

        if (hasLastKnownPlayerPosition)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.35f);
        }
    }
}
