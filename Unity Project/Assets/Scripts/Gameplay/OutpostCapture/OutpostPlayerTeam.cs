using UnityEngine;

public enum OutpostTeam
{
    None,
    Blue,
    Red,
    Neutral
}

public class OutpostPlayerTeam : MonoBehaviour
{
    [field: SerializeField] public OutpostTeam Team { get; private set; } = OutpostTeam.Blue;

    public void SetTeam(OutpostTeam team)
    {
        Team = team;
    }
}