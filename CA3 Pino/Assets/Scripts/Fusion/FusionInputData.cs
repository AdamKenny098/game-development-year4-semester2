using Fusion;
using UnityEngine;

public struct FusionInputData : INetworkInput
{
    public Vector2 move;
    public NetworkBool interact;
}