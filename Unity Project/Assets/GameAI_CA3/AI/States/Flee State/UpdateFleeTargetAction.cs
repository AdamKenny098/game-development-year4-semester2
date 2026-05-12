using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateFleeTargetAction", story: "Continuosly sets flee target away from player", category: "Action", id: "ea8f9098d4c4290968a4eac3ed2a7e4b")]
public partial class UpdateFleeTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Threat;
    [SerializeReference] public BlackboardVariable<Transform> FleeTarget;

    [SerializeReference] public BlackboardVariable<bool> IsFleeing;
    [SerializeReference] public BlackboardVariable<float> PanicTimeRemaining;

    public float fleeDistance = 8f;
    public float sampleRadius = 3f;

    [Header("Panic")]
    public float directionHoldTime = 0.6f;
    public float sidewaysJitter = 0.9f;
    public float backBias = 1.2f;
    public float randomAngleDegrees = 35f;

    float nextRepickTime;
    Vector3 chosenDir;

    protected override Status OnStart()
    {
        nextRepickTime = 0f;
        chosenDir = Vector3.zero;

        if (FleeTarget != null && FleeTarget.Value == null)
        {
            GameObject temp = new GameObject("FleeTarget");
            FleeTarget.Value = temp.transform;
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent?.Value == null) return Status.Failure;
        if (Threat?.Value == null) return Status.Failure;
        if (FleeTarget == null) return Status.Failure;
        if (IsFleeing == null) return Status.Failure;
        if (PanicTimeRemaining == null) return Status.Failure;
        if (!IsFleeing.Value || PanicTimeRemaining.Value <= 0f) return Status.Failure;

        Vector3 agentPos = Agent.Value.transform.position;
        Vector3 threatPos = Threat.Value.position;
        Vector3 away = agentPos - threatPos;
        away.y = 0f;

        if (away.sqrMagnitude < 0.0001f)
            away = Agent.Value.transform.forward;
        else
            away.Normalize();

        if (Time.time >= nextRepickTime || chosenDir.sqrMagnitude < 0.0001f)
        {
            nextRepickTime = Time.time + Mathf.Max(0.05f, directionHoldTime);

            float angle = UnityEngine.Random.Range(-randomAngleDegrees, randomAngleDegrees);
            Vector3 coneDir = Quaternion.AngleAxis(angle, Vector3.up) * away;

            Vector3 right = new Vector3(-away.z, 0f, away.x); // perpendicular on XZ
            float side = UnityEngine.Random.Range(-sidewaysJitter, sidewaysJitter);

            Vector3 raw = (coneDir * backBias) + (right * side);
            raw.y = 0f;

            if (raw.sqrMagnitude < 0.0001f) raw = away;
            chosenDir = raw.normalized;
        }

        Vector3 desiredPos = agentPos + chosenDir * fleeDistance;

        if (!NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            nextRepickTime = 0f;
            return Status.Running;
        }

        if (FleeTarget.Value == null)
        {
            GameObject temp = new GameObject("FleeTarget");
            FleeTarget.Value = temp.transform;
        }

        FleeTarget.Value.position = hit.position;
        return Status.Running;
    }
}