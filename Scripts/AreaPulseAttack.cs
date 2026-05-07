using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AreaPulseAttack : MonoBehaviour
{
    public float damageRadius = 2.2f;
    public int damage = 2;
    public float cooldown = 3f;
    public float effectDuration = 0.2f;
    public Transform areaVisualTransform;
    public SpriteRenderer areaVisualRenderer;
    public Color areaColor = new Color(0.55f, 0.9f, 1f, 0.28f);

    private float nextUseTime;
    private Coroutine visualRoutine;

    private void Awake()
    {
        CacheVisualReferences();
        HideVisual();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (GameManager.Instance != null && !GameManager.Instance.AllowsGameplayActions)
        {
            return;
        }

        bool pressed =
            Keyboard.current.digit1Key.wasPressedThisFrame ||
            Keyboard.current.numpad1Key.wasPressedThisFrame;

        if (!pressed || Time.time < nextUseTime)
        {
            return;
        }

        ActivatePulse();
    }

    public void ActivatePulse()
    {
        nextUseTime = Time.time + cooldown;
        DealAreaDamage();

        if (visualRoutine != null)
        {
            StopCoroutine(visualRoutine);
        }

        visualRoutine = StartCoroutine(ShowPulseRoutine());
    }

    private void DealAreaDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, damageRadius);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.CompareTag("Enemy"))
            {
                continue;
            }

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth == null || damagedEnemies.Contains(enemyHealth))
            {
                continue;
            }

            damagedEnemies.Add(enemyHealth);
            enemyHealth.TakeDamage(damage);
        }
    }

    private IEnumerator ShowPulseRoutine()
    {
        if (areaVisualTransform == null || areaVisualRenderer == null)
        {
            yield break;
        }

        areaVisualTransform.localScale = Vector3.one * damageRadius * 2f;
        areaVisualRenderer.color = areaColor;
        areaVisualRenderer.enabled = true;

        yield return new WaitForSeconds(effectDuration);

        HideVisual();
        visualRoutine = null;
    }

    private void CacheVisualReferences()
    {
        if (areaVisualTransform == null)
        {
            Transform child = transform.Find("AreaPulseVisual");
            if (child != null)
            {
                areaVisualTransform = child;
            }
        }

        if (areaVisualRenderer == null && areaVisualTransform != null)
        {
            areaVisualRenderer = areaVisualTransform.GetComponent<SpriteRenderer>();
        }
    }

    private void HideVisual()
    {
        if (areaVisualRenderer != null)
        {
            areaVisualRenderer.enabled = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.55f, 0.9f, 1f, 0.75f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
