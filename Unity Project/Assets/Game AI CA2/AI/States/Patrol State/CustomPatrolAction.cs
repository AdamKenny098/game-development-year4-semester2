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
    NavMeshAgent nav;
    PatrolPoints patrol;
    int index;
    Animator anim;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
            return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        patrol = Agent.Value.GetComponent<PatrolPoints>();
        anim = Agent.Value.GetComponent<Animator>();

        if (nav == null || patrol == null || patrol.points.Count == 0)
            return Status.Failure;

        nav.isStopped = false;
        index = 0;
        nav.SetDestination(patrol.points[index]);
        anim.SetFloat("Speed", 0.5f);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (AIState.Value != State.Patrol)
        {
            nav.ResetPath();
            return Status.Failure;
        }

        if (!nav.pathPending && nav.remainingDistance <= nav.stoppingDistance)
        {
            index = (index + 1) % patrol.points.Count;
            nav.SetDestination(patrol.points[index]);
        }
        return Status.Running;
    }
}