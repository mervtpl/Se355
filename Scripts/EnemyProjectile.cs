using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifeTime = 3f;
    public int damage = 1;

    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Destroy(gameObject, lifeTime);
    }

    public void Configure(Vector2 fireDirection, GameObject projectileOwner)
    {
        direction = fireDirection.sqrMagnitude > 0.001f ? fireDirection.normalized : Vector2.right;
        owner = projectileOwner;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == owner)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger && !other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
