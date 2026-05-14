using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class TyrantOverlayReporter : MonoBehaviour
{
    public static TyrantOverlayReporter Instance { get; private set; }

    [Header("References")]
    [SerializeField] private NavMeshAgent navAgent;

    [Header("Debug Gizmos")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private float markerSize = 0.35f;

    private State currentState = State.Patrol;
    private string currentBranch = "Patrol";
    private string currentTask = "Waypoint Patrol";

    private bool canSeePlayer;
    private bool hearsNoise;
    private bool hasLastKnownPlayerPosition;
    private bool hasLastHeardNoisePosition;

    private Transform currentTarget;

    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastHeardNoisePosition;
    private Vector3 searchTargetPosition;

    private bool hasLastKnownValue;
    private bool hasLastHeardValue;
    private bool hasSearchTargetValue;

    private bool isInRange;
    private bool canAttack;
    private bool isAttacking;

    private void Awake()
    {
        Instance = this;

        if (navAgent == null)
            navAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (OverlayBT.Instance == null)
            return;

        OverlayBT.Instance.SetBehaviour(currentState, currentBranch, currentTask);

        OverlayBT.Instance.SetPerception(
            canSeePlayer: canSeePlayer,
            hearsNoise: hearsNoise,
            hasLastKnownPlayerPosition: hasLastKnownPlayerPosition,
            hasLastHeardNoisePosition: hasLastHeardNoisePosition
        );

        OverlayBT.Instance.SetTarget(currentTarget);

        OverlayBT.Instance.SetTracking(
            lastKnownPlayerPosition: hasLastKnownValue ? (Vector3?)lastKnownPlayerPosition : null,
            lastHeardNoisePosition: hasLastHeardValue ? (Vector3?)lastHeardNoisePosition : null,
            searchTargetPosition: hasSearchTargetValue ? (Vector3?)searchTargetPosition : null
        );

        OverlayBT.Instance.SetCombat(
            isInRange: isInRange,
            canAttack: canAttack,
            isAttacking: isAttacking
        );

        OverlayBT.Instance.SetNav(navAgent);
    }

    public void ReportBehaviour(State state, string branch, string task)
    {
        currentState = state;
        currentBranch = branch;
        currentTask = task;
    }

    public void ReportVision(bool canSee, bool hasLastKnown)
    {
        canSeePlayer = canSee;
        hasLastKnownPlayerPosition = hasLastKnown;
    }

    public void ReportHearing(bool hears, bool hasLastHeard)
    {
        hearsNoise = hears;
        hasLastHeardNoisePosition = hasLastHeard;
    }

    public void ReportTarget(Transform target)
    {
        currentTarget = target;
    }

    public void ReportLastKnownPlayerPosition(Vector3 position)
    {
        lastKnownPlayerPosition = position;
        hasLastKnownValue = true;
        hasLastKnownPlayerPosition = true;
    }

    public void ReportLastHeardNoisePosition(Vector3 position)
    {
        lastHeardNoisePosition = position;
        hasLastHeardValue = true;
        hasLastHeardNoisePosition = true;
    }

    public void ReportSearchTargetPosition(Vector3 position)
    {
        searchTargetPosition = position;
        hasSearchTargetValue = true;
    }

    public void ReportCombat(bool inRange, bool attackPossible, bool attacking)
    {
        isInRange = inRange;
        canAttack = attackPossible;
        isAttacking = attacking;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    public void ClearSearchMemory()
    {
        hasLastKnownPlayerPosition = false;
        hasLastHeardNoisePosition = false;
        hasLastKnownValue = false;
        hasLastHeardValue = false;
        hasSearchTargetValue = false;
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (hasLastKnownValue)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, markerSize);
        }

        if (hasLastHeardValue)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastHeardNoisePosition, markerSize);
        }

        if (hasSearchTargetValue)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(searchTargetPosition, markerSize);
        }

        if (navAgent != null && navAgent.isOnNavMesh)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, navAgent.destination);
            Gizmos.DrawWireSphere(navAgent.destination, markerSize);
        }
    }
}
