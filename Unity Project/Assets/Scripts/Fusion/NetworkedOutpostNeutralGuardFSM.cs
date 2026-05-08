using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(OutpostPlayerTeam))]
[RequireComponent(typeof(Rigidbody))]
public class NetworkedOutpostNeutralGuardFSM : NetworkBehaviour
{
    private enum GuardState
    {
        Patrol,
        Chase,
        ContestPoint,
        ReturnToPatrol
    }

    [Header("References")]
    [SerializeField] private Transform capturePoint;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float loseTargetRadius = 16f;
    [SerializeField] private float playerNearPointDistance = 8f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2.2f;
    [SerializeField] private float chaseSpeed = 3.6f;
    [SerializeField] private float contestSpeed = 3.0f;
    [SerializeField] private float patrolStoppingDistance = 0.1f;
    [SerializeField] private float chaseStoppingDistance = 1.4f;
    [SerializeField] private float patrolPointReachDistance = 0.8f;
    [SerializeField] private float returnPointReachDistance = 1.2f;

    [Header("NavMesh")]
    [SerializeField] private float navMeshSampleDistance = 3f;

    [Header("Debug")]
    [SerializeField] private GuardState currentState = GuardState.Patrol;
    [SerializeField] private int currentPatrolIndex;

    private NavMeshAgent agent;
    private OutpostPlayerTeam teamIdentity;
    private Transform currentTarget;
    private Vector3 spawnPosition;
    private Vector3 currentPatrolDestination;
    private bool hasPatrolDestination;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        teamIdentity = GetComponent<OutpostPlayerTeam>();

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public override void Spawned()
    {
        teamIdentity.SetTeam(OutpostTeam.Neutral);
        spawnPosition = transform.position;

        AutoFindSceneReferences();

        if (!Object.HasStateAuthority)
        {
            if (agent != null)
                agent.enabled = false;

            return;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = patrolSpeed;
            agent.stoppingDistance = patrolStoppingDistance;
        }

        SnapToNavMeshIfNeeded();

        currentPatrolIndex = 0;
        SetState(GuardState.Patrol, true);
    }

    private void Update()
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        UpdateTarget();

