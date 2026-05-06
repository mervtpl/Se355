using UnityEngine;

public class EnemyFollower : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 2.25f;
    public float stopDistance = 0.25f;
    public bool rotateTowardTarget = true;

    Rigidbody2D body;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void FixedUpdate()
    {
        Transform currentTarget = GetTarget();
        Vector2 toTarget = currentTarget != null ? currentTarget.position - transform.position : Vector2.zero;

        if (currentTarget == null || toTarget.sqrMagnitude <= stopDistance * stopDistance)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = toTarget.normalized;
        body.linearVelocity = direction * moveSpeed;

        if (rotateTowardTarget)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            body.MoveRotation(angle);
        }
    }

    Transform GetTarget()
    {
        if (target != null)
        {
            return target;
        }

        if (PlayerController.Current != null)
        {
            target = PlayerController.Current;
            return target;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        target = player != null ? player.transform : null;
        return target;
    }
}
