using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Play Death Animation", story: "Play death animation", category: "Action", id: "8f12ffe29c0ef6e5291c94aae8202b04")]
public partial class PlayDeathAnimationAction : Action
{

    private Animator animator;
    private bool triggered;

    protected override Status OnStart()
    {
        animator = GameObject.GetComponent<Animator>();

        if (animator != null && !triggered)
        {
            animator.SetTrigger("Die");
            triggered = true;
        }

        return Status.Success;
    }
}

