using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackGuardAction", story: "Fail is [state] is not attack", category: "Action", id: "5769033444156e34777e5475b03f5b85")]
public partial class AttackGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;

    protected override Status OnUpdate()
    {
        return AIState != null && AIState.Value == State.Attack ? Status.Success : Status.Failure;
    }
}

