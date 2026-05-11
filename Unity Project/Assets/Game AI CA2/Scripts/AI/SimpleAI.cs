using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Attack }
    public State state = State.Patrol;
    public Transform player;
    public float detectRange = 12f;
    public float loseRange = 18f;
    public float attackRange = 2f;
    public float attackCooldown = 1.0f;
    public int attackDamage = 10;
    public float faceTargetSpeed = 10f;

    public NavMeshAgent agent;
    public int patrolIndex = 0;
    public float waitTimer = 0f;
    public float attackTimer = 0f;

    [Header("Patrol Default and Random")]
    public bool useRandomPatrol = true;
    public Transform[] patrolPoints;
    public float patrolWaitTime = 1.5f;
    public float wanderRadius = 10f;
    public int newPosForWander = 12;
    public bool aiEnabled = false;


    public void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {   
        if (!aiEnabled) return;
        if (!agent.isOnNavMesh) return;


        ResolvePlayerIfNeeded();
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                if (dist <= detectRange)
                {
                    SetState(State.Chase);
                } 
                else if (patrolPoints != null && patrolPoints.Length > 0)
                {
                    SetState(State.Patrol);
                } 
                break;

            case State.Patrol:
                if (dist <= detectRange)
                {
                    SetState(State.Chase);
                } 
                break;

            case State.Chase:
                if (dist <= attackRange)
                {
                    SetState(State.Attack);
                } 
                else if (dist >= loseRange)
                {
                    SetState((patrolPoints != null && patrolPoints.Length > 0) ? State.Patrol : State.Idle);
                } 
                break;

            case State.Attack:
                if (dist > attackRange)
                {
                    SetState(State.Chase);
                } 
                break;
        }

        switch (state)
        {
            case State.Idle:   
                TickIdle(); 
                break;
            case State.Patrol: 
                TickPatrol(); 
                break;

            case State.Chase:  
                TickChase(); 
                break;

            case State.Attack: 
                TickAttack(); 
                break;
        }
    }

    void SetState(State newState)
    {
        if (state == newState) return;
        state = newState;

        switch (state)
        {
            case State.Idle:
                agent.isStopped = true;
                break;

            case State.Patrol:
            agent.isStopped = false;
            waitTimer = patrolWaitTime; // Instantly pick a point


            // The below code is from the old patrol system but I am keeping it commented out in case I want a hybrid system.
            // Also it will be useful for other projects :)


            // if (!useRandomPatrol) 
            // {
            //     GoToNextPatrolPoint();
            // }
                break;


            case State.Chase:
                agent.isStopped = false;
                break;

            case State.Attack:
                agent.isStopped = true;
                attackTimer = 0f;
                break;
        }
    }

    void TickIdle()
    {
        //Nothing for now
    }

    void TickPatrol()
    {
        if (useRandomPatrol)
        {
            TickRandomWander();
            return;
        }

        // Old behavior
        // if (patrolPoints == null || patrolPoints.Length == 0)
        // {
        //     SetState(State.Idle);
        //     return;
        // }

        // if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        // {
        //     waitTimer += Time.deltaTime;
        //     if (waitTimer >= patrolWaitTime)
        //     {
        //         waitTimer = 0f;
        //         patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        //         GoToNextPatrolPoint();
        //     }
        // }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (patrolPoints[patrolIndex] == null) return;

        agent.SetDestination(patrolPoints[patrolIndex].position);
    }

    void TickChase()
    {
        agent.SetDestination(player.position);
    }

    void TickAttack()
    {
        // Face the player
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, faceTargetSpeed * Time.deltaTime);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            Debug.Log($"{name} attacked {player.name} for {attackDamage} damage");
        }
    }

    public void TickRandomWander()
    {
        bool arrived = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        if (arrived)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                waitTimer = 0f;
                if (TrySetRandomDestination())
                {
                    //returns a destination but idrk what to put here
                }
                else
                {
                    // fails and goes again
                }
            }
        }
    }

    public bool TrySetRandomDestination()
    {
        Vector3 origin = transform.position;

        for (int i = 0; i < newPosForWander; i++)
        {
            //random point in sphere of given radius
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir.y = 0f; // keep on horizontal plane

            Vector3 candidate = origin + randomDir;

            // Snap candidate onto nearest navmesh point
            // https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMesh.SamplePosition.html
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                // Check if path is valid
                // https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshAgent.CalculatePath.html
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    return true;
                }
            }
        }

        return false;
    }

    public void ResolvePlayerIfNeeded()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    public void EnableAI()
    {
        aiEnabled = true;
    }

    public void RebindToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }




}
