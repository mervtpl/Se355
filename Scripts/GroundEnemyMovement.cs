using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundEnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2.6f;
    public int contactDamage = 1;
    public float damageCooldown = 0.75f;

    private Rigidbody2D rb;
    private Transform playerTarget;
    private float lastDamageTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FixedUpdate()
    {
        if (rb == null || GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        Vector2 direction = (playerTarget.position - transform.position);
        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        direction.Normalize();
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(contactDamage);
        lastDamageTime = Time.time;
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }
}
