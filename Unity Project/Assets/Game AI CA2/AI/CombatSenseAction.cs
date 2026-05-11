using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CombatSense", story: "Detects whether or not you can attack", category: "Action", id: "9c3377e3b41b9552654210636a944312")]
public partial class CombatSenseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;
    [SerializeReference] public BlackboardVariable<int> AbilitySlot = new(0);

    [SerializeReference] public BlackboardVariable<Transform> CombatTarget;
    [SerializeReference] public BlackboardVariable<bool> IsAbilityReady;
    [SerializeReference] public BlackboardVariable<bool> CanAttack;
    [SerializeReference] public BlackboardVariable<bool> IsInRange;

    [Header("Fallbacks")]
    public float fallbackRange = 2f;

    protected override Status OnUpdate()
    {
        if (Self == null)
        {
            SafeFail();
            return Status.Running;
        }

        if (Self.Value == null)
        {
            SafeFail();
            return Status.Running;
        }


        Entity selfEntity = Self.Value.GetComponent<Entity>();
        if (selfEntity == null)
        {
            SafeFail();
            return Status.Running;
        }

        if (selfEntity.stats == null)
        {
            SafeFail();
            return Status.Running;
        }

        if (selfEntity.isDead)
        {
            SafeFail();
            return Status.Running;
        }

        Transform chosenTarget = PlayerTransform != null ? PlayerTransform.Value : null;
        if (PlayerTransform == null)
        {
            SafeFail();
            return Status.Running;
        }

        if (chosenTarget == null)
        {
            SafeFail();
            return Status.Running;
        }

        Entity targetEntity = chosenTarget.GetComponentInParent<Entity>();
        if (targetEntity == null)
        {
            SafeFail();
            return Status.Running;
        }

        if (targetEntity.isDead)
        {
            SafeFail();
            return Status.Running;
        }

        if (CombatTarget != null)
        {
            CombatTarget.Value = chosenTarget;
        }

        AbilityManager abilityManager = Self.Value.GetComponent<AbilityManager>();
        if (abilityManager == null)
        {
            SafeFail();
            return Status.Running;
        }

        int slot = AbilitySlot != null ? AbilitySlot.Value : 0;

        AbilityData ability = abilityManager.GetAbility(slot);
        if (ability == null)
        {
            SafeFail();
            return Status.Running;
        }

        float range = ability.range > 0f ? ability.range : fallbackRange;

        Vector3 selfPos = Self.Value.transform.position;
        Vector3 targetPos = chosenTarget.position;
        float distance = Vector3.Distance(selfPos, targetPos);

        bool inRange = distance <= range;
        bool ready = abilityManager.IsReady(slot);
        bool finalCanAttack = inRange && ready;

        if (IsAbilityReady != null)
        {
            IsAbilityReady.Value = ready;
        }

        if (CanAttack != null)
        {
            CanAttack.Value = finalCanAttack;
        }

        OverlayBT.Instance?.SetCombat(
            isInRange: inRange,
            canAttack: finalCanAttack
        );

        OverlayBT.Instance?.SetTarget(
            chosenTarget,
            hasTarget: chosenTarget != null
        );

        if (IsInRange != null)
        {
            IsInRange.Value = inRange;
        }
        return Status.Running;
    }

    void SafeFail()
    {
        if (CombatTarget != null)
            CombatTarget.Value = null;

        if (IsAbilityReady != null)
            IsAbilityReady.Value = false;

        if (CanAttack != null)
            CanAttack.Value = false;

        if (IsInRange != null)
            IsInRange.Value = false;

        OverlayBT.Instance?.SetCombat(
            isInRange: false,
            canAttack: false
        );

        OverlayBT.Instance?.SetTarget(
            null,
            hasTarget: false
        );
    }
}