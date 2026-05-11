using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using StarterAssets;

public class NPC : Character
{
    [Header("NPC Identity")]
    public string firstName;
    public string lastName;
    public string species;

    [Header("Abilities")]
    public List<Unlock> unlockedAbilities = new List<Unlock>();
    public Dictionary<string, float> abilityCooldownTimers = new Dictionary<string, float>();

    [Header("Merchant Specific")]
    public bool isAlerted;
    public bool requiresForgivenessQuest;
    
    public override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        InitialiseAbilities();
    }

    void InitialiseAbilities()
    {
        if (stats == null || ClassSystem.Instance == null)
            return;

        unlockedAbilities.Clear();
        abilityCooldownTimers.Clear();

        List<Unlock> classUnlocks =
            ClassSystem.Instance.GetUnlocks(characterClass);

        foreach (var unlock in classUnlocks)
        {
            if (unlock.unlockLevel <= stats.level)
            {
                unlockedAbilities.Add(unlock);
                abilityCooldownTimers[unlock.unlockName] = 0f;
            }
        }
    }

    public bool CanUseAbility(string abilityName)
    {
        return abilityCooldownTimers.ContainsKey(abilityName) && abilityCooldownTimers[abilityName] <= 0f;
    }

    public void UseAbility(string abilityName)
    {
        if (!CanUseAbility(abilityName)) return;

        Unlock ability = unlockedAbilities.Find(a => a.unlockName == abilityName);
        if (ability == null) return;

        abilityCooldownTimers[abilityName] = ability.unlockCoolDown;
    }
}
