using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public bool enableDebugDamage;
    public int debugDamageAmount = 1;

    [SerializeField] private int currentHealth;

    private bool isDead;

    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        ResetHealth();
    }

    private void OnEnable()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (!enableDebugDamage || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            TakeDamage(debugDamageAmount);
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = Mathf.Max(1, newMaxHealth);
        ResetHealth();
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        GameManager.Instance?.NotifyEnemyDefeated();
        LevelManager.Instance?.NotifyEnemyKilled(this);
        Destroy(gameObject);
    }

    private void ResetHealth()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        isDead = false;
    }
}
