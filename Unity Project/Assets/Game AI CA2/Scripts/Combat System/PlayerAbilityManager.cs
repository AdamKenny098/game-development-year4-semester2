using UnityEngine;

public class PlayerAbilityManager : MonoBehaviour
{
    public Camera cam;
    public Entity owner;
    public AbilityManager abilityManager;

    [Header("Targeting")]
    public float aimDistance = 40f;
    public LayerMask hitMask = ~0;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (owner == null) owner = GetComponentInParent<Entity>();
        if (abilityManager == null) abilityManager = GetComponentInParent<AbilityManager>();
        if (abilityManager == null) abilityManager = owner.gameObject.AddComponent<AbilityManager>();

        abilityManager.owner = owner;
    }

    void Update()
    {
        if (owner == null || owner.isDead) return;
        if (abilityManager == null || abilityManager.loadout == null) return;

        if (Input.GetMouseButton(0)) TrySlot(0);
        if (Input.GetMouseButtonDown(1)) TrySlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha1)) TrySlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TrySlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TrySlot(4);
    }

    void TrySlot(int slot)
    {
        AbilityData ability = abilityManager.GetAbility(slot);
        if (ability == null) return;

        Entity target;
        Vector3 hitPoint;
        if (!TryGetAimTarget(out target, out hitPoint))
        {
            return;
        }

        abilityManager.TryCast(slot, target, hitPoint);
    }

    bool TryGetAimTarget(out Entity target, out Vector3 hitPoint) // Out returns target and hitpoint along with bool success/fail of getting target
    {
        target = null;
        hitPoint = Vector3.zero;

        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, aimDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        hitPoint = hit.point;
        target = hit.collider.GetComponentInParent<Entity>();

        return target != null && !target.isDead;
    }

}
