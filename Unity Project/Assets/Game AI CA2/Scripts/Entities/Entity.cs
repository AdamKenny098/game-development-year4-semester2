using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Stats stats;

    public GameObject lastDamageSource;
    public Entity lastAttacker;
    public Vector3 lastHitPoint;
    public float lastDamageTime;
    Dictionary<AbilityData, float> abilityReadyTime = new Dictionary<AbilityData, float>();
    public bool isDead => stats != null && stats.health <= 0;

    public virtual void Awake()
    {
        if (stats == null)
            stats = GetComponent<Stats>();

        if (stats == null)
        {
            var c = GetComponent<Character>();
            if (c != null) stats = c.stats;
        }

        if (stats != null)
            stats.FillToMax();
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        if (isDead) return;
        if (stats == null) return;

        lastDamageSource = info.source;
        lastAttacker = info.attacker != null? info.attacker: (info.source ? info.source.GetComponentInParent<Entity>() : null);
        lastHitPoint = info.hitPoint;
        lastDamageTime = Time.time;
        stats.health -= info.amount;

        if (stats.health <= 0)
            Die();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (stats == null) return;
        stats.health -= amount;

        if (stats.health <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (stats == null) return;

        stats.health += amount;
        if (stats.health > stats.maxHealth)
            stats.health = stats.maxHealth;
    }


    public virtual void Die()
    {
    }

    bool IsAbilityOffCooldown(AbilityData ability)
    {
        if (ability == null) return false;
        if (!abilityReadyTime.TryGetValue(ability, out float t)) return true;
        return Time.time >= t;
    }

    void StartAbilityCooldown(AbilityData ability)
    {
        if (ability == null) return;
        float cd = Mathf.Max(0f, ability.cooldown);
        abilityReadyTime[ability] = Time.time + cd;
    }

    public bool TryUseAbilityOn(Entity target, AbilityData ability, Vector3 hitPoint)
    {
       if (target == null || ability == null) 
       return false;

        if (stats == null)
        {
            return false;
        }

        if (!IsAbilityOffCooldown(ability))
            return false;

        if (CombatSystem.Instance == null)
            return false;

        if (!CombatSystem.Instance.CanPayCosts(this, ability))
            return false;

        StartAbilityCooldown(ability);

        CombatSystem.Instance.PayCosts(this, ability);
        CombatResult result = CombatSystem.Instance.Resolve(this, target, ability);
        DamageInfo dmg = new DamageInfo
        {
            source = gameObject,
            attacker = this,
            amount = result.damage,
            type = ability.damageType,
            outcome = result.outcome,
            hitPoint = hitPoint
        };

        if (result.damage > 0 || result.outcome == RollOutcome.Crit || result.outcome == RollOutcome.Hit)
            target.TakeDamage(dmg);
        return true;
    }
}