using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Stop Movement", story: "Disable Navmesh Agent", category: "Action", id: "f69007a8b2ff90c7b1b8664b850a88be")]
public partial class StopMovementAction : Action
{
    private NavMeshAgent agent;

    protected override Status OnStart()
    {
        agent = GameObject.GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        return Status.Success;
    }
}

