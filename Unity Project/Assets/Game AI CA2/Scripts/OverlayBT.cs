using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class OverlayBT : MonoBehaviour
{
    public static OverlayBT Instance { get; private set; }

    [Header("UI")]
    [SerializeField] TMP_Text behaviourText;
    [SerializeField] TMP_Text perceptionText;
    [SerializeField] TMP_Text blackboardText;
    [SerializeField] TMP_Text navText;

    [Header("Display")]
    [SerializeField] bool showWorldY = false;
    [SerializeField] string noneLabel = "None";
    [SerializeField] string unknownLabel = "-";

    BTSnapshot snapshot = new BTSnapshot();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        DrawBehaviour();
        DrawPerception();
        DrawBlackboard();
        DrawNav();
    }

    void DrawBehaviour()
    {
        if (!behaviourText)
            return;

        behaviourText.text =
            "=== BEHAVIOUR ===\n" +
            $"Branch: {ValueOrUnknown(snapshot.activeBranch)}\n" +
            $"Task: {ValueOrUnknown(snapshot.activeTask)}\n" +
            $"State: {ValueOrUnknown(snapshot.stateLabel)}\n" +
            $"Interrupt: {ValueOrUnknown(snapshot.lastInterrupt)}\n" +
            $"Recovery: {ValueOrUnknown(snapshot.lastRecovery)}";
    }

    void DrawPerception()
    {
        if (!perceptionText)
            return;

        perceptionText.text =
            "=== PERCEPTION ===\n" +
            $"Can See Player: {FormatBool(snapshot.canSeePlayer, snapshot.hasCanSeePlayer)}\n" +
            $"Hears Noise: {FormatBool(snapshot.hearsNoise, snapshot.hasHearsNoise)}\n" +
            $"Knows Last Pos: {FormatBool(snapshot.knowsLastPlayerPos, snapshot.hasKnowsLastPlayerPos)}\n" +
            $"Searching: {FormatBool(snapshot.isSearching, snapshot.hasIsSearching)}\n" +
            $"Has Target: {FormatBool(snapshot.hasTarget, snapshot.hasHasTarget)}\n" +
            $"Search Type: {ValueOrUnknown(snapshot.searchType)}";
    }

    void DrawBlackboard()
    {
        if (!blackboardText)
            return;

        string healthLine = "";

        if (snapshot.hasCurrentHealth || snapshot.hasMaxHealth || snapshot.hasLowHealthThreshold)
        {
            healthLine =
                "\n" +
                $"Health: {FormatFloat(snapshot.currentHealth, snapshot.hasCurrentHealth)} / " +
                $"{FormatFloat(snapshot.maxHealth, snapshot.hasMaxHealth)} " +
                $"(Low @ {FormatFloat(snapshot.lowHealthThreshold, snapshot.hasLowHealthThreshold)})";
        }

        blackboardText.text =
            "=== BLACKBOARD ===\n" +
            $"Target: {FormatTransform(snapshot.currentTarget)}\n" +
            $"Last Known Pos: {FormatVector(snapshot.lastKnownPlayerPos, snapshot.hasLastKnownPlayerPos)}\n" +
            $"Last Heard Noise: {FormatVector(snapshot.lastHeardNoisePos, snapshot.hasLastHeardNoisePos)}\n" +
            $"Search Position: {FormatVector(snapshot.searchPosition, snapshot.hasSearchPosition)}\n" +
            $"In Range: {FormatBool(snapshot.isInRange, snapshot.hasIsInRange)}\n" +
            $"Can Attack: {FormatBool(snapshot.canAttack, snapshot.hasCanAttack)}\n" +
            $"Low Health: {FormatBool(snapshot.isLowHealth, snapshot.hasIsLowHealth)}\n" +
            $"Fleeing: {FormatBool(snapshot.isFleeing, snapshot.hasIsFleeing)}\n" +
            $"Chase Locked: {FormatBool(snapshot.chaseLocked, snapshot.hasChaseLocked)}" +
            healthLine;
    }

    void DrawNav()
    {
        if (!navText)
            return;

        navText.text =
            "=== NAV ===\n" +
            $"Goal: {ValueOrUnknown(snapshot.goalLabel)}\n" +
            $"Has Path: {FormatBool(snapshot.hasPath, snapshot.hasHasPath)}\n" +
            $"Status: {FormatPathStatus(snapshot.pathStatus, snapshot.hasPathStatus)}\n" +
            $"Dest: {FormatVector(snapshot.destination, snapshot.hasDestination)}\n" +
            $"Remaining: {FormatFloat(snapshot.remainingDistance, snapshot.hasRemainingDistance)}\n" +
            $"Velocity: {FormatFloat(snapshot.velocity, snapshot.hasVelocity)}\n" +
            $"Stuck: {FormatBool(snapshot.isStuck, snapshot.hasIsStuck)}\n" +
            $"Recovery: {ValueOrUnknown(snapshot.lastRecovery)}";
    }

    string FormatTransform(Transform t)
    {
        if (!t)
            return noneLabel;

        return $"{t.name} @ {FormatVector(t.position, true)}";
    }

    string FormatVector(Vector3 value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return showWorldY
            ? $"{value.x:F1}, {value.y:F1}, {value.z:F1}"
            : $"{value.x:F1}, {value.z:F1}";
    }

    string FormatBool(bool value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return value ? "True" : "False";
    }

    string FormatFloat(float value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return value.ToString("F1");
    }

    string FormatPathStatus(NavMeshPathStatus value, bool hasValue)
    {
        if (!hasValue)
            return unknownLabel;

        return value.ToString();
    }

    string ValueOrUnknown(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? unknownLabel : value;
    }

    public void ClearRuntime()
    {
        snapshot.ResetRuntime();
    }

    public void SetBehaviour(string branch, string task = null, string state = null)
    {
        snapshot.activeBranch = branch;
        snapshot.activeTask = task;
        snapshot.stateLabel = state;
    }

    public void SetTarget(Transform target, bool? hasTarget = null)
    {
        snapshot.currentTarget = target;

        if (hasTarget.HasValue)
        {
            snapshot.hasTarget = hasTarget.Value;
            snapshot.hasHasTarget = true;
        }
    }

    public void SetPerception(bool? canSeePlayer = null, bool? hearsNoise = null, bool? knowsLastPlayerPos = null)
    {
        if (canSeePlayer.HasValue)
        {
            snapshot.canSeePlayer = canSeePlayer.Value;
            snapshot.hasCanSeePlayer = true;
        }

        if (hearsNoise.HasValue)
        {
            snapshot.hearsNoise = hearsNoise.Value;
            snapshot.hasHearsNoise = true;
        }

        if (knowsLastPlayerPos.HasValue)
        {
            snapshot.knowsLastPlayerPos = knowsLastPlayerPos.Value;
            snapshot.hasKnowsLastPlayerPos = true;
        }
    }

    public void SetCombat(bool? isInRange = null, bool? canAttack = null, bool? isLowHealth = null, bool? isFleeing = null, bool? chaseLocked = null)
    {
        if (isInRange.HasValue)
        {
            snapshot.isInRange = isInRange.Value;
            snapshot.hasIsInRange = true;
        }

        if (canAttack.HasValue)
        {
            snapshot.canAttack = canAttack.Value;
            snapshot.hasCanAttack = true;
        }

        if (isLowHealth.HasValue)
        {
            snapshot.isLowHealth = isLowHealth.Value;
            snapshot.hasIsLowHealth = true;
        }

        if (isFleeing.HasValue)
        {
            snapshot.isFleeing = isFleeing.Value;
            snapshot.hasIsFleeing = true;
        }

        if (chaseLocked.HasValue)
        {
            snapshot.chaseLocked = chaseLocked.Value;
            snapshot.hasChaseLocked = true;
        }
    }

    public void SetSearch(bool? isSearching = null, string searchType = null, Vector3? searchPosition = null)
    {
        if (isSearching.HasValue)
        {
            snapshot.isSearching = isSearching.Value;
            snapshot.hasIsSearching = true;
        }

        if (!string.IsNullOrWhiteSpace(searchType))
            snapshot.searchType = searchType;

        if (searchPosition.HasValue)
        {
            snapshot.searchPosition = searchPosition.Value;
            snapshot.hasSearchPosition = true;
        }
    }

    public void SetTracking(Vector3? lastKnownPlayerPos = null, Vector3? lastHeardNoisePos = null)
    {
        if (lastKnownPlayerPos.HasValue)
        {
            snapshot.lastKnownPlayerPos = lastKnownPlayerPos.Value;
            snapshot.hasLastKnownPlayerPos = true;
        }

        if (lastHeardNoisePos.HasValue)
        {
            snapshot.lastHeardNoisePos = lastHeardNoisePos.Value;
            snapshot.hasLastHeardNoisePos = true;
        }
    }

    public void SetHealth(float? current = null, float? max = null, float? lowThreshold = null)
    {
        if (current.HasValue)
        {
            snapshot.currentHealth = current.Value;
            snapshot.hasCurrentHealth = true;
        }

        if (max.HasValue)
        {
            snapshot.maxHealth = max.Value;
            snapshot.hasMaxHealth = true;
        }

        if (lowThreshold.HasValue)
        {
            snapshot.lowHealthThreshold = lowThreshold.Value;
            snapshot.hasLowHealthThreshold = true;
        }
    }

    public void SetInterrupt(string reason)
    {
        snapshot.lastInterrupt = reason;
    }

    public void SetRecovery(string recovery, bool? isStuck = null)
    {
        snapshot.lastRecovery = recovery;

        if (isStuck.HasValue)
        {
            snapshot.isStuck = isStuck.Value;
            snapshot.hasIsStuck = true;
        }
    }

    public void SetNav(
        string goal = null,
        bool? hasPath = null,
        NavMeshPathStatus? pathStatus = null,
        Vector3? destination = null,
        float? remainingDistance = null,
        float? velocity = null,
        bool? isStuck = null,
        string recovery = null)
    {
        if (!string.IsNullOrWhiteSpace(goal))
            snapshot.goalLabel = goal;

        if (hasPath.HasValue)
        {
            snapshot.hasPath = hasPath.Value;
            snapshot.hasHasPath = true;
        }

        if (pathStatus.HasValue)
        {
            snapshot.pathStatus = pathStatus.Value;
            snapshot.hasPathStatus = true;
        }

        if (destination.HasValue)
        {
            snapshot.destination = destination.Value;
            snapshot.hasDestination = true;
        }

        if (remainingDistance.HasValue)
        {
            snapshot.remainingDistance = remainingDistance.Value;
            snapshot.hasRemainingDistance = true;
        }

        if (velocity.HasValue)
        {
            snapshot.velocity = velocity.Value;
            snapshot.hasVelocity = true;
        }

        if (isStuck.HasValue)
        {
            snapshot.isStuck = isStuck.Value;
            snapshot.hasIsStuck = true;
        }

        if (!string.IsNullOrWhiteSpace(recovery))
            snapshot.lastRecovery = recovery;
    }

    [System.Serializable]
    public class BTSnapshot
    {
        public string activeBranch;
        public string activeTask;
        public string stateLabel;
        public string searchType;
        public string lastInterrupt;
        public string lastRecovery;
        public string goalLabel;

        public Transform currentTarget;

        public Vector3 lastKnownPlayerPos;
        public bool hasLastKnownPlayerPos;

        public Vector3 lastHeardNoisePos;
        public bool hasLastHeardNoisePos;

        public Vector3 searchPosition;
        public bool hasSearchPosition;

        public Vector3 destination;
        public bool hasDestination;

        public bool canSeePlayer;
        public bool hasCanSeePlayer;

        public bool hearsNoise;
        public bool hasHearsNoise;

        public bool knowsLastPlayerPos;
        public bool hasKnowsLastPlayerPos;

        public bool isSearching;
        public bool hasIsSearching;

        public bool hasTarget;
        public bool hasHasTarget;

        public bool isInRange;
        public bool hasIsInRange;

        public bool canAttack;
        public bool hasCanAttack;

        public bool isLowHealth;
        public bool hasIsLowHealth;

        public bool isFleeing;
        public bool hasIsFleeing;

        public bool chaseLocked;
        public bool hasChaseLocked;

        public bool hasPath;
        public bool hasHasPath;

        public bool isStuck;
        public bool hasIsStuck;

        public NavMeshPathStatus pathStatus;
        public bool hasPathStatus;

        public float currentHealth;
        public bool hasCurrentHealth;

        public float maxHealth;
        public bool hasMaxHealth;

        public float lowHealthThreshold;
        public bool hasLowHealthThreshold;

        public float remainingDistance;
        public bool hasRemainingDistance;

        public float velocity;
        public bool hasVelocity;

        public void ResetRuntime()
        {
            activeBranch = null;
            activeTask = null;
            stateLabel = null;
            searchType = null;
            lastInterrupt = null;
            lastRecovery = null;
            goalLabel = null;
            currentTarget = null;

            hasLastKnownPlayerPos = false;
            hasLastHeardNoisePos = false;
            hasSearchPosition = false;
            hasDestination = false;

            hasCanSeePlayer = false;
            hasHearsNoise = false;
            hasKnowsLastPlayerPos = false;
            hasIsSearching = false;
            hasHasTarget = false;
            hasIsInRange = false;
            hasCanAttack = false;
            hasIsLowHealth = false;
            hasIsFleeing = false;
            hasChaseLocked = false;

            hasHasPath = false;
            hasIsStuck = false;
            hasPathStatus = false;

            hasCurrentHealth = false;
            hasMaxHealth = false;
            hasLowHealthThreshold = false;
            hasRemainingDistance = false;
            hasVelocity = false;
        }
    }
}