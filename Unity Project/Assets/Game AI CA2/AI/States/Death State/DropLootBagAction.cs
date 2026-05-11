using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Drop Loot Bag", story: "Drops a loot bag", category: "Action", id: "62a91ee7060e73d96bd43e13de2122fa")]
public partial class DropLootBagAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> DropLoot;
    protected override Status OnStart()
    {
        return Status.Running;
    }
}

