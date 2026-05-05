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

    [Header("Respawn")]
    [SerializeField] private bool respawnOnNeutralGuardContact = true;
    [SerializeField] private float neutralGuardContactRadius = 1.8f;
    [SerializeField] private float respawnCooldownSeconds = 1.5f;
    [SerializeField] private float outOfBoundsY = -20f;

    [Header("Spawn References")]
    [SerializeField] private Transform blueSpawnPoint;
    [SerializeField] private Transform redSpawnPoint;
    [SerializeField] private Vector3 fallbackBlueSpawnPosition = new Vector3(-44f, 1f, 0f);
    [SerializeField] private Vector3 fallbackRedSpawnPosition = new Vector3(44f, 1f, 0f);

    [Header("Visuals")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Material blueMaterial;
    [SerializeField] private Material redMaterial;

    [Networked] private TickTimer RespawnCooldown { get; set; }

    private CharacterController controller;
    private NetworkTransform networkTransform;
    private NetworkedOutpostPlayerTeam networkedTeam;
    private OutpostPlayerTeam localTeam;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        networkTransform = GetComponent<NetworkTransform>();
        networkedTeam = GetComponent<NetworkedOutpostPlayerTeam>();
        localTeam = GetComponent<OutpostPlayerTeam>();

        if (bodyRenderer == null)
            bodyRenderer = GetComponentInChildren<Renderer>();
    }

    public override void Spawned()
    {
        AutoFindSpawnPointsIfMissing();
        UpdateVisuals();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        OutpostTeam currentTeam = GetCurrentTeam();

        if (transform.position.y < outOfBoundsY)
        {
            RespawnToTeamSpawn(currentTeam, "fell out of bounds");
            return;
        }

        HandleMovement();

        if (respawnOnNeutralGuardContact)
            CheckNeutralGuardContact();
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void HandleMovement()
    {
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

    private void CheckNeutralGuardContact()
    {
        if (RespawnCooldown.IsRunning)
            return;

        OutpostTeam myTeam = GetCurrentTeam();

        if (myTeam != OutpostTeam.Blue && myTeam != OutpostTeam.Red)
            return;

        OutpostPlayerTeam[] teamObjects = FindObjectsByType<OutpostPlayerTeam>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (OutpostPlayerTeam teamObject in teamObjects)
        {
            if (teamObject == null)
                continue;

            if (teamObject == localTeam)
                continue;

            if (teamObject.Team != OutpostTeam.Neutral)
                continue;

            float distance = Vector3.Distance(transform.position, teamObject.transform.position);

            if (distance > neutralGuardContactRadius)
                continue;

            RespawnToTeamSpawn(myTeam, "caught by neutral guard");
            return;
        }
    }

    private void RespawnToTeamSpawn(OutpostTeam team, string reason)
    {
        if (team != OutpostTeam.Blue && team != OutpostTeam.Red)
            return;

        Vector3 targetPosition = GetSpawnPositionForTeam(team);
        Quaternion targetRotation = Quaternion.identity;

        verticalVelocity = 0f;

        if (controller != null)
            controller.enabled = false;

        if (networkTransform != null)
            networkTransform.Teleport(targetPosition, targetRotation);
        else
            transform.SetPositionAndRotation(targetPosition, targetRotation);

        transform.SetPositionAndRotation(targetPosition, targetRotation);

        if (controller != null)
            controller.enabled = true;

        RespawnCooldown = TickTimer.CreateFromSeconds(Runner, respawnCooldownSeconds);

        Debug.Log($"[OutpostNetworkPlayer] {team} player respawned because they were {reason}. Target: {targetPosition}");
    }

    private Vector3 GetSpawnPositionForTeam(OutpostTeam team)
    {
        AutoFindSpawnPointsIfMissing();

        if (team == OutpostTeam.Red)
        {
            if (redSpawnPoint != null)
                return redSpawnPoint.position;

            return fallbackRedSpawnPosition;
        }

        if (blueSpawnPoint != null)
            return blueSpawnPoint.position;

        return fallbackBlueSpawnPosition;
    }

    private void AutoFindSpawnPointsIfMissing()
    {
        if (blueSpawnPoint == null)
        {
            GameObject blueSpawn = GameObject.Find("PlayerSpawn_Blue_01");

            if (blueSpawn != null)
                blueSpawnPoint = blueSpawn.transform;
        }

        if (redSpawnPoint == null)
        {
            GameObject redSpawn = GameObject.Find("PlayerSpawn_Red_01");

            if (redSpawn != null)
                redSpawnPoint = redSpawn.transform;
        }
    }

    private OutpostTeam GetCurrentTeam()
    {
        if (networkedTeam == null)
            networkedTeam = GetComponent<NetworkedOutpostPlayerTeam>();

        if (networkedTeam != null)
            return (OutpostTeam)networkedTeam.TeamId;

        if (localTeam == null)
            localTeam = GetComponent<OutpostPlayerTeam>();

        if (localTeam != null)
            return localTeam.Team;

        return OutpostTeam.None;
    }

    private void UpdateVisuals()
    {
        if (bodyRenderer == null)
            return;

        OutpostTeam currentTeam = GetCurrentTeam();

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