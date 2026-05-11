using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol Guard", story: "Fail if not in Patrol state", category: "AI/Guards")]
public class PatrolGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> WalkSpeed;

    NavMeshAgent nav;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        nav.speed = WalkSpeed;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState.Value != State.Patrol)
        {
            nav.ResetPath();
            return Status.Failure;
        }

        return Status.Success;
    }
}
