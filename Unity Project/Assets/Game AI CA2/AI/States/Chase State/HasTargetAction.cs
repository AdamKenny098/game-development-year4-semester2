using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "HasTarget", story: "Check if target exists and is alive", category: "Action", id: "5966e9b57b696d0e0108ffa11a621cc2")]
public partial class HasTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;
    [SerializeReference] public BlackboardVariable<bool> HasTarget;

    protected override Status OnUpdate()
    {
        bool valid = false;

        if (CurrentTarget != null && CurrentTarget.Value != null)
        {
            Entity targetEntity = CurrentTarget.Value.GetComponentInParent<Entity>();

            if (targetEntity != null)
            {
                valid = !targetEntity.isDead;
            }
            else
            {
                // If the target has no Entity component, still treat it as a valid target.
                // Otherwise chase can never start for player objects without Entity.
                valid = true;
            }
        }

        if (HasTarget != null)
            HasTarget.Value = valid;

        return Status.Success;
    }
}

