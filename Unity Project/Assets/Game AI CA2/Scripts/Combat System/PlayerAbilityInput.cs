using UnityEngine;

public class PlayerAbilityInput : MonoBehaviour
{
    public Camera cam;
    public Entity owner;
    public AbilityManager abilities;
    public float aimDistance = 40f;
    public LayerMask hitMask = ~0;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (owner == null) owner = GetComponentInParent<Entity>();
        if (abilities == null) abilities = GetComponentInParent<AbilityManager>();
    }

    void Update()
    {
    
        if (owner == null || owner.isDead) return;
        if (abilities == null || abilities.loadout == null) return;

        if (Input.GetMouseButton(0)) TrySlot(0);
        if (Input.GetMouseButtonDown(1)) TrySlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha1)) TrySlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TrySlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TrySlot(4);
    }

    void TrySlot(int slot)
    {
        Entity target;
        Vector3 hitPoint;

        if (!TryGetAimTarget(out target, out hitPoint))
        {
            return;
        }
        abilities.TryCast(slot, target, hitPoint);
    }

    bool TryGetAimTarget(out Entity target, out Vector3 hitPoint) // Out returns target and hitpoint along with bool success/fail of getting target
    {
        target = null;
        hitPoint = Vector3.zero;
        if (cam == null) return false;

        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, aimDistance, hitMask, QueryTriggerInteraction.Ignore))
            return false;

        var ent = hit.collider.GetComponentInParent<Entity>();
        if (ent == null) return false;
        if (ent.isDead) return false;

        hitPoint = hit.point;
        target = hit.collider.GetComponentInParent<Entity>();
        return target != null && !target.isDead;
    }
}
