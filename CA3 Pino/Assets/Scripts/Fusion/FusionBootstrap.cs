using System;
using System.Threading.Tasks;
using Fusion;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class FusionBootstrap : MonoBehaviour
{
    [Header("Session")]
    [SerializeField] private string sessionName = "CA3_OutpostCapture";

    [Header("Spawners")]
    [SerializeField] private OutpostFusionPlayerSpawner playerSpawner;
    [SerializeField] private OutpostNetworkWorldSpawner worldSpawner;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private NetworkRunner activeRunner;
    private bool startupInProgress;

    private async void Start()
    {
        await StartAuthenticatedFusionSession();
    }

    private async Task StartAuthenticatedFusionSession()
    {
        if (startupInProgress)
            return;

        startupInProgress = true;

        try
        {
            await InitialiseUnityServices();
            await SignInWithUnityAuthentication();
            await StartFusionSession();
        }
        catch (Exception exception)
        {
            Debug.LogError($"[FusionBootstrap] Startup failed: {exception.Message}");
            Debug.LogException(exception);
        }
        finally
        {
            startupInProgress = false;
        }
    }

    private async Task InitialiseUnityServices()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized)
        {
            if (logDebug)
                Debug.Log("[FusionBootstrap] Unity Services already initialized.");

            return;
        }

        if (logDebug)
            Debug.Log("[FusionBootstrap] Initializing Unity Services...");

        await UnityServices.InitializeAsync();

        if (logDebug)
            Debug.Log("[FusionBootstrap] Unity Services initialized.");
    }

    private async Task SignInWithUnityAuthentication()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            if (logDebug)
            {
                Debug.Log($"[FusionBootstrap] Already signed in with Unity Authentication.");
                Debug.Log($"[FusionBootstrap] Unity Player ID: {AuthenticationService.Instance.PlayerId}");
            }

            return;
        }

        if (logDebug)
            Debug.Log("[FusionBootstrap] Signing in anonymously with Unity Authentication...");

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (logDebug)
        {
            Debug.Log("[FusionBootstrap] Unity Authentication sign-in successful.");
            Debug.Log($"[FusionBootstrap] Unity Player ID: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"[FusionBootstrap] Access token available: {!string.IsNullOrEmpty(AuthenticationService.Instance.AccessToken)}");
        }
    }

    private async Task StartFusionSession()
    {
        if (activeRunner != null)
            return;

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("[FusionBootstrap] Fusion start blocked because Unity Authentication is not signed in.");
            return;
        }

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

        if (logDebug)
            Debug.Log("[FusionBootstrap] Starting Fusion Shared Mode session after Unity Authentication.");

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

        if (logDebug)
            Debug.Log("[FusionBootstrap] Fusion Shared Mode session started.");
    }
}