using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseAction", story: "[Agent] chases Player", category: "Action", id: "564161ffcb50c0b38dd1b5f2e0886b2a")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> AttackRange = new(2.0f);
    [SerializeReference] public BlackboardVariable<bool> ChaseLocked;
    [SerializeReference] public BlackboardVariable<State> AIState;

    public float stopBuffer = 0.25f;
    public float repathThreshold = 0.2f;

    NavMeshAgent nav;
    Vector3 lastDestination = Vector3.positiveInfinity;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        ApplyStoppingDistance();
        nav.isStopped = false;
        lastDestination = Vector3.positiveInfinity;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        if (ChaseLocked != null && ChaseLocked.Value)
        {
            StopAgent();
            return Status.Failure;
        }

        if (Target?.Value == null)
        {
            StopAgent();
            return Status.Failure;
        }

        if (AIState == null || AIState.Value != State.Chase)
        {
            StopAgent();
            return Status.Failure;
        }

        ApplyStoppingDistance();

        Vector3 agentPos = Agent.Value.transform.position;
        Vector3 targetPos = Target.Value.position;

        float range = GetAttackRange();
        float distance = Vector3.Distance(agentPos, targetPos);

        if (distance <= range)
        {
            StopAgent();
            return Status.Success;
        }

        nav.isStopped = false;

        if (Vector3.Distance(lastDestination, targetPos) > repathThreshold || !nav.hasPath)
        {
            nav.SetDestination(targetPos);
            lastDestination = targetPos;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        StopAgent();
    }

    float GetAttackRange()
    {
        if (AttackRange != null && AttackRange.Value > 0f)
            return AttackRange.Value;

        return 2f;
    }

    void ApplyStoppingDistance()
    {
        if (nav == null)
            return;

        float range = GetAttackRange();
        nav.stoppingDistance = Mathf.Max(0.05f, range - stopBuffer);
    }

    void StopAgent()
    {
        if (nav == null || !nav.isOnNavMesh)
            return;

        nav.isStopped = true;
        nav.ResetPath();
    }
}