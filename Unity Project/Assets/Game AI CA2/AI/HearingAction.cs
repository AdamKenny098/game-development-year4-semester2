using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Hearing", story: "Detects nearby Audio sources", category: "Action", id: "bc709b4ddc97364aa346fd3c1941bb73")]
public partial class HearingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<bool> HearsNoise;
    [SerializeReference] public BlackboardVariable<Vector3> LastHeardNoisePosition;

    public float hearingRadius = 30f;
    public float memoryTime = 3f;

    float lastHeardTime;

    protected override Status OnUpdate()
    {
        if (Agent?.Value == null)
            return Status.Running;

        bool heard = ScanForAudio();

        OverlayBT.Instance?.SetPerception(
            hearsNoise: heard
        );

        OverlayBT.Instance?.SetTracking(
            lastHeardNoisePos: LastHeardNoisePosition != null ? LastHeardNoisePosition.Value : (Vector3?)null
        );

        if (heard)
        {
            if (HearsNoise != null)
                HearsNoise.Value = true;

            lastHeardTime = Time.time;
            return Status.Running;
        }

        if (HearsNoise != null && HearsNoise.Value && Time.time - lastHeardTime > memoryTime)
            HearsNoise.Value = false;

        return Status.Running;
    }

    bool ScanForAudio()
    {
        Collider[] hits = Physics.OverlapSphere(
            Agent.Value.transform.position,
            hearingRadius
        );

        foreach (Collider hit in hits)
        {
            AudioSource audio = hit.GetComponentInChildren<AudioSource>();
            if (audio == null || !audio.isPlaying)
                continue;

            if (LastHeardNoisePosition != null)
                LastHeardNoisePosition.Value = audio.transform.position;

            Debug.DrawLine(
                Agent.Value.transform.position,
                audio.transform.position,
                Color.yellow,
                0.1f
            );

            Debug.Log($"[Hearing] Heard sound from {audio.gameObject.name} at {audio.transform.position}");

            return true;
        }

        return false;
    }
}