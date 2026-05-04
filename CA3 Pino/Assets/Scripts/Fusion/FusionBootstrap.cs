using System.Threading.Tasks;
using Fusion;
using UnityEngine;

public class FusionBootstrap : MonoBehaviour
{
    [Header("Session")]
    [SerializeField] private string sessionName = "CA3_OutpostCapture";

    [Header("Spawners")]
    [SerializeField] private OutpostFusionPlayerSpawner playerSpawner;
    [SerializeField] private OutpostNetworkWorldSpawner worldSpawner;

    private NetworkRunner activeRunner;

    private async void Start()
    {
        await StartFusionSession();
    }

    private async Task StartFusionSession()
    {
        if (activeRunner != null)
            return;

        GameObject runnerObject = new GameObject("NetworkRunner");

        activeRunner = runnerObject.AddComponent<NetworkRunner>();
        activeRunner.ProvideInput = true;

        if (playerSpawner != null)
            playerSpawner.Initialise(activeRunner);
        else
            Debug.LogWarning("[FusionBootstrap] No OutpostFusionPlayerSpawner assigned.");

        if (worldSpawner != null)
            worldSpawner.Initialise(activeRunner);
        else
            Debug.LogWarning("[FusionBootstrap] No OutpostNetworkWorldSpawner assigned.");

        StartGameResult result = await activeRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName
        });

        if (!result.Ok)
        {
            Debug.LogError($"[FusionBootstrap] Failed to start Fusion session: {result.ShutdownReason}");
            return;
        }

        Debug.Log("[FusionBootstrap] Fusion Shared Mode session started.");
    }
}