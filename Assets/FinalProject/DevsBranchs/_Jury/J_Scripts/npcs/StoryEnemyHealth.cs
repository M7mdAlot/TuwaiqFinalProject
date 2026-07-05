using UnityEngine;
using UnityEngine.Events;

public class StoryEnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death")]
    public bool disableOnDeath = true;
    public float disableDelay = 1f;

    [Header("Animator")]
    public Animator animator;
    public string damageTrigger = "Damage";
    public string deathBool = "IsDead";

    [Header("Events")]
    public UnityEvent onDeath;

    public bool IsDead => isDead;

    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (animator != null && !string.IsNullOrEmpty(damageTrigger))
            animator.SetTrigger(damageTrigger);

        Debug.Log(name + " took damage. HP: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;

        if (animator != null && !string.IsNullOrEmpty(deathBool))
            animator.SetBool(deathBool, true);

        Debug.Log(name + " died.");

        onDeath?.Invoke();

        if (disableOnDeath)
            Invoke(nameof(DisableEnemy), disableDelay);
    }

    void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
}