using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(BoxCollider))]
public class NetworkedCapturePoint : NetworkBehaviour
{
    [Header("Capture Settings")]
    [SerializeField] private float captureDuration = 8f;
    [SerializeField] private float scoreDuration = 60f;
    [SerializeField] private int maxCaptureSpeedPlayers = 3;

    [Header("Visuals")]
    [SerializeField] private Renderer beaconRenderer;
    [SerializeField] private Material neutralMaterial;
    [SerializeField] private Material blueCapturingMaterial;
    [SerializeField] private Material redCapturingMaterial;
    [SerializeField] private Material blueOwnedMaterial;
    [SerializeField] private Material redOwnedMaterial;
    [SerializeField] private Light captureLight;

    [Header("UI")]
    [SerializeField] private Slider captureSlider;
    [SerializeField] private Slider blueScoreSlider;
    [SerializeField] private Slider redScoreSlider;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text blueScoreText;
    [SerializeField] private TMP_Text redScoreText;

    [Networked] public float CaptureProgress { get; set; }
    [Networked] public float BlueScore { get; set; }
    [Networked] public float RedScore { get; set; }
    [Networked] public int OwnerTeamId { get; set; }
    [Networked] public int CapturingTeamId { get; set; }
    [Networked] public NetworkBool MatchFinished { get; set; }
    [Networked] public NetworkBool NeutralContesting { get; set; }

    private readonly HashSet<OutpostPlayerTeam> bluePlayersInside = new();
    private readonly HashSet<OutpostPlayerTeam> redPlayersInside = new();
    private readonly HashSet<OutpostPlayerTeam> neutralPlayersInside = new();

    private OutpostTeam OwnerTeam => (OutpostTeam)OwnerTeamId;
    private OutpostTeam CapturingTeam => (OutpostTeam)CapturingTeamId;

    public override void Spawned()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        AutoFindSceneReferences();
        UpdateVisuals();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (MatchFinished)
            return;

