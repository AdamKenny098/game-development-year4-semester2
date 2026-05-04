using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionBootstrap : MonoBehaviour
{
    [Header("Session")]
    [SerializeField] private string sessionName = "CA3_OutpostCapture";

    [Header("Spawner")]
    [SerializeField] private OutpostFusionPlayerSpawner playerSpawner;

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
        runnerObject.AddComponent<NetworkSceneManagerDefault>();

        activeRunner.ProvideInput = true;

        if (playerSpawner != null)
            playerSpawner.Initialise(activeRunner);
        else
            Debug.LogWarning("[FusionBootstrap] No player spawner assigned.");

        NetworkSceneInfo sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        StartGameResult result = await activeRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName,
            Scene = sceneInfo,
            SceneManager = activeRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"[FusionBootstrap] Failed to start Fusion session: {result.ShutdownReason}");
            return;
        }

        Debug.Log("[FusionBootstrap] Fusion Shared Mode session started.");
    }
}