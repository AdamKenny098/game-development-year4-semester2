using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(OutpostPlayerTeam))]
[RequireComponent(typeof(NetworkedOutpostPlayerTeam))]
public class OutpostNetworkPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float gravity = -20f;

    [Header("Visuals")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;

    private CharacterController controller;
    private NetworkedOutpostPlayerTeam networkedTeam;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        networkedTeam = GetComponent<NetworkedOutpostPlayerTeam>();

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>();
    }

    public override void Spawned()
    {
        UpdateVisuals();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!GetInput(out FusionInputData input))
            return;

        Vector3 move = new Vector3(input.Move.x, 0f, input.Move.y);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Runner.DeltaTime;

        Vector3 finalMove = move * moveSpeed;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Runner.DeltaTime);
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (bodyRenderer == null || networkedTeam == null)
            return;

        OutpostTeam currentTeam = (OutpostTeam)networkedTeam.TeamId;

        if (currentTeam == OutpostTeam.Blue && blueMaterial != null)
        {
            bodyRenderer.sharedMaterial = blueMaterial;
        }
        else if (currentTeam == OutpostTeam.Red && redMaterial != null)
        {
            bodyRenderer.sharedMaterial = redMaterial;
        }
    }
}