using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Tyrant Attack",
    story: "Tyrant Attacks",
    category: "Action",
    id: "a2e256bab5bb081e2bd2750a3f8d9fd5"
)]
public partial class TyrantAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<State> AIState;

    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;

    [SerializeReference] public BlackboardVariable<bool> CanAttack;
    [SerializeReference] public BlackboardVariable<bool> IsInRange;
    [SerializeReference] public BlackboardVariable<bool> IsAttacking;

    [Header("Attack Settings")]
    [SerializeField] private float attackDuration = 1.1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float faceTurnSpeed = 12f;
    [SerializeField] private string attackTriggerName = "Attack";

    private NavMeshAgent agent;
    private Animator animator;

    private float attackEndTime;
    private float nextAttackTime;
    private bool attackStarted;

    protected override Status OnStart()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        agent = Self.Value.GetComponent<NavMeshAgent>();
        animator = Self.Value.GetComponent<Animator>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        if (agent == null)
            agent = Self.Value.GetComponent<NavMeshAgent>();

        if (agent == null || !agent.isOnNavMesh)
            return Status.Failure;

        Transform target = GetTarget();

        if (target == null)
        {
            StopAttack();
            return Status.Failure;
        }

        bool canAttack = CanAttack != null && CanAttack.Value;
        bool inRange = IsInRange != null && IsInRange.Value;

        if (!canAttack || !inRange)
        {
            StopAttack();
            return Status.Failure;
        }

        if (AIState != null)
            AIState.Value = State.Attack;

        if (CurrentTarget != null)
            CurrentTarget.Value = target;

        agent.isStopped = true;
        agent.ResetPath();

        FaceTarget(target.position);

        if (!attackStarted && Time.time >= nextAttackTime)
        {
            StartAttack();
        }

        if (attackStarted && Time.time >= attackEndTime)
        {
            EndAttackCycle();
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        StopAttack();
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

    private void StartAttack()
    {
        attackStarted = true;
        attackEndTime = Time.time + attackDuration;

        if (IsAttacking != null)
            IsAttacking.Value = true;

        if (animator != null && !string.IsNullOrWhiteSpace(attackTriggerName))
            animator.SetTrigger(attackTriggerName);
    }

    private void EndAttackCycle()
    {
        attackStarted = false;
        nextAttackTime = Time.time + attackCooldown;

        if (IsAttacking != null)
            IsAttacking.Value = false;
    }

    private void StopAttack()
    {
        attackStarted = false;

        if (IsAttacking != null)
            IsAttacking.Value = false;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
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
            Time.deltaTime * faceTurnSpeed
        );
    }
}