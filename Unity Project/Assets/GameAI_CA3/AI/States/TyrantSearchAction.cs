using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TyrantSearch", story: "Tyrant searches", category: "Action", id: "206858b67abf1eb771256fcc81850ed7")]
public partial class TyrantSearchAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<State> AIState;

    [SerializeReference] public BlackboardVariable<Vector3> SearchTargetPosition;

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;
    [SerializeReference] public BlackboardVariable<bool> HearsNoise;

    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastHeardNoisePosition;

    [Header("Search Settings")]
    [SerializeField] private float searchSpeed = 2.25f;
    [SerializeField] private float stoppingDistance = 0.75f;
    [SerializeField] private float searchDuration = 3f;
    [SerializeField] private float turnSpeed = 90f;

    private NavMeshAgent agent;
    private Animator animator;

    private bool hasStartedSearch;
    private bool hasReachedSearchPoint;
    private float searchTimer;

    protected override Status OnStart()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        agent = Self.Value.GetComponent<NavMeshAgent>();
        animator = Self.Value.GetComponent<Animator>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        BeginSearch();

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        if (agent == null)
            agent = Self.Value.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        if (CanSeePlayer != null && CanSeePlayer.Value)
        {
            hasStartedSearch = false;
            hasReachedSearchPoint = false;
            return Status.Failure;
        }

        if (!hasStartedSearch)
            BeginSearch();

        if (!hasReachedSearchPoint)
        {
            if (HasReachedDestination())
            {
                hasReachedSearchPoint = true;
                searchTimer = searchDuration;
                agent.isStopped = true;

                if (animator != null)
                    animator.SetFloat("Speed", 0f);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(SearchTargetPosition.Value);

                if (animator != null)
                    animator.SetFloat("Speed", agent.velocity.magnitude);
            }

            if (AIState != null)
            {
                AIState.Value = State.Search;
            }

            return Status.Success;
        }

        searchTimer -= Time.deltaTime;

        Self.Value.transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

        if (searchTimer <= 0f)
        {
            FinishSearch();
        }

        if (AIState != null)
        {
            AIState.Value = State.Search;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
        if (animator != null)
            animator.SetFloat("Speed", 0f);
    }

    private void BeginSearch()
    {
        hasStartedSearch = true;
        hasReachedSearchPoint = false;
        searchTimer = searchDuration;

        if (AIState != null)
            AIState.Value = State.Search;

        agent.isStopped = false;
        agent.speed = searchSpeed;
        agent.stoppingDistance = stoppingDistance;

        Vector3 target = SearchTargetPosition != null
            ? SearchTargetPosition.Value
            : Self.Value.transform.position;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
        else
            agent.SetDestination(target);

        if (animator != null)
            animator.SetFloat("Speed", 0.5f);
    }

    private void FinishSearch()
    {
        hasStartedSearch = false;
        hasReachedSearchPoint = false;

        if (HasLastKnownPlayerPosition != null)
            HasLastKnownPlayerPosition.Value = false;

        if (HasLastHeardNoisePosition != null)
            HasLastHeardNoisePosition.Value = false;

        if (HearsNoise != null)
            HearsNoise.Value = false;

        if (AIState != null)
            AIState.Value = State.Patrol;

        agent.isStopped = false;
        agent.ResetPath();
    }

    private bool HasReachedDestination()
    {
        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance)
            return false;

        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }
}