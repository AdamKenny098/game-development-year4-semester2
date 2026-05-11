using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryUseAbility", story: "Try use ability on target", category: "Action", id: "c9a9073caa7f43cfb59c4bab974d1f03")]
public partial class TryUseAbilityAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> CombatTarget;
    protected override Status OnUpdate()
    {

        if (Self == null || Self.Value == null) return Status.Failure;

        GameObject selfObject = Self.Value;
        Transform selfTransform = selfObject.transform;

        if (CombatTarget == null || CombatTarget.Value == null) return Status.Failure;

        Transform targetTransform = CombatTarget.Value;
        float directDistance = Vector3.Distance(selfTransform.position, targetTransform.position);

        var ai = selfObject.GetComponentInChildren<AIAbilityManager>();
        if (ai == null || ai.abilityManager == null) return Status.Failure;

        var targetEntity = targetTransform.GetComponentInParent<Entity>();
        if (targetEntity == null || targetEntity.stats == null || targetEntity.isDead) return Status.Failure;

        int slot = 0;
        AbilityData ability = ai.abilityManager.GetAbility(slot);

        if (ability == null)
        {
            return Status.Failure;
        }

        bool slotReady = ai.abilityManager.IsReady(slot);
        float slotCooldownRemaining = ai.abilityManager.GetCooldownRemaining(slot);

        Vector3 hitPoint = targetTransform.position;

        bool inRange = ability.range <= 0f || directDistance <= ability.range;
        bool canPayCosts = ai.owner != null && CombatSystem.Instance != null && CombatSystem.Instance.CanPayCosts(ai.owner, ability);
        bool cast = ai.TryAttackNow(targetEntity, hitPoint);

        if (cast)
        {
            if (targetEntity.stats != null)
            {
                bool hpDropped = targetEntity.stats.health < targetEntity.stats.maxHealth;
            }

            return Status.Success;
        }
        return Status.Failure;
    }
}