        switch (currentState)
        {
            case GuardState.Patrol:
                UpdatePatrol();
                break;

            case GuardState.Chase:
                UpdateChase();
                break;

            case GuardState.ContestPoint:
                UpdateContestPoint();
                break;

            case GuardState.ReturnToPatrol:
                UpdateReturnToPatrol();
                break;
        }
    }

    private void UpdatePatrol()
    {
        if (currentTarget != null)
        {
            if (IsTargetNearCapturePoint(currentTarget))
                SetState(GuardState.ContestPoint);
            else
                SetState(GuardState.Chase);

            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            SetDestinationSafely(spawnPosition);
            return;
        }

        if (!hasPatrolDestination)
        {
            MoveToNextPatrolPoint();
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= patrolPointReachDistance)
            MoveToNextPatrolPoint();
    }

    private void UpdateChase()
    {
        if (currentTarget == null)
        {
            SetState(GuardState.ReturnToPatrol);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget > loseTargetRadius)
        {
            currentTarget = null;
            SetState(GuardState.ReturnToPatrol);
            return;
        }

        if (IsTargetNearCapturePoint(currentTarget))
        {
            SetState(GuardState.ContestPoint);
            return;
        }

        SetDestinationSafely(currentTarget.position);
    }

    private void UpdateContestPoint()
    {
        if (capturePoint == null)
        {
            SetState(GuardState.Chase);
            return;
        }

        if (currentTarget == null)
        {
            SetState(GuardState.ReturnToPatrol);
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget > loseTargetRadius)
        {
            currentTarget = null;
            SetState(GuardState.ReturnToPatrol);
            return;
        }

        if (!IsTargetNearCapturePoint(currentTarget))
        {
            SetState(GuardState.Chase);
            return;
        }

        SetDestinationSafely(capturePoint.position);
    }

    private void UpdateReturnToPatrol()
    {
        if (currentTarget != null)
        {
            if (IsTargetNearCapturePoint(currentTarget))
                SetState(GuardState.ContestPoint);
            else
                SetState(GuardState.Chase);

            return;
        }

        Vector3 returnDestination = GetClosestPatrolPointOrSpawn();
        SetDestinationSafely(returnDestination);

        if (agent.pathPending)
            return;

        if (Vector3.Distance(transform.position, returnDestination) <= returnPointReachDistance)
            SetState(GuardState.Patrol, true);
    }

    private void SetState(GuardState newState, bool force = false)
    {
        if (!force && currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case GuardState.Patrol:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = patrolStoppingDistance;
                hasPatrolDestination = false;
                MoveToNextPatrolPoint();
                break;

            case GuardState.Chase:
                agent.speed = chaseSpeed;
                agent.stoppingDistance = chaseStoppingDistance;
                hasPatrolDestination = false;
                break;

            case GuardState.ContestPoint:
                agent.speed = contestSpeed;
                agent.stoppingDistance = patrolStoppingDistance;
                hasPatrolDestination = false;
                break;

            case GuardState.ReturnToPatrol:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = patrolStoppingDistance;
                hasPatrolDestination = false;
                break;
        }
    }

    private void UpdateTarget()
    {
        if (currentTarget != null)
        {
            float distanceToCurrentTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToCurrentTarget <= loseTargetRadius)
                return;

            currentTarget = null;
        }

        currentTarget = FindClosestPlayerTarget();
    }

    private Transform FindClosestPlayerTarget()
    {
        OutpostPlayerTeam[] teams = FindObjectsByType<OutpostPlayerTeam>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (OutpostPlayerTeam team in teams)
        {
            if (team == null)
                continue;

            if (team == teamIdentity)
                continue;

            if (team.Team != OutpostTeam.Blue && team.Team != OutpostTeam.Red)
                continue;

            float distance = Vector3.Distance(transform.position, team.transform.position);

            if (distance > detectionRadius)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = team.transform;
            }
        }

        return closestTarget;
    }

    private bool IsTargetNearCapturePoint(Transform target)
    {
        if (target == null || capturePoint == null)
            return false;

        float distance = Vector3.Distance(target.position, capturePoint.position);
        return distance <= playerNearPointDistance;
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        int attempts = 0;

        while (attempts < patrolPoints.Length)
        {
            Transform point = patrolPoints[currentPatrolIndex];

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            attempts++;

            if (point == null)
                continue;

            if (TryGetNavMeshPosition(point.position, out Vector3 navPosition))
            {
                currentPatrolDestination = navPosition;
                hasPatrolDestination = true;
                agent.SetDestination(currentPatrolDestination);
                return;
            }
        }

        hasPatrolDestination = false;
    }

    private Vector3 GetClosestPatrolPointOrSpawn()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return spawnPosition;

        Transform closestPoint = null;
        float closestDistance = float.MaxValue;

        foreach (Transform point in patrolPoints)
        {
            if (point == null)
                continue;

            float distance = Vector3.Distance(transform.position, point.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint != null ? closestPoint.position : spawnPosition;
    }

    private bool SetDestinationSafely(Vector3 destination)
    {
        if (!TryGetNavMeshPosition(destination, out Vector3 navPosition))
            return false;

        agent.SetDestination(navPosition);
        return true;
    }

    private bool TryGetNavMeshPosition(Vector3 position, out Vector3 navPosition)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            navPosition = hit.position;
            return true;
        }

        navPosition = position;
        return false;
    }

private void AutoFindSceneReferences()
{
    if (capturePoint == null)
    {
        GameObject center = GameObject.Find("CapturePoint_Center");

        if (center != null)
            capturePoint = center.transform;
    }

    bool needsPatrolPointSetup =
        patrolPoints == null ||
        patrolPoints.Length == 0 ||
        HasMissingPatrolPoints();

    if (!needsPatrolPointSetup)
        return;

    patrolPoints = new Transform[4];

    patrolPoints[0] = FindTransform("PatrolPoint_01");
    patrolPoints[1] = FindTransform("PatrolPoint_02");
    patrolPoints[2] = FindTransform("PatrolPoint_03");
    patrolPoints[3] = FindTransform("PatrolPoint_04");

    Debug.Log("[NetworkedOutpostNeutralGuardFSM] Patrol points auto-assigned.");
}

private bool HasMissingPatrolPoints()
{
    if (patrolPoints == null)
        return true;

    foreach (Transform point in patrolPoints)
    {
        if (point == null)
            return true;
    }

    return false;
}
    private Transform FindTransform(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.transform : null;
    }

    private void SnapToNavMeshIfNeeded()
    {
        if (agent.isOnNavMesh)
            return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.Warp(hit.position);
        else
            Debug.LogWarning($"[NetworkedOutpostNeutralGuardFSM] {name} could not find nearby NavMesh.");
    }
}