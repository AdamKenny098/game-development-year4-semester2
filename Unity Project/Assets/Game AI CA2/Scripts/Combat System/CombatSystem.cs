using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatSystem : MonoBehaviour
{

    public static CombatSystem Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public CombatResult Resolve(Entity attacker, Entity target, AbilityData ability)
    {
        CombatResult r = new CombatResult();

        if (attacker == null || target == null || ability == null)
        {
            r.outcome = RollOutcome.Miss;
            return r;
        }

        Stats a = attacker.stats;
        Stats t = target.stats;
        if (a == null || t == null)
        {
            r.outcome = RollOutcome.Miss;
            return r;
        }

        if (ability.resolution == AbilityResolution.AttackRoll)
            ResolveAttackRoll(ref r, a, t, ability);
        else
            ResolveSavingThrow(ref r, a, t, ability);

        return r;
    }

    public void ResolveAttackRoll(ref CombatResult r, Stats a, Stats t, AbilityData ability)
    {
        int d20 = Dice.Instance.RollD20();
        int bonus = GetAttackBonus(a, ability);
        int ac = t.ArmorClass;
        int total = d20 + bonus;

        r.d20 = d20;
        r.totalToHitOrSave = total;
        r.targetNumber = ac;
        r.saved = false;

        bool hit = (d20 == 20) || (total >= ac);
        if (!hit)
        {
            r.outcome = RollOutcome.Miss;
            r.damage = 0;
            return;
        }

        int dmg = Dice.Instance.Roll(ability.damageDice, a);
        if (dmg < 0) dmg = 0;

        if (d20 == 20)
        {
            int extra = Dice.Instance.Roll(ability.damageDice, a);
            if (extra > 0) dmg += extra;
            r.outcome = RollOutcome.Crit;
        }
        else
        {
            r.outcome = RollOutcome.Hit;
        }

        r.damage = dmg;
    }

    public void ResolveSavingThrow(ref CombatResult r, Stats a, Stats t, AbilityData ability)
    {
        int dc = a.SpellSaveDC;
        int bonus = GetSaveBonus(t, ability.saveType);

        int d20 = Dice.Instance.RollD20();
        int total = d20 + bonus;

        r.d20 = d20;
        r.totalToHitOrSave = total;
        r.targetNumber = dc;

        bool saved = total >= dc;
        r.saved = saved;

        int dmg = Dice.Instance.Roll(ability.damageDice, a);
        if (dmg < 0) dmg = 0;

        if (saved)
        {
            dmg = ability.halfDamageOnSave ? Mathf.FloorToInt(dmg * 0.5f) : 0;
            r.outcome = RollOutcome.Miss; // “resisted”
        }
        else
        {
            r.outcome = RollOutcome.Hit;
        }

        r.damage = dmg;
    }

    public int GetAttackBonus(Stats a, AbilityData ability)
    {
        if (ability.usesSpellAttackBonus)
            return a.SpellAttackBonus;

        // Piercing = ranged, else melee (your current rule)
        return (ability.damageType == DamageType.Piercing) ? a.RangedAttackBonus : a.MeleeAttackBonus;
    }

    public int GetSaveBonus(Stats t, SaveType type)
    {
        if (type == SaveType.Strength) return t.StrMod;
        if (type == SaveType.Dexterity) return t.DexMod;
        if (type == SaveType.Intelligence) return t.IntMod;
        if (type == SaveType.Charisma) return t.ChaMod;
        return 0;
    }

    public  bool CanPayCosts(Entity user, AbilityData ability)
    {
        if (user == null || user.stats == null || ability == null)
            return false;

        Stats s = user.stats;
        return s.stamina >= ability.staminaCost && s.mana >= ability.manaCost;
    }

    public  void PayCosts(Entity user, AbilityData ability)
    {
        if (user == null || user.stats == null || ability == null)
            return;

        Stats s = user.stats;

        s.stamina -= ability.staminaCost;
        if (s.stamina < 0) s.stamina = 0;

        s.mana -= ability.manaCost;
        if (s.mana < 0) s.mana = 0;
    }
}
