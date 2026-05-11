using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StartSearchTimer", story: "Starts the search timer", category: "Action", id: "ab4b9b3d3a28edae6f5a8349cd2bf573")]
public partial class StartSearchTimerAction : Action
{
    [SerializeReference] public BlackboardVariable<float> SearchDuration;
    [SerializeReference] public BlackboardVariable<float> SearchTimeRemaining;

    protected override Status OnStart()
    {
        if (SearchDuration == null || SearchTimeRemaining == null)
            return Status.Failure;

        SearchTimeRemaining.Value = SearchDuration.Value;
        return Status.Success;
    }
}

