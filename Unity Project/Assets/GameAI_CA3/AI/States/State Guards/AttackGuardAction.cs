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
    [SerializeReference] public BlackboardVariable<bool> CanAttack;
    [SerializeReference] public BlackboardVariable<bool> IsInRange;
    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;

    protected override Status OnUpdate()
    {
        bool hasTarget = CurrentTarget != null && CurrentTarget.Value != null;
        bool inRange = IsInRange != null && IsInRange.Value;
        bool canAttack = CanAttack != null && CanAttack.Value;

        if (!hasTarget || !inRange || !canAttack)
            return Status.Failure;

        if (AIState != null)
            AIState.Value = State.Attack;

        return Status.Success;
    }
}