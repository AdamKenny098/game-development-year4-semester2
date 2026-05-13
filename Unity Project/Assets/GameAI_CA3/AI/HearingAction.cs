using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Hearing",
    story: "[Agent] listens for [Player]",
    category: "Action",
    id: "bc709b4ddc97364aa346fd3c1941bb73"
)]
public partial class HearingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;

    [SerializeReference] public BlackboardVariable<bool> HearsNoise;
    [SerializeReference] public BlackboardVariable<Vector3> LastHeardNoisePosition;
    [SerializeReference] public BlackboardVariable<bool> HasLastHeardNoisePosition;

    [Header("Hearing Settings")]
    public float hearingRadius = 8f;
    public float memoryTime = 3f;
    public bool requireNoiseFlag = false;

    [Tooltip("Optional. If assigned, hearing only triggers when this Blackboard bool is true.")]
    [SerializeReference] public BlackboardVariable<bool> PlayerIsMakingNoise;

    private Transform agentTransform;
    private Transform playerTransform;
    private float lastHeardTime;

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

        bool heard = false;

        if (playerTransform != null)
            heard = ComputeHearing();

        if (heard)
        {
            if (HearsNoise != null)
                HearsNoise.Value = true;

            if (LastHeardNoisePosition != null)
                LastHeardNoisePosition.Value = playerTransform.position;

            if (HasLastHeardNoisePosition != null)
                HasLastHeardNoisePosition.Value = true;

            lastHeardTime = Time.time;
            Debug.DrawLine(agentTransform.position, playerTransform.position, Color.yellow, 0.05f);

            if (LastHeardNoisePosition != null)
                TyrantOverlayReporter.Instance?.ReportLastHeardNoisePosition(LastHeardNoisePosition.Value);
        }
        else
        {
            bool memoryExpired = Time.time - lastHeardTime > memoryTime;

            if (memoryExpired && HearsNoise != null)
                HearsNoise.Value = false;
        }

        TyrantOverlayReporter.Instance?.ReportHearing(
            HearsNoise != null && HearsNoise.Value,
            HasLastHeardNoisePosition != null && HasLastHeardNoisePosition.Value
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

    private bool ComputeHearing()
    {
        if (requireNoiseFlag)
        {
            if (PlayerIsMakingNoise == null || !PlayerIsMakingNoise.Value)
                return false;
        }

        float distance = Vector3.Distance(agentTransform.position, playerTransform.position);
        return distance <= hearingRadius;
    }
}
