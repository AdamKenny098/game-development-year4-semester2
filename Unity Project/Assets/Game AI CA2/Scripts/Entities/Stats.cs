using UnityEngine;

[System.Serializable]
public class Stats
{
    public int level = 1;

    public int maxHealth;
    public int health;

    public int maxMana;
    public int mana;

    public int maxStamina;
    public int stamina;

    public int strength;
    public int dexterity;
    public int intelligence;
    public int charisma;

    [Header("Combat (base bonuses)")]
    public int armorBonus = 0;
    public int shieldBonus = 0;

    public void FillToMax()
    {
        health = maxHealth;
        mana = maxMana;
        stamina = maxStamina;
    }

    // ===== DnD style =

    public int StrMod
    {
        get { return Mod(strength); }
    }

    public int DexMod
    {
        get { return Mod(dexterity); }
    }

    public int IntMod
    {
        get { return Mod(intelligence); }
    }

    public int ChaMod
    {
        get { return Mod(charisma); }
    }

    // 5e-ish: 1-4 => +2, 5-8 => +3, 9-12 => +4, etc.
    public int ProficiencyBonus
    {
        get
        {
            int lvl = Mathf.Max(1, level);
            return 2 + Mathf.FloorToInt((lvl - 1) / 4f);
        }
    }

    // Simple AC baseline: 10 + DEX mod + armor/shield bonuses
    public int ArmorClass
    {
        get { return 10 + DexMod + armorBonus + shieldBonus; }
    }

    // Spell math
    public int SpellSaveDC
    {
        get { return 8 + ProficiencyBonus + IntMod; }
    }

    public int SpellAttackBonus
    {
        get { return ProficiencyBonus + IntMod; }
    }

    // Weapon math
    public int MeleeAttackBonus
    {
        get { return ProficiencyBonus + StrMod; }
    }

    public int RangedAttackBonus
    {
        get { return ProficiencyBonus + DexMod; }
    }

    static int Mod(int score)
    {
        return Mathf.FloorToInt((score - 10) / 2f);
    }
}
