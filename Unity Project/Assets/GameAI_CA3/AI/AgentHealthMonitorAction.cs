using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AgentHealthMonitor", story: "Monitors Agent Health", category: "Action", id: "700ac6b7936a9c7b579981f68a25c25b")]
public partial class AgentHealthMonitorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Health;
    [SerializeReference] public BlackboardVariable<float> MaxHealth;
    [SerializeReference] public BlackboardVariable<bool> IsDead;
    [SerializeReference] public BlackboardVariable<Transform> LastThreat;

    Entity entity;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
            return Status.Failure;

        entity = Agent.Value.GetComponent<Entity>();
        if (entity == null)
            return Status.Failure;

        // Init max/health once
        if (entity.stats != null)
        {
            if (MaxHealth != null) MaxHealth.Value = entity.stats.maxHealth;
            if (Health != null) Health.Value = entity.stats.health;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (entity == null || entity.stats == null)
            return Status.Failure;

        if (MaxHealth != null) MaxHealth.Value = entity.stats.maxHealth;
        if (Health != null) Health.Value = entity.stats.health;

        if (IsDead != null) IsDead.Value = entity.isDead;

        if (LastThreat != null)
        {
            if (entity.lastAttacker != null)
                LastThreat.Value = entity.lastAttacker.transform;
            else if (entity.lastDamageSource != null)
                LastThreat.Value = entity.lastDamageSource.transform;
            else
                LastThreat.Value = null;
        }

        OverlayBT.Instance?.SetHealth(
            current: Health != null ? Health.Value : (float?)null,
            max: MaxHealth != null ? MaxHealth.Value : (float?)null
        );

        OverlayBT.Instance?.SetTarget(
            LastThreat != null ? LastThreat.Value : null,
            hasTarget: LastThreat != null && LastThreat.Value != null
        );
        return Status.Running;
    }

    protected override void OnEnd()
    {
        entity = null;
    }
}

