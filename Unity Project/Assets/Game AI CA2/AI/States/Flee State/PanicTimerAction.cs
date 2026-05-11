using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PanicTimerAction", story: "Controls Panic Duration", category: "Action", id: "77f6911fc90fc67981ee04a901eb7212")]
public partial class PanicTimerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> PanicTimeRemaining;
    [SerializeReference] public BlackboardVariable<bool> IsFleeing;
    [SerializeReference] public BlackboardVariable<bool> IsLowHealth;
    [SerializeReference] public BlackboardVariable<float> Health;
    [SerializeReference] public BlackboardVariable<float> LowHealthThreshold;

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null) return Status.Failure;

        Entity entity = Self.Value.GetComponent<Entity>();
        if (entity == null || entity.stats == null) return Status.Failure;

        if (entity.isDead || (Health != null && Health.Value <= 0f))
        {
            if (Health != null) Health.Value = 0f;

            IsFleeing.Value = false;
            IsLowHealth.Value = false;
            PanicTimeRemaining.Value = 0f;
            return Status.Failure;
        }

        PanicTimeRemaining.Value -= Time.deltaTime;

        if (PanicTimeRemaining.Value <= 0f)
        {
            PanicTimeRemaining.Value = 0f;

            float recoveredHealth = LowHealthThreshold != null ? LowHealthThreshold.Value + 1f : entity.stats.health + 1f;

            entity.stats.health = Mathf.CeilToInt(recoveredHealth);

            if (Health != null) Health.Value = entity.stats.health;

            IsFleeing.Value = false;
            IsLowHealth.Value = false;

            return Status.Failure;
        }

        return Status.Running;
    }
}