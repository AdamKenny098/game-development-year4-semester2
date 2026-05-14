using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class OverlayBT : MonoBehaviour
{
    public static OverlayBT Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TMP_Text behaviourText;
    [SerializeField] private TMP_Text perceptionText;
    [SerializeField] private TMP_Text blackboardText;
    [SerializeField] private TMP_Text navText;

    [Header("Display")]
    [SerializeField] private bool showWorldY = false;
    [SerializeField] private string noneLabel = "None";
    [SerializeField] private string unknownLabel = "-";

    private string stateLabel;
    private string activeBranch;
    private string activeTask;
    private string goalLabel;

    private bool canSeePlayer;
    private bool hearsNoise;
    private bool hasLastKnownPlayerPosition;
    private bool hasLastHeardNoisePosition;

    private bool hasCanSeePlayer;
    private bool hasHearsNoise;
    private bool hasLastKnownFlag;
    private bool hasLastHeardFlag;

    private Transform currentTarget;
    private bool hasTarget;
    private bool hasTargetFlag;

    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastHeardNoisePosition;
    private Vector3 searchTargetPosition;

    private bool hasLastKnownPositionValue;
    private bool hasLastHeardPositionValue;
    private bool hasSearchTargetValue;

    private bool isInRange;
    private bool canAttack;
    private bool isAttacking;
    private bool isLowHealth;
    private bool isFleeing;
    private bool chaseLocked;

    private bool hasIsInRange;
    private bool hasCanAttack;
    private bool hasIsAttacking;
    private bool hasIsLowHealth;
    private bool hasIsFleeing;
    private bool hasChaseLocked;

    private float currentHealth;
    private float maxHealth;
    private bool hasCurrentHealth;
    private bool hasMaxHealth;

    private bool hasNavData;
    private bool navHasPath;
    private NavMeshPathStatus navPathStatus;
    private Vector3 navDestination;
    private float navRemainingDistance;
    private float navVelocity;
    private bool navIsStopped;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        DrawBehaviour();
        DrawPerception();
        DrawBlackboard();
        DrawNav();
    }

    private void DrawBehaviour()
    {
        if (!behaviourText)
            return;

        behaviourText.text =
            "=== TYRANT BEHAVIOUR ===\n" +
            $"State: {ValueOrUnknown(stateLabel)}\n" +
            $"Branch: {ValueOrUnknown(activeBranch)}\n" +
            $"Task: {ValueOrUnknown(activeTask)}";
    }

    private void DrawPerception()
    {
        if (!perceptionText)
            return;

        perceptionText.text =
            "=== PERCEPTION ===\n" +
            $"Can See Player: {FormatBool(canSeePlayer, hasCanSeePlayer)}\n" +
            $"Hears Noise: {FormatBool(hearsNoise, hasHearsNoise)}\n" +
            $"Has Last Known Pos: {FormatBool(hasLastKnownPlayerPosition, hasLastKnownFlag)}\n" +
            $"Has Last Heard Pos: {FormatBool(hasLastHeardNoisePosition, hasLastHeardFlag)}";
    }

    private void DrawBlackboard()
    {
        if (!blackboardText)
            return;

        string healthLine = "";

        if (hasCurrentHealth || hasMaxHealth)
        {
            healthLine =
                "\n" +
                $"Health: {FormatFloat(currentHealth, hasCurrentHealth)} / {FormatFloat(maxHealth, hasMaxHealth)}";
        }

        blackboardText.text =
            "=== BLACKBOARD ===\n" +
            $"Target: {FormatTarget()}\n" +
            $"Last Known Player: {FormatVector(lastKnownPlayerPosition, hasLastKnownPositionValue)}\n" +
            $"Last Heard Noise: {FormatVector(lastHeardNoisePosition, hasLastHeardPositionValue)}\n" +
            $"Search Target: {FormatVector(searchTargetPosition, hasSearchTargetValue)}\n" +
            $"In Range: {FormatBool(isInRange, hasIsInRange)}\n" +
            $"Can Attack: {FormatBool(canAttack, hasCanAttack)}\n" +
            $"Is Attacking: {FormatBool(isAttacking, hasIsAttacking)}\n" +
            $"Low Health: {FormatBool(isLowHealth, hasIsLowHealth)}\n" +
            $"Fleeing: {FormatBool(isFleeing, hasIsFleeing)}\n" +
            $"Chase Locked: {FormatBool(chaseLocked, hasChaseLocked)}" +
            healthLine;
    }

    private void DrawNav()
    {
        if (!navText)
            return;

        navText.text =
            "=== NAVMESH ===\n" +
            $"Goal: {ValueOrUnknown(goalLabel)}\n" +
            $"Has Path: {(hasNavData ? FormatBool(navHasPath, true) : unknownLabel)}\n" +
            $"Path Status: {(hasNavData ? navPathStatus.ToString() : unknownLabel)}\n" +
            $"Destination: {FormatVector(navDestination, hasNavData)}\n" +
            $"Remaining: {(hasNavData ? navRemainingDistance.ToString("F1") : unknownLabel)}\n" +
            $"Velocity: {(hasNavData ? navVelocity.ToString("F1") : unknownLabel)}\n" +
            $"Stopped: {(hasNavData ? FormatBool(navIsStopped, true) : unknownLabel)}";
    }

    public void ClearRuntime()
    {
        stateLabel = null;
        activeBranch = null;
        activeTask = null;
        goalLabel = null;

        hasCanSeePlayer = false;
        hasHearsNoise = false;
        hasLastKnownFlag = false;
        hasLastHeardFlag = false;

        currentTarget = null;
        hasTarget = false;
        hasTargetFlag = false;

        hasLastKnownPositionValue = false;
        hasLastHeardPositionValue = false;
        hasSearchTargetValue = false;

        hasIsInRange = false;
        hasCanAttack = false;
        hasIsAttacking = false;
        hasIsLowHealth = false;
        hasIsFleeing = false;
        hasChaseLocked = false;

        hasCurrentHealth = false;
        hasMaxHealth = false;
        hasNavData = false;
    }

    public void SetBehaviour(State state, string branch = null, string task = null)
    {
        stateLabel = state.ToString();
        activeBranch = branch;
        activeTask = task;
    }

    public void SetBehaviour(string branch, string task = null, string state = null)
    {
        activeBranch = branch;
        activeTask = task;
        stateLabel = state;
    }

    public void SetPerception(
        bool? canSeePlayer = null,
        bool? hearsNoise = null,
        bool? knowsLastPlayerPos = null,
        bool? hasLastKnownPlayerPosition = null,
        bool? hasLastHeardNoisePosition = null)
    {
        if (canSeePlayer.HasValue)
        {
            this.canSeePlayer = canSeePlayer.Value;
            hasCanSeePlayer = true;
        }

        if (hearsNoise.HasValue)
        {
            this.hearsNoise = hearsNoise.Value;
            hasHearsNoise = true;
        }

        if (knowsLastPlayerPos.HasValue)
        {
            this.hasLastKnownPlayerPosition = knowsLastPlayerPos.Value;
            hasLastKnownFlag = true;
        }

        if (hasLastKnownPlayerPosition.HasValue)
        {
            this.hasLastKnownPlayerPosition = hasLastKnownPlayerPosition.Value;
            hasLastKnownFlag = true;
        }

        if (hasLastHeardNoisePosition.HasValue)
        {
            this.hasLastHeardNoisePosition = hasLastHeardNoisePosition.Value;
            hasLastHeardFlag = true;
        }
    }

    public void SetTarget(Transform target, bool? hasTarget = null)
    {
        currentTarget = target;

        if (hasTarget.HasValue)
        {
            this.hasTarget = hasTarget.Value;
            hasTargetFlag = true;
        }
        else
        {
            this.hasTarget = target != null;
            hasTargetFlag = true;
        }
    }

    public void SetTracking(
        Vector3? lastKnownPlayerPosition = null,
        Vector3? lastHeardNoisePosition = null,
        Vector3? searchTargetPosition = null,
        Vector3? lastKnownPlayerPos = null,
        Vector3? lastHeardNoisePos = null)
    {
        if (lastKnownPlayerPosition.HasValue)
        {
            this.lastKnownPlayerPosition = lastKnownPlayerPosition.Value;
            hasLastKnownPositionValue = true;
        }

        if (lastKnownPlayerPos.HasValue)
        {
            this.lastKnownPlayerPosition = lastKnownPlayerPos.Value;
            hasLastKnownPositionValue = true;
        }

        if (lastHeardNoisePosition.HasValue)
        {
            this.lastHeardNoisePosition = lastHeardNoisePosition.Value;
            hasLastHeardPositionValue = true;
        }

        if (lastHeardNoisePos.HasValue)
        {
            this.lastHeardNoisePosition = lastHeardNoisePos.Value;
            hasLastHeardPositionValue = true;
        }

        if (searchTargetPosition.HasValue)
        {
            this.searchTargetPosition = searchTargetPosition.Value;
            hasSearchTargetValue = true;
        }
    }

    public void SetCombat(
        bool? isInRange = null,
        bool? canAttack = null,
        bool? isAttacking = null,
        bool? isLowHealth = null,
        bool? isFleeing = null,
        bool? chaseLocked = null)
    {
        if (isInRange.HasValue)
        {
            this.isInRange = isInRange.Value;
            hasIsInRange = true;
        }

        if (canAttack.HasValue)
        {
            this.canAttack = canAttack.Value;
            hasCanAttack = true;
        }

        if (isAttacking.HasValue)
        {
            this.isAttacking = isAttacking.Value;
            hasIsAttacking = true;
        }

        if (isLowHealth.HasValue)
        {
            this.isLowHealth = isLowHealth.Value;
            hasIsLowHealth = true;
        }

        if (isFleeing.HasValue)
        {
            this.isFleeing = isFleeing.Value;
            hasIsFleeing = true;
        }

        if (chaseLocked.HasValue)
        {
            this.chaseLocked = chaseLocked.Value;
            hasChaseLocked = true;
        }
    }

    public void SetSearch(bool? isSearching = null, string searchType = null, Vector3? searchPosition = null)
    {
        if (!string.IsNullOrWhiteSpace(searchType))
            goalLabel = searchType;

        if (searchPosition.HasValue)
        {
            searchTargetPosition = searchPosition.Value;
            hasSearchTargetValue = true;
        }
    }

    public void SetHealth(float? current = null, float? max = null, float? lowHealthThreshold = null)
    {
        if (current.HasValue)
        {
            currentHealth = current.Value;
            hasCurrentHealth = true;
        }

        if (max.HasValue)
        {
            maxHealth = max.Value;
            hasMaxHealth = true;
        }
    }

    public void SetGoal(string label)
    {
        goalLabel = label;
    }

    public void SetGoal(Vector3 destination)
    {
        navDestination = destination;
        hasNavData = true;
    }

    public void SetGoal(string label, Vector3 destination)
    {
        goalLabel = label;
        navDestination = destination;
        hasNavData = true;
    }

    public void SetNav(NavMeshAgent agent)
    {
        if (agent == null || !agent.isOnNavMesh)
        {
            hasNavData = false;
            return;
        }

        hasNavData = true;
        navHasPath = agent.hasPath;
        navPathStatus = agent.pathStatus;
        navDestination = agent.destination;
        navRemainingDistance = agent.remainingDistance;
        navVelocity = agent.velocity.magnitude;
        navIsStopped = agent.isStopped;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        hasTarget = false;
        hasTargetFlag = true;
    }

    private string FormatTarget()
    {
        if (hasTargetFlag && !hasTarget)
            return noneLabel;

        if (!currentTarget)
            return noneLabel;

        return $"{currentTarget.name} @ {FormatVector(currentTarget.position, true)}";
    }

    private string FormatVector(Vector3 value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return showWorldY
            ? $"{value.x:F1}, {value.y:F1}, {value.z:F1}"
            : $"{value.x:F1}, {value.z:F1}";
    }

    private string FormatBool(bool value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return value ? "True" : "False";
    }

    private string FormatFloat(float value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return value.ToString("F1");
    }

    private string ValueOrUnknown(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? unknownLabel : value;
    }
}
