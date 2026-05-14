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

    protected override Status OnUpdate()
    {
        if (AIState != null)
            AIState.Value = State.Patrol;

        return Status.Success;
    }
}
