using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FleeGuardAction", story: "Fails if State is not Fleeing", category: "Action", id: "a20c53dd58af20452be8b6de189b537e")]
public partial class FleeGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;

    protected override Status OnUpdate()
    {
        return AIState.Value == State.Flee ? Status.Success : Status.Failure;
    }

}

