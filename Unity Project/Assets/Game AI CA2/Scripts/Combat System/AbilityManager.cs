using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public Entity owner;

    [Header("Runtime Loadout")]
    public AbilityLoadout loadout;

    float[] nextReady;

    void Awake()
    {
        if (owner == null) owner = GetComponent<Entity>();
        if (nextReady == null || nextReady.Length != 5)
        {
            nextReady = new float[5];
        }
    }

    public bool IsReady(int slot)
    {
        if (nextReady == null) return false;
        if (slot < 0 || slot >= nextReady.Length) return false;
        return Time.time >= nextReady[slot];
    }

    public void SetLoadout(AbilityLoadout newLoadout)
    {
        loadout = newLoadout;

        if (nextReady == null || nextReady.Length != 5)
        {
            nextReady = new float[5];
        }

        for (int i = 0; i < nextReady.Length; i++)
            nextReady[i] = 0f;
    }

    public AbilityData GetAbility(int slot)
    {
        if (loadout == null) return null;

        if (slot == 0) return loadout.primary;
        if (slot == 1) return loadout.secondary;
        if (slot == 2) return loadout.unlock1;
        if (slot == 3) return loadout.unlock2;
        if (slot == 4) return loadout.unlock3;

        return null;
    }

    public bool TryCast(int slot, Entity target, Vector3 hitPoint)
    {
        AbilityData ability = GetAbility(slot);
        return TryCast(slot, ability, target, hitPoint);
    }

    public bool TryCast(int slot, AbilityData ability, Entity target, Vector3 hitPoint)
    {
        if (owner == null || owner.stats == null) return false;
        if (ability == null) return false; 
        if (target == null) return false;
        if (!IsReady(slot)) return false;

        if (ability.range > 0f) 
        {
            float dist = Vector3.Distance(owner.transform.position, target.transform.position);
            if (dist > ability.range) return false;
        }

        bool ok = owner.TryUseAbilityOn(target, ability, hitPoint);
        if (!ok) return false;

        nextReady[slot] = Time.time + Mathf.Max(0f, ability.cooldown);
        return true;
    }

    public float GetCooldownRemaining(int slot)
    {
        if (nextReady == null) return 0f;
        if (slot < 0 || slot >= nextReady.Length) return 0f;

        float remain = nextReady[slot] - Time.time;
        return remain > 0f ? remain : 0f;
    }

    public float GetCooldownDuration(int slot)
    {
        AbilityData abilityData = GetAbility(slot);
        if (abilityData == null) return 0f;
        return Mathf.Max(0f, abilityData.cooldown);
    }
}