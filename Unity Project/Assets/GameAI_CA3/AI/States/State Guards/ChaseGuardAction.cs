using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseGuardAction", story: "Fail if not in Chase state or target lost", category: "Action", id: "f19fcd09b25efa762b095631af62b4b8")]
public class ChaseGuardAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;

    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;
    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;

    protected override Status OnUpdate()
    {
        if (CanSeePlayer == null || !CanSeePlayer.Value)
            return Status.Failure;

        Transform target = null;

        if (CurrentTarget != null && CurrentTarget.Value != null)
            target = CurrentTarget.Value;
        else if (PlayerTransform != null && PlayerTransform.Value != null)
            target = PlayerTransform.Value;
        else if (Player != null && Player.Value != null)
            target = Player.Value.transform;

        if (target == null)
            return Status.Failure;

        if (CurrentTarget != null)
            CurrentTarget.Value = target;

        if (PlayerTransform != null)
            PlayerTransform.Value = target;

        if (Player != null)
            Player.Value = target.gameObject;

        if (AIState != null)
            AIState.Value = State.Chase;

        return Status.Success;
    }
}