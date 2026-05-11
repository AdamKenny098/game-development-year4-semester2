using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text nameText;
    public Slider hpSlider;

    [Header("Follow")]
    public Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);
    Transform target;
    Entity targetEntity;
    Camera cam;

    void Update()
    {
        if (targetEntity == null || targetEntity.stats == null || target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + worldOffset;

        if (cam == null) cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCam = cam.transform.position - transform.position;
            toCam.y = 0f;

            if (toCam.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(-toCam);   
            }
        }
        
        Stats s = targetEntity.stats;

        if (hpSlider != null)
        {
            hpSlider.maxValue = s.maxHealth;
            hpSlider.value = s.health;
        }
        
        if (targetEntity.isDead)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Bind(Entity entity, Transform followTarget = null) // followTarget is optional, if not provided will default to entity's transform
    {
        targetEntity = entity;
        target = followTarget ?? entity?.transform;
        cam = Camera.main;
        RefreshStaticText();
    }

    void RefreshStaticText()
    {
        if (targetEntity == null || targetEntity.stats == null) return;

        if (nameText != null)
        {
            string n = targetEntity.gameObject.name;
            int lvl = targetEntity.stats.level;
            nameText.text = n + "  Lv " + lvl;
        }
    }
}
