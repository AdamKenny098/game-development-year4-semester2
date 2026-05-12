using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SearchGuardAction", story: "Fail if not in Search state or no search target", category: "Action", id: "9eeb12e0cda8aced551c8f523dd071eb")]
public partial class SearchGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<Transform> LastKnownPlayerTransform;
    [SerializeReference] public BlackboardVariable<Transform> SoundPosition;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> IsSearching;
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
        if (AIState.Value != State.Search || !IsSearching.Value)
        {
            if (nav && nav.isOnNavMesh)
                nav.ResetPath();

            return Status.Failure;
        }

        return Status.Success;
    }
}
