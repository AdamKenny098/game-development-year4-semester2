using UnityEngine;

public class HearingGizmo : MonoBehaviour
{
    public float hearingRange = 30f;
    public Color gizmoColor = Color.yellow;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
    }
}
