using UnityEngine;

public class DemoHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float NormalizedHealth => maxHealth <= 0f ? 0f : currentHealth / maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsLowHealth => NormalizedHealth <= 0.3f && !IsDead;

    private void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void Damage(float amount)
    {
        if (IsDead)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Abs(amount));
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Abs(amount));
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
    }
}
