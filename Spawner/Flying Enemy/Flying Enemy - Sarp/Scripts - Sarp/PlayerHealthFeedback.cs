using System.Collections;
using UnityEngine;

public class PlayerHealthFeedback : MonoBehaviour
{
    public int maxHealth = 100;
    public float hitFlashDuration = 0.6f;

    public bool IsInfected => isInfected;

    private int currentHealth;
    private Renderer[] cachedRenderers;
    private Color[] baseColors;
    private Coroutine flashRoutine;
    private bool isInfected;

    private void Awake()
    {
        currentHealth = maxHealth;
        cachedRenderers = GetComponentsInChildren<Renderer>();
        baseColors = new Color[cachedRenderers.Length];

        for (var i = 0; i < cachedRenderers.Length; i++)
        {
            baseColors[i] = cachedRenderers[i].material.color;
        }
    }

    public void TakeHit(int damage, Color attackerColor)
    {
        if (isInfected)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"You are under attack! Lost {damage} HP! Current HP: {currentHealth}");

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (currentHealth == 0)
        {
            ApplyInfection(attackerColor);
            return;
        }

        flashRoutine = StartCoroutine(FlashRoutine(attackerColor));
    }

    private void ApplyInfection(Color attackerColor)
    {
        isInfected = true;

        for (var i = 0; i < cachedRenderers.Length; i++)
        {
            cachedRenderers[i].material.color = attackerColor;
            baseColors[i] = attackerColor;
        }
    }

    private IEnumerator FlashRoutine(Color attackerColor)
    {
        var elapsed = 0f;

        while (elapsed < hitFlashDuration)
        {
            elapsed += Time.deltaTime;
            var normalizedTime = Mathf.Clamp01(elapsed / hitFlashDuration);
            var blend = normalizedTime < 0.5f
                ? Mathf.SmoothStep(0f, 1f, normalizedTime / 0.5f)
                : Mathf.SmoothStep(1f, 0f, (normalizedTime - 0.5f) / 0.5f);

            for (var i = 0; i < cachedRenderers.Length; i++)
            {
                cachedRenderers[i].material.color = Color.Lerp(baseColors[i], attackerColor, blend);
            }

            yield return null;
        }

        for (var i = 0; i < cachedRenderers.Length; i++)
        {
            cachedRenderers[i].material.color = baseColors[i];
        }

        flashRoutine = null;
    }
}
