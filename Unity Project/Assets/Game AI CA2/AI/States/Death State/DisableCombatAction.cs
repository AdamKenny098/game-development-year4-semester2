using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DisableCombat", story: "Disable Combat Scripts on Agent", category: "Action", id: "e5d3090a143334e86b33bb3f4b611593")]
public partial class DisableCombatAction : Action
{
    protected override Status OnStart()
    {
        AIAbilityManager aiAbilityManager = GameObject.GetComponent<AIAbilityManager>();
        if (aiAbilityManager != null)
            aiAbilityManager.enabled = false;

        AbilityManager abilityManager = GameObject.GetComponent<AbilityManager>();
        if (abilityManager != null)
            abilityManager.enabled = false;

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