        UpdateAuthorityPresenceFlags();
        UpdateOwnerScore();
        UpdateCaptureProgress();
        CheckWinCondition();
    }

    public override void Render()
    {
        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        OutpostPlayerTeam team = other.GetComponentInParent<OutpostPlayerTeam>();

        if (team == null)
            return;

        if (team.Team == OutpostTeam.Blue)
            bluePlayersInside.Add(team);
        else if (team.Team == OutpostTeam.Red)
            redPlayersInside.Add(team);
        else if (team.Team == OutpostTeam.Neutral)
            neutralPlayersInside.Add(team);

        UpdateAuthorityPresenceFlags();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!Object || !Object.HasStateAuthority)
            return;

        OutpostPlayerTeam team = other.GetComponentInParent<OutpostPlayerTeam>();

        if (team == null)
            return;

        bluePlayersInside.Remove(team);
        redPlayersInside.Remove(team);
        neutralPlayersInside.Remove(team);

        UpdateAuthorityPresenceFlags();
    }

    private void UpdateAuthorityPresenceFlags()
    {
        RemoveInvalidEntries(bluePlayersInside);
        RemoveInvalidEntries(redPlayersInside);
        RemoveInvalidEntries(neutralPlayersInside);

        NeutralContesting = neutralPlayersInside.Count > 0;
    }

    private void RemoveInvalidEntries(HashSet<OutpostPlayerTeam> set)
    {
        set.RemoveWhere(item => item == null || !item.gameObject.activeInHierarchy);
    }

    private void UpdateOwnerScore()
    {
        if (OwnerTeam == OutpostTeam.None)
            return;

        if (NeutralContesting)
            return;

        bool bluePresent = bluePlayersInside.Count > 0;
        bool redPresent = redPlayersInside.Count > 0;

        if (bluePresent && redPresent)
            return;

        float scoreGain = Runner.DeltaTime / scoreDuration;

        if (OwnerTeam == OutpostTeam.Blue)
            BlueScore = Mathf.Clamp01(BlueScore + scoreGain);

        if (OwnerTeam == OutpostTeam.Red)
            RedScore = Mathf.Clamp01(RedScore + scoreGain);
    }

    private void UpdateCaptureProgress()
    {
        bool bluePresent = bluePlayersInside.Count > 0;
        bool redPresent = redPlayersInside.Count > 0;

        if (NeutralContesting)
            return;

        if (bluePresent && redPresent)
            return;

        if (!bluePresent && !redPresent)
            return;

        OutpostTeam activeTeam = bluePresent ? OutpostTeam.Blue : OutpostTeam.Red;

        if (activeTeam == OwnerTeam)
        {
            CaptureProgress = 0f;
            CapturingTeamId = (int)OutpostTeam.None;
            return;
        }

        if (CapturingTeam != activeTeam)
        {
            CapturingTeamId = (int)activeTeam;
            CaptureProgress = 0f;
        }

        int playerCount = activeTeam == OutpostTeam.Blue
            ? bluePlayersInside.Count
            : redPlayersInside.Count;

        int clampedPlayerCount = Mathf.Clamp(playerCount, 1, maxCaptureSpeedPlayers);
        float speedMultiplier = clampedPlayerCount;

        CaptureProgress += (Runner.DeltaTime / captureDuration) * speedMultiplier;
        CaptureProgress = Mathf.Clamp01(CaptureProgress);

        if (CaptureProgress >= 1f)
        {
            OwnerTeamId = (int)activeTeam;
            CapturingTeamId = (int)OutpostTeam.None;
            CaptureProgress = 0f;
        }
    }

    private void CheckWinCondition()
    {
        if (BlueScore >= 1f)
        {
            BlueScore = 1f;
            MatchFinished = true;
        }

        if (RedScore >= 1f)
        {
            RedScore = 1f;
            MatchFinished = true;
        }
    }

    private void AutoFindSceneReferences()
    {
        if (beaconRenderer == null)
        {
            GameObject beacon = GameObject.Find("Capture_Beacon");

            if (beacon != null)
                beaconRenderer = beacon.GetComponent<Renderer>();
        }

        if (captureLight == null)
        {
            GameObject lightObject = GameObject.Find("Capture_Point_Light");

            if (lightObject != null)
                captureLight = lightObject.GetComponent<Light>();
        }

        if (statusText == null)
            statusText = FindText("StatusText");

        if (blueScoreText == null)
            blueScoreText = FindText("BlueScoreText");

        if (redScoreText == null)
            redScoreText = FindText("RedScoreText");

        if (captureSlider == null)
            captureSlider = FindSlider("CaptureSlider");

        if (blueScoreSlider == null)
            blueScoreSlider = FindSlider("BlueScoreSlider");

        if (redScoreSlider == null)
            redScoreSlider = FindSlider("RedScoreSlider");
    }

    private TMP_Text FindText(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private Slider FindSlider(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Slider>() : null;
    }

    private void UpdateVisuals()
    {
        UpdateSliders();
        UpdateText();
        UpdateBeaconMaterial();
        UpdateCaptureLight();
    }

    private void UpdateSliders()
    {
        if (captureSlider != null)
            captureSlider.value = CaptureProgress;

        if (blueScoreSlider != null)
            blueScoreSlider.value = BlueScore;

        if (redScoreSlider != null)
            redScoreSlider.value = RedScore;
    }

    private void UpdateText()
    {
        if (blueScoreText != null)
            blueScoreText.text = $"BLUE {Mathf.RoundToInt(BlueScore * 100f)}%";

        if (redScoreText != null)
            redScoreText.text = $"RED {Mathf.RoundToInt(RedScore * 100f)}%";

        if (statusText == null)
            return;

        if (MatchFinished)
        {
            statusText.text = BlueScore >= 1f ? "BLUE WINS" : "RED WINS";
            return;
        }

        bool bluePresent = bluePlayersInside.Count > 0;
        bool redPresent = redPlayersInside.Count > 0;

        if (NeutralContesting)
        {
            statusText.text = "GUARD CONTESTING";
            return;
        }

        if (bluePresent && redPresent)
        {
            statusText.text = "CONTESTED";
            return;
        }

        if (CapturingTeam == OutpostTeam.Blue)
        {
            statusText.text = $"BLUE CAPTURING {Mathf.RoundToInt(CaptureProgress * 100f)}%";
            return;
        }

        if (CapturingTeam == OutpostTeam.Red)
        {
            statusText.text = $"RED CAPTURING {Mathf.RoundToInt(CaptureProgress * 100f)}%";
            return;
        }

        if (OwnerTeam == OutpostTeam.Blue)
        {
            statusText.text = "BLUE CONTROLS THE POINT";
            return;
        }

        if (OwnerTeam == OutpostTeam.Red)
        {
            statusText.text = "RED CONTROLS THE POINT";
            return;
        }

        statusText.text = "CONTROL POINT NEUTRAL";
    }

    private void UpdateBeaconMaterial()
    {
        if (beaconRenderer == null)
            return;

        Material targetMaterial = neutralMaterial;

        if (OwnerTeam == OutpostTeam.Blue && blueOwnedMaterial != null)
            targetMaterial = blueOwnedMaterial;
        else if (OwnerTeam == OutpostTeam.Red && redOwnedMaterial != null)
            targetMaterial = redOwnedMaterial;
        else if (CapturingTeam == OutpostTeam.Blue && blueCapturingMaterial != null)
            targetMaterial = blueCapturingMaterial;
        else if (CapturingTeam == OutpostTeam.Red && redCapturingMaterial != null)
            targetMaterial = redCapturingMaterial;

        if (targetMaterial != null)
            beaconRenderer.sharedMaterial = targetMaterial;
    }

    private void UpdateCaptureLight()
    {
        if (captureLight == null)
            return;

        captureLight.enabled =
            OwnerTeam != OutpostTeam.None ||
            CapturingTeam != OutpostTeam.None ||
            NeutralContesting;

        if (NeutralContesting)
            captureLight.color = Color.yellow;
        else if (OwnerTeam == OutpostTeam.Blue || CapturingTeam == OutpostTeam.Blue)
            captureLight.color = Color.blue;
        else if (OwnerTeam == OutpostTeam.Red || CapturingTeam == OutpostTeam.Red)
            captureLight.color = Color.red;
        else
            captureLight.color = Color.white;
    }
}