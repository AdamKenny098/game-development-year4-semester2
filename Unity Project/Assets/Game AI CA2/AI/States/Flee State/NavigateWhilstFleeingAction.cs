using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NavigateWhilstFleeing", story: "Agent flees towards target", category: "Action", id: "20dc4bac5cf0814e5d54ea550d1dca65")]
public partial class NavigateWhileFleeingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> FleeTarget;
    [SerializeReference] public BlackboardVariable<bool> IsFleeing;
    [SerializeReference] public BlackboardVariable<float> PanicTimeRemaining;
    [SerializeReference] public BlackboardVariable<float> FleeSpeed = new(6f);
    [SerializeReference] public BlackboardVariable<float> FleeDistance = new(10f);

    NavMeshAgent nav;
    Animator anim;
    protected override Status OnStart()
    {
        if (Agent?.Value == null) return Status.Failure;

        nav = Agent.Value.GetComponent<NavMeshAgent>();
        if (nav == null || !nav.isOnNavMesh) return Status.Failure;

        nav.isStopped = false;
        nav.speed = FleeSpeed.Value;

        anim = Agent.Value.GetComponent<Animator>();
        anim.SetFloat("Speed", 1.0f);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!IsFleeing.Value || PanicTimeRemaining.Value <= 0f) return Status.Failure;
        if (Agent == null || Agent.Value == null) return Status.Failure;
        if (FleeTarget == null) return Status.Failure;
        if (FleeTarget.Value == null) return Status.Running;


        if (nav == null || !nav.isOnNavMesh) return Status.Failure;
        bool pathSet = nav.SetDestination(FleeTarget.Value.position);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (nav && nav.isOnNavMesh && !IsFleeing.Value)
            nav.ResetPath();
    }
}