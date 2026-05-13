using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "CombatSense",
    story: "[Self] checks if the player is in attack range",
    category: "Action",
    id: "9c3377e3b41b9552654210636a944312"
)]
public partial class CombatSenseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> PlayerTransform;

    [SerializeReference] public BlackboardVariable<bool> CanSeePlayer;
    [SerializeReference] public BlackboardVariable<bool> IsInRange;
    [SerializeReference] public BlackboardVariable<bool> CanAttack;

    [Header("Tyrant Range Settings")]
    public float attackRange = 3.5f;
    public bool requireVisionToAttack = false;

    private Transform selfTransform;
    private Transform playerTransform;

    protected override Status OnStart()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        selfTransform = Self.Value.transform;
        ResolvePlayer();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
        {
            SetCombatValues(false, false);
            return Status.Running;
        }

        if (selfTransform == null)
            selfTransform = Self.Value.transform;

        ResolvePlayer();

        if (playerTransform == null)
        {
            SetCombatValues(false, false);
            return Status.Running;
        }

        float distance = Vector3.Distance(selfTransform.position, playerTransform.position);
        bool inRange = distance <= attackRange;

        bool hasVision = CanSeePlayer == null || CanSeePlayer.Value;
        bool canAttack = inRange && (!requireVisionToAttack || hasVision);

        SetCombatValues(inRange, canAttack);

        TyrantOverlayReporter.Instance?.ReportCombat(inRange, canAttack, false);

        return Status.Running;
    }

    private void ResolvePlayer()
    {
        if (Player != null && Player.Value != null)
        {
            playerTransform = Player.Value.transform;

            if (PlayerTransform != null)
                PlayerTransform.Value = playerTransform;

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

    private void SetCombatValues(bool inRange, bool canAttack)
    {
        if (IsInRange != null)
            IsInRange.Value = inRange;

        if (CanAttack != null)
            CanAttack.Value = canAttack;
    }
}
