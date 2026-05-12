using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "InvestigateTargetAction", story: "Agent moves to Target", category: "Action", id: "7286853cf702193036db9d3b4d43b237")]
public partial class InvestigateTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<Vector3> SearchPosition;
    [SerializeReference] public BlackboardVariable<float> ArrivalDistance = new(1.5f);

    NavMeshAgent nav;
    Vector3 cachedTargetPosition;

    protected override Status OnStart()
    {
        if (Agent?.Value == null || SearchPosition == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        cachedTargetPosition = SearchPosition.Value;

        nav.isStopped = false;
        nav.updateRotation = true;
        nav.stoppingDistance = Mathf.Max(0.1f, ArrivalDistance.Value);
        nav.SetDestination(cachedTargetPosition);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState == null || AIState.Value != State.Search)
            return Status.Success;

        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
            return Status.Success;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (nav != null && nav.isOnNavMesh)
            nav.ResetPath();
    }
}

