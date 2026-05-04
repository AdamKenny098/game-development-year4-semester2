using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class OutpostNetworkPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Visuals")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;

    private NetworkedOutpostPlayerTeam team;

    private void Awake()
    {
        team = GetComponent<NetworkedOutpostPlayerTeam>();

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!GetInput<FusionInputData>(out FusionInputData input))
            return;

        Vector3 move = new Vector3(input.move.x, 0f, input.move.y);

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        transform.position += move * moveSpeed * Runner.DeltaTime;
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (bodyRenderer == null || team == null)
            return;

        OutpostTeam currentTeam = (OutpostTeam)team.TeamId;

        if (currentTeam == OutpostTeam.Blue && blueMaterial != null)
            bodyRenderer.sharedMaterial = blueMaterial;
        else if (currentTeam == OutpostTeam.Red && redMaterial != null)
            bodyRenderer.sharedMaterial = redMaterial;
    }
}