using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CustomPatrolAction", story: "[Target] patrols [points]", category: "Action", id: "573d3f416e28e1aaafb275c9e77c15c3")]
public class PatrolStateAction : Action
{
    [SerializeReference] public BlackboardVariable<State> AIState;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<Vector3>> Waypoints;

    [SerializeField] private float waitAtPoint = 0.5f;
    [SerializeField] private bool startAtNearestPoint = true;

    private NavMeshAgent nav;
    private Animator anim;

    private int index;
    private float waitTimer;
    private bool waiting;
    private bool initialised;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        anim = Agent.Value.GetComponent<Animator>();

        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        if (Waypoints == null || Waypoints.Value == null || Waypoints.Value.Count == 0)
            return Status.Failure;

        if (!initialised)
        {
            if (startAtNearestPoint)
                index = GetNearestWaypointIndex();
            else
                index = Mathf.Clamp(index, 0, Waypoints.Value.Count - 1);

            waiting = false;
            waitTimer = 0f;
            initialised = true;

            SetDestinationToCurrentWaypoint();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (nav == null || !nav.isOnNavMesh)
            return Status.Failure;

        if (Waypoints == null || Waypoints.Value == null || Waypoints.Value.Count == 0)
            return Status.Failure;


        if (waiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                waiting = false;
                MoveToNextWaypoint();
            }

            return Status.Success;
        }

        if (!nav.hasPath && !nav.pathPending)
        {
            SetDestinationToCurrentWaypoint();
        }

        if (HasReachedDestination())
        {
            waiting = true;
            waitTimer = waitAtPoint;

            nav.isStopped = true;

            if (anim != null)
                anim.SetFloat("Speed", 0f);

            return Status.Success;
        }

        nav.isStopped = false;

        if (anim != null)
            anim.SetFloat("Speed", nav.velocity.magnitude);

        return Status.Success;
    }

    protected override void OnEnd()
    {
        // Intentionally empty.
        // This action returns Success every tick so the Behaviour Graph can re-evaluate senses.
        // Do not stop or reset the NavMeshAgent here, or patrol movement will be cancelled every tick.
    }

    private void MoveToNextWaypoint()
    {
        index = (index + 1) % Waypoints.Value.Count;

        nav.isStopped = false;
        SetDestinationToCurrentWaypoint();

        if (anim != null)
            anim.SetFloat("Speed", 0.5f);
    }

    private void SetDestinationToCurrentWaypoint()
    {
        Vector3 destination = Waypoints.Value[index];

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            nav.SetDestination(hit.position);
        else
            nav.SetDestination(destination);
    }

    private bool HasReachedDestination()
    {
        if (nav.pathPending)
            return false;

        if (nav.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            MoveToNextWaypoint();
            return false;
        }

        if (nav.remainingDistance > nav.stoppingDistance)
            return false;

        if (nav.hasPath && nav.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }

    private int GetNearestWaypointIndex()
    {
        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < Waypoints.Value.Count; i++)
        {
            float distance = Vector3.Distance(
                Agent.Value.transform.position,
                Waypoints.Value[i]
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
}