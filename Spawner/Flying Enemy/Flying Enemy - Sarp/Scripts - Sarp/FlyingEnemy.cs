using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class FlyingEnemy : MonoBehaviour
{
    private static readonly List<FlyingEnemy> ActiveEnemies = new List<FlyingEnemy>();

    [Header("References")]
    public Transform playerTarget;

    [Header("Movement")]
    public float moveSpeed = 3.2f;
    public float hoverAmplitude = 0.22f;
    public float hoverFrequency = 3.5f;
    public float flightHeight = 1.8f;
    public float separationRadius = 1.35f;
    public float separationStrength = 1.6f;

    [Header("Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1.4f;
    public float attackDuration = 0.3f;
    public int attackDamage = 5;

    [Header("Click Destroy")]
    public float clickDestroyRange = 4f;
    public float clickDestroyDelay = 0.5f;
    public Color clickDestroyColor = Color.green;

    public Action onDestroyed;

    public Color EnemyColor => new Color(1f, 0.66f, 0.22f);

    private float hoverSeed;
    private float lastAttackTime = -999f;
    private bool isAttacking;
    private Vector3 baseScale;
    private Transform body;
    private Transform[] spikes;
    private bool isDestroying;

    public static bool IsSpawnPointClear(Vector3 position, float minDistance)
    {
        var minDistanceSqr = minDistance * minDistance;
        foreach (var enemy in ActiveEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            var offset = enemy.transform.position - position;
            offset.y = 0f;
            if (offset.sqrMagnitude < minDistanceSqr)
            {
                return false;
            }
        }

        return true;
    }

    private void OnEnable()
    {
        if (!ActiveEnemies.Contains(this))
        {
            ActiveEnemies.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }

    private void Start()
    {
        hoverSeed = UnityEngine.Random.Range(0f, 10f);
        baseScale = transform.localScale;
        body = transform.Find("Body");
        spikes = new Transform[transform.childCount];
        for (var i = 0; i < transform.childCount; i++)
        {
            spikes[i] = transform.GetChild(i);
        }
    }

    private void Update()
    {
        if (playerTarget == null)
        {
            return;
        }

        if (isDestroying)
        {
            AnimateVirusMotion();
            return;
        }

        TryHandleClickDestroy();

        if (playerTarget.TryGetComponent<PlayerHealthFeedback>(out var playerHealth) && playerHealth.IsInfected)
        {
            isAttacking = false;
            AnimateHover();
            AnimateVirusMotion();
            return;
        }

        if (isAttacking)
        {
            AnimateVirusMotion();
            return;
        }

        AnimateHover();
        AnimateVirusMotion();

        var targetPosition = GetTargetFlightPosition();
        var toTarget = targetPosition - transform.position;
        var distanceToTarget = toTarget.magnitude;
        var separation = CalculateSeparation();

        var movementDirection = Vector3.zero;
        if (distanceToTarget > attackRange)
        {
            movementDirection = toTarget.normalized;
        }

        movementDirection += separation;
        if (movementDirection.sqrMagnitude > 0.0001f)
        {
            transform.position += movementDirection.normalized * moveSpeed * Time.deltaTime;
        }

        if (distanceToTarget <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }

        FaceMovementDirection(movementDirection.sqrMagnitude > 0.0001f ? movementDirection : toTarget);
    }

    private void TryHandleClickDestroy()
    {
        if (!WasPrimaryClickPressed() || Camera.main == null || playerTarget == null)
        {
            return;
        }

        var playerOffset = transform.position - playerTarget.position;
        playerOffset.y = 0f;
        if (playerOffset.sqrMagnitude > clickDestroyRange * clickDestroyRange)
        {
            return;
        }

        var ray = Camera.main.ScreenPointToRay(GetPointerPosition());
        if (Physics.Raycast(ray, out var hit) && hit.transform.IsChildOf(transform))
        {
            StartCoroutine(ClickDestroyRoutine());
        }
    }

    private static bool WasPrimaryClickPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetMouseButtonDown(0);
#else
        return false;
#endif
    }

    private static Vector3 GetPointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
        return Vector3.zero;
#endif
    }

    private IEnumerator ClickDestroyRoutine()
    {
        isDestroying = true;
        isAttacking = false;

        foreach (var childRenderer in GetComponentsInChildren<Renderer>())
        {
            childRenderer.material.color = clickDestroyColor;
        }

        yield return new WaitForSeconds(clickDestroyDelay);
        Debug.Log("One virus has been destroyed.");
        Destroy(gameObject);
    }

    private void AnimateHover()
    {
        var hover = Mathf.Sin((Time.time + hoverSeed) * hoverFrequency) * hoverAmplitude;
        var position = transform.position;
        position.y = flightHeight + hover;
        transform.position = position;
    }

    private void AnimateVirusMotion()
    {
        var pulse = 1f + Mathf.Sin((Time.time + hoverSeed) * hoverFrequency * 1.6f) * 0.08f;
        transform.localScale = baseScale * pulse;
        transform.Rotate(0f, 35f * Time.deltaTime, 0f, Space.Self);

        if (body != null)
        {
            body.localScale = new Vector3(0.72f, 0.34f + Mathf.Sin((Time.time + hoverSeed) * hoverFrequency) * 0.04f, 0.72f);
        }

        foreach (var spike in spikes)
        {
            if (spike == null || !spike.name.StartsWith("Spike"))
            {
                continue;
            }

            var spikePulse = 1f + Mathf.Sin((Time.time + hoverSeed) * hoverFrequency * 2f + spike.GetSiblingIndex()) * 0.12f;
            spike.localScale = new Vector3(0.16f, 0.28f * spikePulse, 0.16f);
        }
    }

    private IEnumerator AttackRoutine()
    {
        if (isDestroying)
        {
            yield break;
        }
	
        isAttacking = true;
        lastAttackTime = Time.time;

        var start = transform.position;
        var impactPoint = playerTarget.position + Vector3.up * 0.45f;
        var attackMidpoint = attackDuration * 0.5f;
        var elapsed = 0f;
        var hitApplied = false;

        while (elapsed < attackDuration && !isDestroying)
        {
            elapsed += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / attackDuration);
            var dashT = normalizedTime < 0.5f
                ? Mathf.SmoothStep(0f, 1f, normalizedTime / 0.5f)
                : Mathf.SmoothStep(1f, 0f, (normalizedTime - 0.5f) / 0.5f);

            transform.position = Vector3.Lerp(start, impactPoint, dashT);
            FaceMovementDirection(impactPoint - start);

            if (!hitApplied && elapsed >= attackMidpoint)
            {
                hitApplied = true;
                if (playerTarget.TryGetComponent<PlayerHealthFeedback>(out var playerHealth))
                {
                    playerHealth.TakeHit(attackDamage, EnemyColor);
                    if (playerHealth.IsInfected)
                    {
                        break;
                    }
                }
            }

            yield return null;
        }

        if (!isDestroying)
        {
            transform.position = start;
        }
        isAttacking = false;
    }

    private Vector3 GetTargetFlightPosition()
    {
        var playerPosition = playerTarget.position;
        return new Vector3(playerPosition.x, flightHeight, playerPosition.z);
    }

    private Vector3 CalculateSeparation()
    {
        var separation = Vector3.zero;
        var separationRadiusSqr = separationRadius * separationRadius;

        foreach (var other in ActiveEnemies)
        {
            if (other == null || other == this || !other.isActiveAndEnabled)
            {
                continue;
            }

            var offset = transform.position - other.transform.position;
            offset.y = 0f;
            var distanceSqr = offset.sqrMagnitude;
            if (distanceSqr >= separationRadiusSqr)
            {
                continue;
            }

            if (distanceSqr < 0.0001f)
            {
                offset = GetFallbackSeparationDirection(other);
                distanceSqr = offset.sqrMagnitude;
            }

            var distance = Mathf.Sqrt(distanceSqr);
            separation += offset.normalized * ((separationRadius - distance) / separationRadius);
        }

        return separation * separationStrength;
    }

    private Vector3 GetFallbackSeparationDirection(FlyingEnemy other)
    {
        var angle = (GetInstanceID() - other.GetInstanceID()) * 137.5f * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.01f;
    }

    private void FaceMovementDirection(Vector3 direction)
    {
        var flatDirection = new Vector3(direction.x, 0f, direction.z);
        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.rotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
    }

    private void OnDestroy()
    {
        ActiveEnemies.Remove(this);
        onDestroyed?.Invoke();
    }
}
