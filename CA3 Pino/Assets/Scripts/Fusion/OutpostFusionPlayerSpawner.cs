using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class OutpostFusionPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefab")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Hardcoded Spawn Positions")]
    [SerializeField] private Vector3 blueSpawnPosition = new Vector3(-44f, 2f, 0f);
    [SerializeField] private Vector3 redSpawnPosition = new Vector3(44f, 2f, 0f);

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;
    [SerializeField] private bool forceSpawnPositionForDebug = true;

    private NetworkRunner runner;
    private NetworkObject localPlayerObject;

    public void Initialise(NetworkRunner networkRunner)
    {
        runner = networkRunner;
        runner.AddCallbacks(this);

        if (logDebug)
            Debug.Log("[OutpostFusionPlayerSpawner] Initialised and callbacks registered.");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        if (localPlayerObject != null)
        {
            if (logDebug)
                Debug.LogWarning("[OutpostFusionPlayerSpawner] Local player already spawned. Ignoring duplicate join callback.");

            return;
        }

        OutpostTeam assignedTeam = GetTeamForPlayer(player);
        Vector3 spawnPosition = GetHardcodedSpawnPosition(assignedTeam);
        Quaternion spawnRotation = Quaternion.identity;

        if (logDebug)
        {
            Debug.Log($"[OutpostFusionPlayerSpawner] Local player {player} assigned team: {assignedTeam}");
            Debug.Log($"[OutpostFusionPlayerSpawner] Spawn requested at: {spawnPosition}");
        }

        NetworkObject spawnedPlayer = runner.Spawn(
            playerPrefab,
            spawnPosition,
            spawnRotation,
            player,
            (spawnRunner, spawnedObject) =>
            {
                spawnedObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

                OutpostPlayerTeam localTeam = spawnedObject.GetComponent<OutpostPlayerTeam>();

                if (localTeam != null)
                    localTeam.SetTeam(assignedTeam);

                NetworkedOutpostPlayerTeam networkedTeam =
                    spawnedObject.GetComponent<NetworkedOutpostPlayerTeam>();

                if (networkedTeam != null)
                    networkedTeam.SetTeam(assignedTeam);
            }
        );

        localPlayerObject = spawnedPlayer;
        runner.SetPlayerObject(player, spawnedPlayer);

        spawnedPlayer.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        ApplyTeamToSpawnedPlayer(spawnedPlayer, assignedTeam);

        if (forceSpawnPositionForDebug)
            StartCoroutine(ForceSpawnPositionForFrames(spawnedPlayer, spawnPosition, spawnRotation));

        if (logDebug)
        {
            Debug.Log($"[OutpostFusionPlayerSpawner] Spawned object name: {spawnedPlayer.name}");
            Debug.Log($"[OutpostFusionPlayerSpawner] Spawned object actual position after spawn: {spawnedPlayer.transform.position}");
        }
    }

    private IEnumerator ForceSpawnPositionForFrames(NetworkObject spawnedPlayer, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        for (int i = 0; i < 20; i++)
        {
            if (spawnedPlayer == null)
                yield break;

            spawnedPlayer.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

            if (logDebug)
                Debug.Log($"[OutpostFusionPlayerSpawner] Force spawn frame {i}: {spawnedPlayer.transform.position}");

            yield return null;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        if (localPlayerObject == null)
            return;

        runner.Despawn(localPlayerObject);
        localPlayerObject = null;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        Vector2 move = Vector2.zero;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed)
                move.y += 1f;

            if (keyboard.sKey.isPressed)
                move.y -= 1f;

            if (keyboard.dKey.isPressed)
                move.x += 1f;

            if (keyboard.aKey.isPressed)
                move.x -= 1f;
        }

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        FusionInputData data = new FusionInputData
        {
            Move = move
        };

        input.Set(data);
    }

    private OutpostTeam GetTeamForPlayer(PlayerRef player)
    {
        List<PlayerRef> players = new();

        foreach (PlayerRef activePlayer in runner.ActivePlayers)
            players.Add(activePlayer);

        players.Sort((a, b) => a.RawEncoded.CompareTo(b.RawEncoded));

        int index = players.IndexOf(player);

        if (logDebug)
            Debug.Log($"[OutpostFusionPlayerSpawner] Player index in session: {index}");

        if (index == 0)
            return OutpostTeam.Blue;

        return OutpostTeam.Red;
    }

    private Vector3 GetHardcodedSpawnPosition(OutpostTeam team)
    {
        if (team == OutpostTeam.Red)
            return redSpawnPosition;

        return blueSpawnPosition;
    }

    private void ApplyTeamToSpawnedPlayer(NetworkObject spawnedPlayer, OutpostTeam assignedTeam)
    {
        NetworkedOutpostPlayerTeam networkedTeam =
            spawnedPlayer.GetComponent<NetworkedOutpostPlayerTeam>();

        if (networkedTeam != null)
            networkedTeam.SetTeam(assignedTeam);
        else
            Debug.LogWarning("[OutpostFusionPlayerSpawner] Spawned player has no NetworkedOutpostPlayerTeam.");

        OutpostPlayerTeam localTeam =
            spawnedPlayer.GetComponent<OutpostPlayerTeam>();

        if (localTeam != null)
            localTeam.SetTeam(assignedTeam);
        else
            Debug.LogWarning("[OutpostFusionPlayerSpawner] Spawned player has no OutpostPlayerTeam.");
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