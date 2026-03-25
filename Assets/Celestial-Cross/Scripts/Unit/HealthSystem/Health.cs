using UnityEngine;
using CelestialCross.Combat;
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

    private PassiveManager passiveManager;

    void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void Start()
    {
        DamagePopupManager.Instance?.Register(this);
        passiveManager = GetComponent<PassiveManager>();
    }

    public void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount, bool isCritical = false, Unit source = null)
    {
        if (amount <= 0)
            return;

        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnDamageTaken?.Invoke(amount, isCritical);

        if (CurrentHealth <= 0)
            Die();
    }

public void Heal(int amount, bool allowOverheal = false)
    {
        if (amount <= 0)
            return;

        if (allowOverheal)
        {
            CurrentHealth += amount;
        }
        else
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);  
        }
            
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnHealed?.Invoke(amount);
    }

    void Die()
    {
        Debug.Log($"[Health] {gameObject.name} morreu");
        OnDeath?.Invoke();
    }
}
