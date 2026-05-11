using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSystem : MonoBehaviour
{
    public static ClassSystem Instance;
    public enum Classes
    {
        Warrior,
        Archer,
        Mage,
        Thief,
        Merchant
    }

    public Dictionary<Classes, ClassStats> classData;
    public Dictionary<Classes, List<Unlock>> classUnlocks;
    public AbilityLoadout warriorLoadout;
    public AbilityLoadout archerLoadout;
    public AbilityLoadout mageLoadout;
    public AbilityLoadout thiefLoadout;
    public AbilityLoadout merchantLoadout;

    public AbilityLoadout GetLoadout(Classes c)
    {
        switch (c)
        {
            case Classes.Warrior: return warriorLoadout;
            case Classes.Archer: return archerLoadout;
            case Classes.Mage: return mageLoadout;
            case Classes.Thief: return thiefLoadout;
            case Classes.Merchant: return merchantLoadout;
        }
        return null;
    }


    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitialiseClassData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitialiseClassData()
{
    classData = new Dictionary<Classes, ClassStats>()
    {
        {
            Classes.Warrior,
            new ClassStats
            {
                className = "Warrior",
                baseHealth = 50,
                baseMana = 30,
                baseStamina = 120,
                baseStrength = 18,
                baseDexterity = 10,
                baseIntelligence = 8,
                baseCharisma = 9,
                healthPerLevel = 15,
                manaPerLevel = 6,
                staminaPerLevel = 10
            }
        },
        {
            Classes.Archer,
            new ClassStats
            {
                className = "Archer",
                baseHealth = 35,
                baseMana = 40,
                baseStamina = 80,
                baseStrength = 12,
                baseDexterity = 18,
                baseIntelligence = 10,
                baseCharisma = 11,
                healthPerLevel = 10,
                manaPerLevel = 8,
                staminaPerLevel = 8
            }
        },
        {
            Classes.Mage,
            new ClassStats
            {
                className = "Mage",
                baseHealth = 20,
                baseMana = 150,
                baseStamina = 30,
                baseStrength = 6,
                baseDexterity = 9,
                baseIntelligence = 20,
                baseCharisma = 10,
                healthPerLevel = 5,
                manaPerLevel = 15,
                staminaPerLevel = 5
            }
        },
        {
            Classes.Thief,
            new ClassStats
            {
                className = "Thief",
                baseHealth = 35,
                baseMana = 35,
                baseStamina = 100,
                baseStrength = 10,
                baseDexterity = 17,
                baseIntelligence = 11,
                baseCharisma = 12,
                healthPerLevel = 10,
                manaPerLevel = 5,
                staminaPerLevel = 5
            }
        },
        {
            Classes.Merchant,
            new ClassStats
            {
                className = "Merchant",
                baseHealth = 100,
                baseMana = 50,
                baseStamina = 50,
                baseStrength = 8,
                baseDexterity = 10,
                baseIntelligence = 13,
                baseCharisma = 18,
                healthPerLevel = 20,
                manaPerLevel = 10,
                staminaPerLevel = 10
            }
        }
    };

    classUnlocks = new Dictionary<Classes, List<Unlock>>()
    {
        {
            Classes.Warrior, new List<Unlock>
            {
                new Unlock { unlockName = "Power Strike", unlockLevel = 3, description = "A heavy melee attack dealing 200% damage.", unlockCoolDown = 5f },
                new Unlock { unlockName = "Shield Bash", unlockLevel = 5, description = "Stuns an enemy for 1.5 seconds.", unlockCoolDown = 8f },
                new Unlock { unlockName = "War Cry", unlockLevel = 8, description = "Boosts damage by 20% for a short duration.", unlockCoolDown = 12f }
            }
        },
        {
            Classes.Archer, new List<Unlock>
            {
                new Unlock { unlockName = "Piercing Shot", unlockLevel = 3, description = "Fires an arrow that passes through multiple enemies.", unlockCoolDown = 6f },
                new Unlock { unlockName = "Eagle Eye", unlockLevel = 6, description = "Increases chance of critical hits and overall damage for 10 seconds.", unlockCoolDown = 15f }
            }
        },
        {
            Classes.Mage, new List<Unlock>
            {
                new Unlock { unlockName = "Firebolt", unlockLevel = 2, description = "Launches a ball of fire that explodes on impact.", unlockCoolDown = 3f },
                new Unlock { unlockName = "Teleport", unlockLevel = 5, description = "Instantly move to a nearby location.", unlockCoolDown = 10f },
                new Unlock { unlockName = "Arcane Storm", unlockLevel = 8, description = "Unleash a storm of arcane energy around you.", unlockCoolDown = 20f }
            }
        },
        {
            Classes.Thief, new List<Unlock>
            {
                new Unlock { unlockName = "Backstab", unlockLevel = 2, description = "Deals extra damage when attacking from behind.", unlockCoolDown = 4f },
                new Unlock { unlockName = "Shadowstep", unlockLevel = 4, description = "Instantly dash behind your target.", unlockCoolDown = 6f },
                new Unlock { unlockName = "Smoke Bomb", unlockLevel = 6, description = "Escape combat by blinding enemies briefly.", unlockCoolDown = 10f }
            }
        }
    };
}

    public ClassStats GetStats(Classes characterClass )
    {
        if (classData.ContainsKey(characterClass))
        {
            return classData[characterClass];
        }
        return new ClassStats();
    }

    public List<Unlock> GetUnlocks(Classes characterClass)
    {
        if (classUnlocks.ContainsKey(characterClass))
        {
            return classUnlocks[characterClass];
        }
        return new List<Unlock>();
    }
}

public class ClassStats
{
    public string className;
    public int baseHealth;
    public int baseMana;
    public int baseStamina;
    public int baseStrength;
    public int baseDexterity;
    public int baseIntelligence;
    public int baseCharisma;

    public int healthPerLevel;
    public int manaPerLevel;
    public int staminaPerLevel;
}

public class Unlock
{
    public string unlockName;
    public int unlockLevel;
    public string description;
    public float unlockCoolDown;
}


