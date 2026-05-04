using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class OutpostFusionPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform blueSpawnPoint;
    [SerializeField] private Transform redSpawnPoint;

    private NetworkRunner runner;
    private NetworkObject localPlayerObject;

    public void Initialise(NetworkRunner networkRunner)
    {
        runner = networkRunner;
        runner.AddCallbacks(this);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        OutpostTeam assignedTeam = GetTeamForPlayer(player);
        Transform spawnPoint = GetSpawnPoint(assignedTeam);

        NetworkObject spawnedPlayer = runner.Spawn(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            player
        );

        localPlayerObject = spawnedPlayer;

        NetworkedOutpostPlayerTeam networkedTeam =
            spawnedPlayer.GetComponent<NetworkedOutpostPlayerTeam>();

        if (networkedTeam != null)
            networkedTeam.SetTeam(assignedTeam);

        Debug.Log($"[OutpostFusionPlayerSpawner] Spawned local player as {assignedTeam}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        if (localPlayerObject != null)
        {
            runner.Despawn(localPlayerObject);
            localPlayerObject = null;
        }
    }

    private OutpostTeam GetTeamForPlayer(PlayerRef player)
    {
        List<PlayerRef> players = new List<PlayerRef>();

        foreach (PlayerRef activePlayer in runner.ActivePlayers)
            players.Add(activePlayer);

        players.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));

        int index = players.IndexOf(player);

        if (index == 0)
            return OutpostTeam.Blue;

        return OutpostTeam.Red;
    }

    private Transform GetSpawnPoint(OutpostTeam team)
    {
        if (team == OutpostTeam.Red && redSpawnPoint != null)
            return redSpawnPoint;

        if (blueSpawnPoint != null)
            return blueSpawnPoint;

        if (redSpawnPoint != null)
            return redSpawnPoint;

        return transform;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        FusionInputData data = new FusionInputData
        {
            move = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            ),
            interact = Input.GetKeyDown(KeyCode.F)
        };

        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}