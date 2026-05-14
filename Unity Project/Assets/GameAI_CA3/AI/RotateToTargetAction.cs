using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RotateToTargetAction", story: "[Agent] rotates to face [Target]", category: "Action", id: "c67deb7a1fa128a9a181d20d151b0fe2")]
public class RotateToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new(360f);
    [SerializeReference] public BlackboardVariable<float> AngleThreshold = new(5f);
    Transform agentTransform;

    protected override Status OnStart()
    {
        if (Agent?.Value == null || Target?.Value == null)
            return Status.Failure;

        agentTransform = Agent.Value.transform;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value == null)
            return Status.Failure;

        Vector3 toTarget = Target.Value.position - agentTransform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f) //Distance check if it is within range
            return Status.Success;

        Quaternion desiredRotation = Quaternion.LookRotation(toTarget);
        agentTransform.rotation = Quaternion.RotateTowards(agentTransform.rotation, desiredRotation, RotationSpeed.Value * Time.deltaTime);

        float angle = Quaternion.Angle(agentTransform.rotation, desiredRotation);

        if (angle <= AngleThreshold.Value)
        {
            return Status.Success; // Facing target
        }

        return Status.Running;
    }
}

