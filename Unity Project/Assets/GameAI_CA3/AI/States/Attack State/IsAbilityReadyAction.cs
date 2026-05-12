using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IsAbilityReady", story: "Is Agent Ability Ready", category: "Action", id: "84e40a51b619349ea68254ee8e26e152")]
public class IsAbilityReadyAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<int> Slot;
    [SerializeReference] public BlackboardVariable<bool> IsReady;

    protected override Status OnUpdate()
    {
        if (Self.Value == null) return Status.Failure;

        AbilityManager abilities = Self.Value.GetComponentInChildren<AbilityManager>();
        if (abilities == null) return Status.Failure;
        int slot = Slot?.Value ?? 0;
        bool ready = abilities.IsReady(slot);
        if (IsReady != null) IsReady.Value = ready;

        return ready ? Status.Success : Status.Failure;
    }
}

