using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolPoints : MonoBehaviour
{
    [SerializeField] private float patrolRadius = 12f;
    [SerializeField] private float minDistanceBetweenPoints = 3f;
    [SerializeField] private int pointCount = 4;
    [SerializeField] private int maxAttemptsPerPoint = 20;

    public List<Vector3> points = new();
    IEnumerator Start()
    {
        yield return null; // allow NavMesh + AI to initialize
        Generate();
    }

    void Generate()
    {
        points.Clear();
        Vector3 origin = transform.position;
        for (int i = 0; i < pointCount; i++)
        {
            if (TryFindValidPoint(origin, points, out Vector3 p))
            {
                points.Add(p);
            }
            else
            {
                points.Add(origin);
            }
        }
    }

    bool TryFindValidPoint(Vector3 origin, List<Vector3> existing, out Vector3 result)
    {
        for (int attempt = 0; attempt < maxAttemptsPerPoint; attempt++)
        {
            Vector3 random = origin + Random.insideUnitSphere * patrolRadius;
            random.y = origin.y;

            if (!NavMesh.SamplePosition(random, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                continue;

            if (TooClose(hit.position, existing))
                continue;

            NavMeshPath path = new NavMeshPath();
            if (!NavMesh.CalculatePath(origin, hit.position, NavMesh.AllAreas, path) ||
                path.status != NavMeshPathStatus.PathComplete)
                continue;

            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    bool TooClose(Vector3 candidate, List<Vector3> existing)
    {
        foreach (var p in existing)
        {
            if (Vector3.Distance(candidate, p) < minDistanceBetweenPoints)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var p in points)
            Gizmos.DrawSphere(p, 0.25f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }
#endif
}
