using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Low Health", story: "Triggers Fleeing when health is low", category: "Action", id: "e7a1cc0dffe4c7fc76834cbbc3cba006")]
public partial class LowHealthAction : Action
{
    [SerializeReference] public BlackboardVariable<float> MaxHealth;
    [SerializeReference] public BlackboardVariable<float> Health;
    [SerializeReference] public BlackboardVariable<float> LowHealthThreshold;

    [SerializeReference] public BlackboardVariable<bool> IsLowHealth;
    [SerializeReference] public BlackboardVariable<Transform> LastThreat;
    [SerializeReference] public BlackboardVariable<Transform> Threat;

    protected override Status OnStart()
    {
        if (Health == null || MaxHealth == null || LowHealthThreshold == null || IsLowHealth == null)
            return Status.Failure;

        if (LowHealthThreshold.Value <= 0f)
            LowHealthThreshold.Value = MaxHealth.Value * 0.15f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float max = MaxHealth.Value;
        float hp = Health.Value;

        if (LowHealthThreshold.Value <= 0f)
            LowHealthThreshold.Value = max * 0.15f;

        // Dead = not fleeing, no threat
        if (hp <= 0f)
        {
            IsLowHealth.Value = false;

            if (Threat != null)
                Threat.Value = null;

            return Status.Running;
        }

        float threshold = LowHealthThreshold.Value;
        bool low = hp <= threshold;

        IsLowHealth.Value = low;

        if (Threat != null)
        {
            if (low && LastThreat != null && LastThreat.Value != null)
                Threat.Value = LastThreat.Value;
            else
                Threat.Value = null;
        }

        OverlayBT.Instance?.SetCombat(
            isLowHealth: low,
            isFleeing: low
        );

        OverlayBT.Instance?.SetTarget(
            Threat != null ? Threat.Value : null,
            hasTarget: Threat != null && Threat.Value != null
        );

        return Status.Running;
    }
}