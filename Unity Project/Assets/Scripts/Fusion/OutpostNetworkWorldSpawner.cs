using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class OutpostNetworkWorldSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Prefabs")]
    [SerializeField] private NetworkPrefabRef capturePointPrefab;
    [SerializeField] private NetworkPrefabRef guardPrefab;

    [Header("Spawn Positions")]
    [SerializeField] private Vector3 capturePointPosition = new Vector3(0f, 0.8f, -14f);
    [SerializeField] private Vector3 guardPosition = new Vector3(0f, 1f, -5f);

    [Header("Spawn Control")]
    [SerializeField] private bool requireSharedModeMaster = true;
    [SerializeField] private float spawnDelaySeconds = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private NetworkRunner runner;
    private bool spawnAttemptStarted;
    private bool spawnedWorldObjects;

    public void Initialise(NetworkRunner networkRunner)
    {
        runner = networkRunner;
        runner.AddCallbacks(this);

        if (logDebug)
            Debug.Log("[OutpostNetworkWorldSpawner] Initialised and callbacks registered.");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer)
            return;

        if (spawnAttemptStarted)
            return;

        spawnAttemptStarted = true;
        StartCoroutine(DelayedSpawnAttempt());
    }

    private IEnumerator DelayedSpawnAttempt()
    {
        if (logDebug)
            Debug.Log("[OutpostNetworkWorldSpawner] Waiting before world spawn attempt.");

        yield return new WaitForSeconds(spawnDelaySeconds);

        TrySpawnWorldObjects();
    }

    public void TrySpawnWorldObjects()
    {
        if (spawnedWorldObjects)
        {
            if (logDebug)
                Debug.Log("[OutpostNetworkWorldSpawner] World objects already spawned. Skipping.");

            return;
        }

        if (runner == null)
        {
            Debug.LogWarning("[OutpostNetworkWorldSpawner] Runner is null. Cannot spawn world objects.");
            return;
        }

        if (!runner.IsRunning)
        {
            Debug.LogWarning("[OutpostNetworkWorldSpawner] Runner is not running yet. Cannot spawn world objects.");
            return;
        }

        if (requireSharedModeMaster && !runner.IsSharedModeMasterClient)
        {
            if (logDebug)
                Debug.Log("[OutpostNetworkWorldSpawner] This client is not Shared Mode Master Client. Skipping world spawn.");

            return;
        }

        if (logDebug)
        {
            Debug.Log($"[OutpostNetworkWorldSpawner] Attempting world spawn.");
            Debug.Log($"[OutpostNetworkWorldSpawner] IsSharedModeMasterClient: {runner.IsSharedModeMasterClient}");
            Debug.Log($"[OutpostNetworkWorldSpawner] Capture prefab valid: {capturePointPrefab.IsValid}");
            Debug.Log($"[OutpostNetworkWorldSpawner] Guard prefab valid: {guardPrefab.IsValid}");
        }

        SpawnCapturePoint();
        SpawnGuard();

        spawnedWorldObjects = true;
    }

    private void SpawnCapturePoint()
    {
        if (!capturePointPrefab.IsValid)
        {
            Debug.LogWarning("[OutpostNetworkWorldSpawner] Capture point prefab is missing or not registered.");
            return;
        }

        NetworkObject capturePoint = runner.Spawn(
            capturePointPrefab,
            capturePointPosition,
            Quaternion.identity
        );

        if (logDebug)
            Debug.Log($"[OutpostNetworkWorldSpawner] Spawned networked capture point at {capturePoint.transform.position}");
    }

    private void SpawnGuard()
    {
        if (!guardPrefab.IsValid)
        {
            Debug.LogWarning("[OutpostNetworkWorldSpawner] Guard prefab is missing or not registered.");
            return;
        }

        NetworkObject guard = runner.Spawn(
            guardPrefab,
            guardPosition,
            Quaternion.identity
        );

        if (logDebug)
            Debug.Log($"[OutpostNetworkWorldSpawner] Spawned networked guard at {guard.transform.position}");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

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