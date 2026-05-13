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
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;

    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;
    [SerializeReference] public BlackboardVariable<Transform> CurrentTarget;
    [SerializeReference] public BlackboardVariable<Transform> LastKnownPlayerTransform;

    [SerializeReference] public BlackboardVariable<Vector3> LastKnownPlayerPosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastKnownPlayerPosition;

    [Header("Vision Settings")]
    public float viewDistance = 14f;
    public float viewAngle = 95f;
    public float eyeHeight = 1.6f;
    public float targetHeight = 1.2f;
    public LayerMask visionMask = ~0;

    private Transform agentTransform;
    private Transform playerTransform;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
            return Status.Failure;

        agentTransform = Agent.Value.transform;
        ResolvePlayer();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent == null || Agent.Value == null)
            return Status.Failure;

        if (agentTransform == null)
            agentTransform = Agent.Value.transform;

        ResolvePlayer();

        if (playerTransform == null)
        {
            SetVisible(false);
            return Status.Running;
        }

        bool canSee = ComputeVision();

        SetVisible(canSee);

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

        return Status.Running;
    }

    private void ResolvePlayer()
    {
        if (Player != null && Player.Value != null)
        {
            playerTransform = Player.Value.transform;
            return;
        }

        if (PlayerTransform != null && PlayerTransform.Value != null)
        {
            playerTransform = PlayerTransform.Value;

            if (Player != null)
                Player.Value = playerTransform.gameObject;

            return;
        }

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer == null)
            return;

        playerTransform = foundPlayer.transform;

        if (Player != null)
            Player.Value = foundPlayer;

        if (PlayerTransform != null)
            PlayerTransform.Value = playerTransform;
    }

    private bool ComputeVision()
    {
        Vector3 origin = agentTransform.position + Vector3.up * eyeHeight;
        Vector3 target = playerTransform.position + Vector3.up * targetHeight;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        if (distance > viewDistance)
        {
            Debug.DrawLine(origin, target, Color.gray, 0.05f);
            return false;
        }

        float angle = Vector3.Angle(agentTransform.forward, direction);

        if (angle > viewAngle * 0.5f)
        {
            Debug.DrawLine(origin, target, Color.gray, 0.05f);
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction.normalized,
            distance,
            visionMask,
            QueryTriggerInteraction.Ignore
        );

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == null)
                continue;

            if (hit.transform.root == agentTransform.root)
                continue;

            if (hit.transform.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
            {
                Debug.DrawLine(origin, hit.point, Color.green, 0.05f);
                return true;
            }

            Debug.DrawLine(origin, hit.point, Color.red, 0.05f);
            return false;
        }

        Debug.DrawLine(origin, target, Color.green, 0.05f);
        return true;
    }

    private void SetVisible(bool canSee)
    {
        if (CanSeePlayer != null)
            CanSeePlayer.Value = canSee;

        if (canSee)
        {
            if (Player != null && playerTransform != null)
                Player.Value = playerTransform.gameObject;

            if (PlayerTransform != null)
                PlayerTransform.Value = playerTransform;

            if (CurrentTarget != null)
                CurrentTarget.Value = playerTransform;

            if (LastKnownPlayerTransform != null)
                LastKnownPlayerTransform.Value = playerTransform;

            if (LastKnownPlayerPosition != null)
                LastKnownPlayerPosition.Value = playerTransform.position;

            if (HasLastKnownPlayerPosition != null)
                HasLastKnownPlayerPosition.Value = true;
        }
        else
        {
            if (CurrentTarget != null)
                CurrentTarget.Value = null;

            if (PlayerTransform != null)
                PlayerTransform.Value = null;
        }
    }
}