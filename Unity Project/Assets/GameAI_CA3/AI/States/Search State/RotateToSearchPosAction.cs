using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RotateToSearchPos", story: "[Agent] rotates to face [searchposition]", category: "Action", id: "adb5cd156bca3d09f552686c12812bb3")]
public partial class RotateToSearchPosAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<Vector3> SearchPosition;
    [SerializeReference] public BlackboardVariable<float> RotationSpeed = new(360f);
    [SerializeReference] public BlackboardVariable<float> AngleThreshold = new(5f);

    Transform agentTransform;
    Vector3 cachedSearchPosition;

    protected override Status OnStart()
    {
        if (Agent?.Value == null || SearchPosition == null)
            return Status.Failure;

        agentTransform = Agent.Value.transform;
        cachedSearchPosition = SearchPosition.Value;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState == null || AIState.Value != State.Search)
            return Status.Success;

        if (agentTransform == null)
            return Status.Failure;

        Vector3 toTarget = cachedSearchPosition - agentTransform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return Status.Success;

        Quaternion desiredRotation = Quaternion.LookRotation(toTarget);
        agentTransform.rotation = Quaternion.RotateTowards(
            agentTransform.rotation,
            desiredRotation,
            RotationSpeed.Value * Time.deltaTime
        );

        float angle = Quaternion.Angle(agentTransform.rotation, desiredRotation);

        if (angle <= AngleThreshold.Value)
            return Status.Success;

        return Status.Running;
    }
}