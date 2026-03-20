using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 10;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    public event Action<int, int> OnHealthChanged;
    public event Action<int, bool> OnDamageTaken;
    public event Action<int> OnHealed;
    public event Action OnDeath;

    void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Start()
    {
        DamagePopupManager.Instance?.Register(this);
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount, bool isCritical = false)
    {
        if (amount <= 0)
            return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke(amount, isCritical);

        if (CurrentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealed?.Invoke(amount);
    }

    void Die()
    {
        Debug.Log($"[Health] {gameObject.name} morreu");
        OnDeath?.Invoke();
    }
}
