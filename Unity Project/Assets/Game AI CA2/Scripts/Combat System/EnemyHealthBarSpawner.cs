using UnityEngine;

public class EnemyHealthBarSpawner : MonoBehaviour
{
    public EnemyHealthBar healthBarPrefab;
    public Vector3 offset = new Vector3(0f, 2.0f, 0f);

    EnemyHealthBar instance;
    Entity entity;

    void Awake()
    {
        entity = GetComponentInParent<Entity>();
    }

    void Start()
    {
        if (healthBarPrefab == null || entity == null) return;

        instance = Instantiate(healthBarPrefab, transform, false);

        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        instance.worldOffset = offset;

        Transform follow = transform;
        instance.Bind(entity, follow);
    }

    void OnDestroy()
    {
        if (instance != null)
            Destroy(instance.gameObject);
    }
}
