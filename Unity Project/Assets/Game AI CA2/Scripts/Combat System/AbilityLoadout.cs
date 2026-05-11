using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Ability Loadout")]
public class AbilityLoadout : ScriptableObject
{
    public AbilityData primary;
    public AbilityData secondary;
    public AbilityData unlock1;
    public AbilityData unlock2;
    public AbilityData unlock3;
}
