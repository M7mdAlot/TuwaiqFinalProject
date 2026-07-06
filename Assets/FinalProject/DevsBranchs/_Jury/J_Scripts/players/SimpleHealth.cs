using UnityEngine;
using UnityEngine.Events;
using Aegis.Core;

// Implements Aegis.Core.IDamageable additively (bridge only) so Mohammed's
// weapon/core systems can damage this object through the interface without
// changing how TakeDamage(int) already works for existing callers.
public class SimpleHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;

    public UnityEvent onHealthChanged;

    public bool IsAlive => currentHealth > 0;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        onHealthChanged?.Invoke();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log(name + " took damage. HP: " + currentHealth);

        onHealthChanged?.Invoke();

        if (currentHealth <= 0)
            Die();
    }

    void IDamageable.TakeDamage(float amount)
    {
        TakeDamage(Mathf.RoundToInt(amount));
    }

    void Die()
    {
        Debug.Log(name + " died.");
    }
}