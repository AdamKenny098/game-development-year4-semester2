using UnityEngine;

public class Character : Entity
{
    [Header("Progression")]
    public int level = 1;
    public int currentXP = 0;
    public ClassSystem.Classes characterClass;

    [Header("Abilities")]
    public AbilityManager abilityManager;

    public override void Awake()
    {
        base.Awake();
    }

    public virtual void Start()
    {
        ApplyClassToStats();
    }

    public void ApplyClassToStats()
    {
        if (ClassSystem.Instance == null)
        {
            return;
        }

        ClassStats cs = ClassSystem.Instance.GetStats(characterClass);
        
        int lvl = Mathf.Max(1, stats.level); // Ensure level is at least 1

        stats.maxHealth = cs.baseHealth + cs.healthPerLevel * (lvl - 1);
        stats.maxMana = cs.baseMana + cs.manaPerLevel * (lvl - 1);
        stats.maxStamina = cs.baseStamina + cs.staminaPerLevel * (lvl - 1);

        stats.strength = cs.baseStrength;
        stats.dexterity = cs.baseDexterity;
        stats.intelligence = cs.baseIntelligence;
        stats.charisma = cs.baseCharisma;

        stats.FillToMax();
        ApplyClassAbilities();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
    }

    public void ApplyClassAbilities()
    {
        AbilityLoadout chosen = null;

        if (ClassSystem.Instance != null)
            chosen = ClassSystem.Instance.GetLoadout(characterClass);

        abilityManager.SetLoadout(chosen);

    }
}
