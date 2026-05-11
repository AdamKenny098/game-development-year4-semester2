using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ClearSearchMemory", story: "Clears references used in searching", category: "Action", id: "0a84aa7191339495dc00dad638b3523e")]
public partial class ClearSearchMemoryAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HearsNoise;
    [SerializeReference] public BlackboardVariable<bool> IsSearching;
    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;

    protected override Status OnUpdate()
    {
        if (CanSeePlayer != null && CanSeePlayer.Value)
            return Status.Success;

        if (HasLastKnownPlayerPosition != null)
            HasLastKnownPlayerPosition.Value = false;

        if (HearsNoise != null)
            HearsNoise.Value = false;

        if (IsSearching != null)
            IsSearching.Value = false;

        return Status.Success;
    }
}

