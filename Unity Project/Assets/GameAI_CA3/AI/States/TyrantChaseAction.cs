using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TyrantChase", story: "tyrant chases", category: "Action", id: "d6fb9bf06a94568a1ff4ddee28f68745")]
public partial class TyrantChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    [SerializeReference] public BlackboardVariable<State> AIState;

    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;

    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 3.25f;
    [SerializeField] private float stoppingDistance = 1.6f;
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float turnSpeed = 8f;

    private NavMeshAgent agent;
    private Animator animator;
    private float nextRepathTime;

    protected override Status OnStart()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        agent = Self.Value.GetComponent<NavMeshAgent>();
        animator = Self.Value.GetComponent<Animator>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = stoppingDistance;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        if (AIState != null && AIState.Value != State.Chase)
            return Status.Failure;

        if (agent == null)
            agent = Self.Value.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        Transform target = GetTarget();

        if (target == null)
            return Status.Failure;

        if (LastKnownPlayerPosition != null)
            LastKnownPlayerPosition.Value = target.position;

        if (HasLastKnownPlayerPosition != null)
            HasLastKnownPlayerPosition.Value = true;

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathInterval;

            agent.isStopped = false;
            agent.speed = chaseSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.SetDestination(target.position);
        }

        FaceTarget(target.position);

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (animator != null)
            animator.SetFloat("Speed", 0f);

        // Do not ResetPath here.
        // Brain/Switch may move to Search or Attack next.
    }

    private Transform GetTarget()
    {
        if (CurrentTarget != null && CurrentTarget.Value != null)
            return CurrentTarget.Value;

        if (PlayerTransform != null && PlayerTransform.Value != null)
            return PlayerTransform.Value;

        if (Player != null && Player.Value != null)
            return Player.Value.transform;

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
        {
            if (Player != null)
                Player.Value = foundPlayer;

            if (PlayerTransform != null)
                PlayerTransform.Value = foundPlayer.transform;

            if (CurrentTarget != null)
                CurrentTarget.Value = foundPlayer.transform;

            return foundPlayer.transform;
        }

        return null;
    }

    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - Self.Value.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Self.Value.transform.rotation = Quaternion.Slerp(
            Self.Value.transform.rotation,
            targetRotation,
            Time.deltaTime * turnSpeed
        );
    }
}