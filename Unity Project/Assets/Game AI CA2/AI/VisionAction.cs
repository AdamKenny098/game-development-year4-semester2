using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Vision", story: "Calculating if Agent Can See [Player]", category: "Action", id: "7082f207e7e538659c549d5d6bbbe5f0")]
public partial class VisionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;
    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;
    [SerializeReference] public BlackboardVariable<Transform> LastKnownPlayerTransform;

    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;

    [Header("Vision Settings")]
    public float viewDistance = 12f;
    public float viewAngle = 90f;
    public float eyeHeight = 1.6f;
    public float targetHeight = 1.2f;

    Transform eyeOrigin;
    Transform player;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        eyeOrigin = Agent.Value.transform;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        if (eyeOrigin == null)
            eyeOrigin = Agent.Value.transform;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        if (player == null)
        {
            if (CanSeePlayer != null) CanSeePlayer.Value = false;
            if (Player != null) Player.Value = null;
            if (PlayerTransform != null) PlayerTransform.Value = null;
            if (CurrentTarget != null) CurrentTarget.Value = null;
            return Status.Running;
        }

        bool canSee = ComputeVision();

        OverlayBT.Instance?.SetPerception(
            canSeePlayer: canSee,
            knowsLastPlayerPos: HasLastKnownPlayerPosition != null && HasLastKnownPlayerPosition.Value
        );

        OverlayBT.Instance?.SetTarget(
            CurrentTarget != null ? CurrentTarget.Value : null,
            hasTarget: CurrentTarget != null && CurrentTarget.Value != null
        );

        OverlayBT.Instance?.SetTracking(
            lastKnownPlayerPos: LastKnownPlayerPosition != null ? LastKnownPlayerPosition.Value : (Vector3?)null
        );

        if (CanSeePlayer != null)
            CanSeePlayer.Value = canSee;

        if (canSee)
        {
            if (Player != null)
                Player.Value = player.gameObject;

            if (PlayerTransform != null)
                PlayerTransform.Value = player;

            if (CurrentTarget != null)
                CurrentTarget.Value = player;

            if (LastKnownPlayerTransform != null)
                LastKnownPlayerTransform.Value = player;

            if (LastKnownPlayerPosition != null)
                LastKnownPlayerPosition.Value = player.position;

            if (HasLastKnownPlayerPosition != null)
                HasLastKnownPlayerPosition.Value = true;
        }
        else
        {
            if (Player != null)
                Player.Value = null;

            if (PlayerTransform != null)
                PlayerTransform.Value = null;

            if (CurrentTarget != null)
                CurrentTarget.Value = null;

            // Do NOT clear last known memory here.
            // Search branch needs it.
        }

        return Status.Running;
    }

    bool ComputeVision()
    {
        Vector3 origin = eyeOrigin.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * targetHeight;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > viewDistance)
            return false;

        if (Vector3.Angle(eyeOrigin.forward, dir) > viewAngle * 0.5f)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(origin, dir.normalized, dist, ~0, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit h in hits)
        {
            if (!h.transform)
                continue;

            if (Agent?.Value != null && h.transform.root == Agent.Value.transform.root)
                continue;

            return h.transform.CompareTag("Player") || h.transform.root.CompareTag("Player");
        }

        return false;
    }
}