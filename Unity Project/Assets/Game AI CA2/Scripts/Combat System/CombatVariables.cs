using UnityEngine;

public enum DamageType
{
    Bludgeoning,
    Piercing,
    Slashing,
    Magic
}

public enum AbilityResolution
{
    AttackRoll,
    SavingThrow
}

public enum SaveType
{
    Strength,
    Dexterity,
    Intelligence,
    Charisma
}

public enum RollOutcome
{
    Miss,
    Hit,
    Crit
}

public struct DamageInfo
{
    public GameObject source;
    public Entity attacker;
    public int amount;
    public DamageType type;
    public RollOutcome outcome;
    public Vector3 hitPoint;
}

public struct CombatResult
{
    public RollOutcome outcome;
    public int damage;
    public int d20;
    public int totalToHitOrSave;
    public int targetNumber;
    public bool saved;
}
