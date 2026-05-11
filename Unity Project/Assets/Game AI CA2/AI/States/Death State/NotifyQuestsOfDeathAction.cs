using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "NotifyQuestsOfDeath", story: "Notifies QuestSystem of Death", category: "Action", id: "6f7a1abc214ddcfdddfe579390f12356")]
public partial class NotifyQuestsOfDeathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
            return Status.Failure;

        Agent.Value.GetComponent<Monster>().Die();

        return Status.Success;
    }
}