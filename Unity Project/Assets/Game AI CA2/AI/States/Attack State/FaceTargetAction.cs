using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FaceTarget", story: "rotate towards target", category: "Action", id: "619cacbef6ec103a65868bd2bfb6f97b")]
public partial class FaceTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Transform> CombatTarget;
    [SerializeReference] public BlackboardVariable<float> TurnSpeedDegPerSec;

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null) return Status.Failure;
        if (CombatTarget == null || CombatTarget.Value == null) return Status.Failure;

        float turn = (TurnSpeedDegPerSec != null && TurnSpeedDegPerSec.Value > 0f) ? TurnSpeedDegPerSec.Value : 720f;

        Transform transform = Self.Value.transform;
        Vector3 dir = CombatTarget.Value.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return Status.Success;

        Quaternion desired = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, turn * Time.deltaTime);

        return Status.Success;
    }
}

