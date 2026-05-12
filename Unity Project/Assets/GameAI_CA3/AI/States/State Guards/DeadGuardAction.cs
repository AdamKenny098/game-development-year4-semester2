using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DeadGuardAction", story: "Fail is State is not Dead", category: "Action", id: "84736544bc600a653fcd8e445cffb3a0")]
public class DeadGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    NavMeshAgent nav;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState.Value != State.Dead)
        {
            nav.ResetPath();
            return Status.Failure;
        }

        return Status.Success;
    }
}

