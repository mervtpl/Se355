using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invulnerabilityDuration = 0.75f;
    public float hurtFlashDuration = 0.1f;
    public Color hurtFlashColor = new Color(1f, 0.45f, 0.45f, 1f);

    [SerializeField] private int currentHealth;

    private float lastDamageTime = -999f;
    private bool isDead;
    private SpriteRenderer spriteRenderer;
    private Color defaultColor;
    private Coroutine flashRoutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 20;
            defaultColor = spriteRenderer.color;
        }

        ResetHealth();
    }

    private void OnEnable()
    {
        ResetHealth();
    }

    private void Start()
    {
        GameManager.Instance?.RegisterPlayer(this);
        GameManager.Instance?.OnPlayerHealthChanged(this);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || isDead)
        {
            return;
        }

        if (Time.time < lastDamageTime + invulnerabilityDuration)
        {
            return;
        }

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        GameManager.Instance?.OnPlayerHealthChanged(this);

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(HurtFlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }

        AutoShooter autoShooter = GetComponent<AutoShooter>();
        if (autoShooter != null)
        {
            autoShooter.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        GameManager.Instance?.HandlePlayerDeath();
    }

    public void ResetHealth()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        isDead = false;
        lastDamageTime = -999f;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
    }

    private IEnumerator HurtFlashRoutine()
    {
        if (spriteRenderer == null)
        {
            yield break;
        }

        spriteRenderer.color = hurtFlashColor;
        yield return new WaitForSeconds(hurtFlashDuration);

        if (!isDead)
        {
            spriteRenderer.color = defaultColor;
        }
    }
}
