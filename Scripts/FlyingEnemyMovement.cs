using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyingEnemyMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform playerTarget;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectionRange = 20f;
    public float stopDistance = 1.2f;
    public float idleHoverSpeed = 1.8f;
    public float idleHoverAmount = 0.45f;
    public float chaseHoverAmount = 0.6f;
    public float chaseHoverSpeed = 3f;
    public float followOffsetRadius = 1.4f;
    public float separationDistance = 1.1f;
    public float separationStrength = 1.5f;

    [Header("Contact Damage")]
    public int contactDamage = 1;
    public float damageCooldown = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer visualSpriteRenderer;
    private Vector2 idleAnchorPosition;
    private Vector2 lastMoveDirection = Vector2.right;
    private float lastDamageTime = -999f;
    private float waveOffset;
    private float nextPlayerSearchTime;
    private Vector2 followOffset;
    private bool isChasing;

    [Header("Visual Facing")]
    public bool invertFlip;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ConfigureRigidbody();
        CacheVisualSpriteRenderer();

        idleAnchorPosition = transform.position;
        waveOffset = Random.Range(0f, 10f);
        followOffset = Random.insideUnitCircle * followOffsetRadius;
    }

    private void Start()
    {
        TryFindPlayer();
    }

    private void Reset()
    {
        ConfigureRigidbody();
    }

    private void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (followOffset == Vector2.zero && followOffsetRadius > 0f)
        {
            followOffset = Random.insideUnitCircle * followOffsetRadius;
        }

        if (playerTarget == null && Time.time >= nextPlayerSearchTime)
        {
            TryFindPlayer();
            nextPlayerSearchTime = Time.time + 1f;
        }

        if (playerTarget != null)
        {
            Vector2 toPlayer = (Vector2)playerTarget.position - rb.position;
            float distance = toPlayer.magnitude;

            if (distance <= detectionRange)
            {
                UpdateChaseMovement(toPlayer, distance);
                UpdateSpriteFlip();
                return;
            }
        }

        UpdateIdleMovement();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null)
        {
            return;
        }

        TryDealContactDamage(collision.collider);
    }

    private void UpdateIdleMovement()
    {
        if (isChasing)
        {
            idleAnchorPosition = rb.position;
            isChasing = false;
        }

        float idleWave = Mathf.Sin((Time.fixedTime + waveOffset) * idleHoverSpeed);
        Vector2 desiredPosition = idleAnchorPosition + Vector2.up * (idleWave * idleHoverAmount);
        Vector2 nextPosition = Vector2.Lerp(rb.position, desiredPosition, 0.2f);
        rb.MovePosition(nextPosition);
    }

    private void UpdateChaseMovement(Vector2 toPlayer, float distance)
    {
        isChasing = true;

        if (distance > 0.001f)
        {
            lastMoveDirection = toPlayer.normalized;
        }

        Vector2 perpendicular = new Vector2(-lastMoveDirection.y, lastMoveDirection.x);
        float hoverWave = Mathf.Sin((Time.fixedTime + waveOffset) * chaseHoverSpeed) * chaseHoverAmount;
        Vector2 hoverOffset = perpendicular * hoverWave;
        Vector2 separationOffset = CalculateSeparationOffset();
        Vector2 targetCenter = (Vector2)playerTarget.position + followOffset;

        Vector2 desiredPoint = targetCenter - lastMoveDirection * stopDistance + hoverOffset + separationOffset;
        Vector2 nextPosition = Vector2.MoveTowards(rb.position, desiredPoint, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(nextPosition);
    }

    private void TryFindPlayer()
    {
        if (playerTarget != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
    }

    private void TryDealContactDamage(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (Time.time < lastDamageTime + damageCooldown)
        {
            return;
        }

        other.SendMessage("TakeDamage", contactDamage, SendMessageOptions.DontRequireReceiver);
        lastDamageTime = Time.time;
    }

    private void UpdateSpriteFlip()
    {
        if (visualSpriteRenderer == null || playerTarget == null)
        {
            return;
        }

        bool shouldFlip = playerTarget.position.x < transform.position.x;
        visualSpriteRenderer.flipX = invertFlip ? !shouldFlip : shouldFlip;
    }

    private Vector2 CalculateSeparationOffset()
    {
        if (separationDistance <= 0f || separationStrength <= 0f)
        {
            return Vector2.zero;
        }

        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, separationDistance);
        if (nearbyColliders == null || nearbyColliders.Length == 0)
        {
            return Vector2.zero;
        }

        Vector2 separation = Vector2.zero;

        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            Collider2D nearbyCollider = nearbyColliders[i];
            if (nearbyCollider == null || nearbyCollider.gameObject == gameObject)
            {
                continue;
            }

            FlyingEnemyMovement otherEnemy = nearbyCollider.GetComponent<FlyingEnemyMovement>();
            if (otherEnemy == null)
            {
                continue;
            }

            Vector2 away = rb.position - (Vector2)otherEnemy.transform.position;
            float distance = away.magnitude;
            if (distance <= 0.001f)
            {
                continue;
            }

            float weight = 1f - Mathf.Clamp01(distance / separationDistance);
            separation += away.normalized * weight;
        }

        return separation * separationStrength;
    }

    private void CacheVisualSpriteRenderer()
    {
        if (visualSpriteRenderer != null)
        {
            return;
        }

        Transform visualTransform = transform.Find("Visual");
        if (visualTransform != null)
        {
            visualSpriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
        }
    }

    private void ConfigureRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 1f, 0.8f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
