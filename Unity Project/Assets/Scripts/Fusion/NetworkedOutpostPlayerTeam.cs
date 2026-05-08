using Fusion;
using UnityEngine;

[RequireComponent(typeof(OutpostPlayerTeam))]
public class NetworkedOutpostPlayerTeam : NetworkBehaviour
{
    [Networked] public int TeamId { get; set; }

    private OutpostPlayerTeam localTeam;

    private void Awake()
    {
        localTeam = GetComponent<OutpostPlayerTeam>();
    }

    public void SetTeam(OutpostTeam team)
    {
        if (!Object.HasStateAuthority)
            return;

        TeamId = (int)team;
        ApplyTeam();
    }

    public override void Spawned()
    {
        ApplyTeam();
    }

    public override void Render()
    {
        ApplyTeam();
    }

    private void ApplyTeam()
    {
        if (localTeam == null)
            localTeam = GetComponent<OutpostPlayerTeam>();

        localTeam.SetTeam((OutpostTeam)TeamId);
    }
}