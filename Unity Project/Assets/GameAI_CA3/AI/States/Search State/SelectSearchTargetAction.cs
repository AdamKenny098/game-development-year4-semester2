using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SelectSearchTargetAction", story: "Selects what the AI should search for", category: "Action", id: "d490182665038579646e826e3b6660d4")]
public partial class SelectSearchTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;

    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;

    [SerializeReference] public BlackboardVariable<Vector3> LastHeardNoisePosition;
    [SerializeReference] public BlackboardVariable<bool> HasHeardNoise;

    [SerializeReference] public BlackboardVariable<Vector3> SearchPosition;
    [SerializeReference] public BlackboardVariable<SearchSourceType> SearchSource;

    protected override Status OnUpdate()
    {
        if (AIState == null || AIState.Value != State.Search)
            return Status.Failure;

        if (HasLastKnownPlayerPosition != null && HasLastKnownPlayerPosition.Value)
        {
            SearchPosition.Value = LastKnownPlayerPosition.Value;
            SearchSource.Value = SearchSourceType.Player;
            return Status.Success;
        }

        if (HasHeardNoise != null && HasHeardNoise.Value)
        {
            SearchPosition.Value = LastHeardNoisePosition.Value;
            SearchSource.Value = SearchSourceType.Noise;
            return Status.Success;
        }

        SearchSource.Value = SearchSourceType.None;
        return Status.Failure;
    }
}

