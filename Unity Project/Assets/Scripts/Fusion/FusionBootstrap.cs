using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
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

    [Header("Authentication")]
    [SerializeField] private bool usePhotonCustomAuth = true;

    [Tooltip("Turn this on only when recording rejection evidence. Turn it off again after the test.")]
    [SerializeField] private bool simulateInvalidTokenForRejectionTest = false;

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
                Debug.Log("[FusionBootstrap] Already signed in with Unity Authentication.");
                Debug.Log($"[FusionBootstrap] Unity Player ID: {AuthenticationService.Instance.PlayerId}");
                Debug.Log($"[FusionBootstrap] Access token available: {!string.IsNullOrEmpty(AuthenticationService.Instance.AccessToken)}");
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

        StartGameArgs startGameArgs = new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = sessionName
        };

        if (usePhotonCustomAuth)
            startGameArgs.AuthValues = BuildPhotonCustomAuthValues();

        StartGameResult result = await activeRunner.StartGame(startGameArgs);

        if (!result.Ok)
        {
            Debug.LogError($"[FusionBootstrap] Failed to start Fusion session: {result.ShutdownReason}");
            return;
        }

        if (logDebug)
            Debug.Log("[FusionBootstrap] Fusion Shared Mode session started.");
    }

    private AuthenticationValues BuildPhotonCustomAuthValues()
    {
        string unityPlayerId = AuthenticationService.Instance.PlayerId;
        string unityAccessToken = AuthenticationService.Instance.AccessToken;

        if (simulateInvalidTokenForRejectionTest)
        {
            unityAccessToken = "bad-token-for-ca3-rejection-test";

            if (logDebug)
                Debug.LogWarning("[FusionBootstrap] Rejection test enabled. Sending intentionally invalid Unity access token.");
        }

        AuthenticationValues authValues = new AuthenticationValues
        {
            AuthType = CustomAuthenticationType.Custom,
            UserId = unityPlayerId
        };

        Dictionary<string, object> authPayload = new Dictionary<string, object>
        {
            { "unityPlayerId", unityPlayerId },
            { "unityAccessToken", unityAccessToken }
        };

        authValues.SetAuthPostData(authPayload);

        if (logDebug)
        {
            Debug.Log("[FusionBootstrap] Photon Custom Auth values prepared.");
            Debug.Log($"[FusionBootstrap] Photon Custom Auth UserId: {unityPlayerId}");
            Debug.Log($"[FusionBootstrap] Unity access token included: {!string.IsNullOrEmpty(unityAccessToken)}");
        }

        return authValues;
    }
}