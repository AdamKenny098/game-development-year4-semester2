using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimatorDriver : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;

    [Header("Parameters")]
    public string moveBool = "IsMoving";
    public string attackInt = "Attack";

    [Header("Tuning")]
    public float moveThreshold = 0.1f;

    void Awake()
    {
        if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInParent<Animator>();
    }

    void Update()
    {
        if (agent == null || anim == null) return;

        bool attacking = anim.GetInteger(attackInt) != 0;

        bool isMoving =
            !attacking &&
            !agent.isStopped &&
            agent.remainingDistance > agent.stoppingDistance &&
            agent.velocity.magnitude > moveThreshold;

        anim.SetBool(moveBool, isMoving);
    }
}