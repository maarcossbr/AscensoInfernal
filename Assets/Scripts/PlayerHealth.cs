
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Vida del jugador")]
    [SerializeField] private int maxHealth = 3;

    [Header("Invulnerabilidad")]
    [SerializeField] private float invulnerabilityDuration = 1f;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead => CurrentHealth <= 0;
    public bool IsInvulnerable =>
        !IsDead && Time.time < invulnerableUntil;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private float invulnerableUntil;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        invulnerabilityDuration =
            Mathf.Max(0f, invulnerabilityDuration);

        CurrentHealth = maxHealth;
    }

    public bool TakeDamage(int amount = 1)
    {
        if (amount <= 0 || IsDead || IsInvulnerable)
            return false;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (!IsDead)
        {
            invulnerableUntil =
                Time.time + invulnerabilityDuration;
        }

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (IsDead)
            Died?.Invoke();

        return true;
    }

    public void KillInstantly()
    {
        if (IsDead)
            return;

        CurrentHealth = 0;
        invulnerableUntil = 0f;

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        Died?.Invoke();
    }

    public void RestoreFullHealth()
    {
        CurrentHealth = MaxHealth;
        invulnerableUntil = 0f;

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }


    public void RestoreHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(amount, 1, MaxHealth);
        invulnerableUntil = 0f;

        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

}
