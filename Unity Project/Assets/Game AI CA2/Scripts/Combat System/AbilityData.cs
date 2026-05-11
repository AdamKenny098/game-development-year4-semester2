using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName = "Basic Attack";
    public AbilityResolution resolution = AbilityResolution.AttackRoll;
    public DamageType damageType = DamageType.Slashing;
    public string damageDice = "1d8+STR";
    public bool usesSpellAttackBonus = false; 
    public SaveType saveType = SaveType.Dexterity;
    public bool halfDamageOnSave = true;
    public int staminaCost = 0;
    public int manaCost = 0;
    public float cooldown = 0.6f;
    public float range = 2.2f;
    public float radius = 0.75f;
    public Sprite icon;
    public int requiredLevel = 9;
}
