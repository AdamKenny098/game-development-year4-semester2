using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SearchesForPlayerAction", story: "Agent looks around for Player", category: "Action", id: "658db9a513f33c1d6c335d81e98d2599")]
public partial class SearchesForPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<State> AIState;

    public float rotationSpeed = 120f;
    public float minTurnAngle = 60f;
    public float maxTurnAngle = 180f;
    Transform agentTransform;
    float targetYaw;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        agentTransform = Agent.Value.transform;

        float delta = UnityEngine.Random.Range(minTurnAngle, maxTurnAngle);
        if (UnityEngine.Random.value > 0.5f)
            delta = -delta;

        targetYaw = agentTransform.eulerAngles.y + delta;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState.Value != State.Search)
            return Status.Success;

        float currentY = agentTransform.eulerAngles.y;
        float newY = Mathf.MoveTowardsAngle(
            currentY,
            targetYaw,
            rotationSpeed * Time.deltaTime
        );

        agentTransform.rotation = Quaternion.Euler(0f, newY, 0f);
        return Status.Running;
    }
}

