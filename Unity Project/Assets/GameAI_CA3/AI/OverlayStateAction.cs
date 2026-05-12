using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "OverlayState", story: "Sets [State] in Overlay", category: "Action", id: "0adc56e4e56032105f19021dd23e27ff")]
public partial class OverlayStateAction : Action
{
    [SerializeReference] public BlackboardVariable<string> Branch;
    [SerializeReference] public BlackboardVariable<string> Task;
    [SerializeReference] public BlackboardVariable<string> StateLabel;

    protected override Status OnStart()
    {
        OverlayBT.Instance?.SetBehaviour(
            Branch != null ? Branch.Value : null,
            Task != null ? Task.Value : null,
            StateLabel != null ? StateLabel.Value : null
        );

        return Status.Success;
    }
}

