using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Bullet self-destructs after this many seconds if it hits nothing.")]
    public float lifetime = 3f;

    private Rigidbody2D rigid;
    private float damage;
    private float life_timer;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, float dmg)
    {
        damage = dmg;
        life_timer = lifetime;

        Vector2 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        rigid.linearVelocity = normalized * speed;

        float angle = Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        life_timer -= Time.deltaTime;
        if (life_timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() != null)
            return;

        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null)
            target.TakeDamage(damage);

        Destroy(gameObject);
    }
}
