using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CapturePointLocal : MonoBehaviour
{
    [Header("Capture Settings")]
    [SerializeField] private float captureDuration = 8f;
    [SerializeField] private float scoreDuration = 60f;
    [SerializeField] private int maxCaptureSpeedPlayers = 3;

    [Header("Optional Setup Lock")]
    [SerializeField] private bool startsLocked = false;
    [SerializeField] private float unlockDelay = 10f;

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

    private readonly HashSet<OutpostPlayerTeam> bluePlayersInside = new();
    private readonly HashSet<OutpostPlayerTeam> redPlayersInside = new();

    private OutpostTeam owner = OutpostTeam.None;
    private OutpostTeam capturingTeam = OutpostTeam.None;

    private float captureProgress;
    private float blueScore;
    private float redScore;
    private float unlockTimer;
    private bool matchFinished;

    private bool IsLocked => startsLocked && unlockTimer < unlockDelay;

    private void Start()
    {
        unlockTimer = 0f;
        captureProgress = 0f;
        blueScore = 0f;
        redScore = 0f;
        owner = OutpostTeam.None;
        capturingTeam = OutpostTeam.None;

        UpdateVisuals();
    }

    private void Update()
    {
        if (matchFinished)
            return;

        if (IsLocked)
        {
            unlockTimer += Time.deltaTime;
            UpdateVisuals();
            return;
        }

        UpdateOwnerScore();
        UpdateCaptureProgress();
        CheckWinCondition();
        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        OutpostPlayerTeam playerTeam = other.GetComponentInParent<OutpostPlayerTeam>();

        if (playerTeam == null)
            return;

        if (playerTeam.Team == OutpostTeam.Blue)
            bluePlayersInside.Add(playerTeam);

        if (playerTeam.Team == OutpostTeam.Red)
            redPlayersInside.Add(playerTeam);

        UpdateVisuals();
    }

    private void OnTriggerExit(Collider other)
    {
        OutpostPlayerTeam playerTeam = other.GetComponentInParent<OutpostPlayerTeam>();

        if (playerTeam == null)
            return;

        bluePlayersInside.Remove(playerTeam);
        redPlayersInside.Remove(playerTeam);

        UpdateVisuals();
    }

    private void UpdateOwnerScore()
    {
        if (owner == OutpostTeam.None)
            return;

        float scoreGain = Time.deltaTime / scoreDuration;

        if (owner == OutpostTeam.Blue)
            blueScore = Mathf.Clamp01(blueScore + scoreGain);

        if (owner == OutpostTeam.Red)
            redScore = Mathf.Clamp01(redScore + scoreGain);
    }

    private void UpdateCaptureProgress()
    {
        bool bluePresent = bluePlayersInside.Count > 0;
        bool redPresent = redPlayersInside.Count > 0;

        if (bluePresent && redPresent)
            return;

        if (!bluePresent && !redPresent)
            return;

        OutpostTeam activeTeam = bluePresent ? OutpostTeam.Blue : OutpostTeam.Red;

        if (activeTeam == owner)
        {
            captureProgress = 0f;
            capturingTeam = OutpostTeam.None;
            return;
        }

        if (capturingTeam != activeTeam)
        {
            capturingTeam = activeTeam;
            captureProgress = 0f;
        }

        int playerCount = activeTeam == OutpostTeam.Blue
            ? bluePlayersInside.Count
            : redPlayersInside.Count;

        int clampedPlayerCount = Mathf.Clamp(playerCount, 1, maxCaptureSpeedPlayers);
        float speedMultiplier = clampedPlayerCount;

        captureProgress += (Time.deltaTime / captureDuration) * speedMultiplier;
        captureProgress = Mathf.Clamp01(captureProgress);

        if (captureProgress >= 1f)
        {
            owner = activeTeam;
            capturingTeam = OutpostTeam.None;
            captureProgress = 0f;
        }
    }

    private void CheckWinCondition()
    {
        if (blueScore >= 1f)
        {
            blueScore = 1f;
            matchFinished = true;
        }

        if (redScore >= 1f)
        {
            redScore = 1f;
            matchFinished = true;
        }
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
            captureSlider.value = captureProgress;

        if (blueScoreSlider != null)
            blueScoreSlider.value = blueScore;

        if (redScoreSlider != null)
            redScoreSlider.value = redScore;
    }

    private void UpdateText()
    {
        if (blueScoreText != null)
            blueScoreText.text = $"BLUE {Mathf.RoundToInt(blueScore * 100f)}%";

        if (redScoreText != null)
            redScoreText.text = $"RED {Mathf.RoundToInt(redScore * 100f)}%";

        if (statusText == null)
            return;

        if (matchFinished)
        {
            statusText.text = blueScore >= 1f ? "BLUE WINS" : "RED WINS";
            return;
        }

        if (IsLocked)
        {
            float remaining = Mathf.Max(0f, unlockDelay - unlockTimer);
            statusText.text = $"POINT UNLOCKS IN {Mathf.CeilToInt(remaining)}";
            return;
        }

        bool bluePresent = bluePlayersInside.Count > 0;
        bool redPresent = redPlayersInside.Count > 0;

        if (bluePresent && redPresent)
        {
            statusText.text = "CONTESTED";
            return;
        }

        if (capturingTeam == OutpostTeam.Blue)
        {
            statusText.text = $"BLUE CAPTURING {Mathf.RoundToInt(captureProgress * 100f)}%";
            return;
        }

        if (capturingTeam == OutpostTeam.Red)
        {
            statusText.text = $"RED CAPTURING {Mathf.RoundToInt(captureProgress * 100f)}%";
            return;
        }

        if (owner == OutpostTeam.Blue)
        {
            statusText.text = "BLUE CONTROLS THE POINT";
            return;
        }

        if (owner == OutpostTeam.Red)
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

        if (owner == OutpostTeam.Blue && blueOwnedMaterial != null)
            targetMaterial = blueOwnedMaterial;
        else if (owner == OutpostTeam.Red && redOwnedMaterial != null)
            targetMaterial = redOwnedMaterial;
        else if (capturingTeam == OutpostTeam.Blue && blueCapturingMaterial != null)
            targetMaterial = blueCapturingMaterial;
        else if (capturingTeam == OutpostTeam.Red && redCapturingMaterial != null)
            targetMaterial = redCapturingMaterial;

        if (targetMaterial != null)
            beaconRenderer.sharedMaterial = targetMaterial;
    }

    private void UpdateCaptureLight()
    {
        if (captureLight == null)
            return;

        captureLight.enabled = owner != OutpostTeam.None || capturingTeam != OutpostTeam.None;

        if (owner == OutpostTeam.Blue || capturingTeam == OutpostTeam.Blue)
            captureLight.color = Color.blue;
        else if (owner == OutpostTeam.Red || capturingTeam == OutpostTeam.Red)
            captureLight.color = Color.red;
        else
            captureLight.color = Color.white;
    }
}