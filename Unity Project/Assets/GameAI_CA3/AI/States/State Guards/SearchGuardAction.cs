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

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;
    [SerializeReference] public BlackboardVariable<bool> HearsNoise;

    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPlayerPosition;

    [SerializeReference] public BlackboardVariable<bool> HasLastHeardNoisePosition;
    [SerializeReference] public BlackboardVariable<Vector3> LastHeardNoisePosition;

    [SerializeReference] public BlackboardVariable<Vector3> SearchTargetPosition;

    protected override Status OnUpdate()
    {
        if (CanSeePlayer != null && CanSeePlayer.Value)
            return Status.Failure;

        bool hasHeardNoise = HearsNoise != null && HearsNoise.Value;
        bool hasNoisePosition = HasLastHeardNoisePosition != null && HasLastHeardNoisePosition.Value;

        if (hasHeardNoise && hasNoisePosition)
        {
            if (SearchTargetPosition != null && LastHeardNoisePosition != null)
                SearchTargetPosition.Value = LastHeardNoisePosition.Value;

            if (AIState != null)
                AIState.Value = State.Search;

            return Status.Success;
        }

        bool hasLastKnownPlayer = HasLastKnownPlayerPosition != null && HasLastKnownPlayerPosition.Value;

        if (hasLastKnownPlayer)
        {
            if (SearchTargetPosition != null && LastKnownPlayerPosition != null)
                SearchTargetPosition.Value = LastKnownPlayerPosition.Value;

            if (AIState != null)
                AIState.Value = State.Search;

            return Status.Success;
        }

        return Status.Failure;
    }
}
