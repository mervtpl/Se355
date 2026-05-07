using UnityEngine;

public class FlyingShadowEffect : MonoBehaviour
{
    public FlyingVisualEffect visualEffect;
    public float shadowScaleAmount = 0.12f;
    public float shadowAlpha = 0.3f;

    private Vector3 startLocalScale;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        startLocalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = shadowAlpha;
            spriteRenderer.color = color;
        }
    }

    private void Update()
    {
        float hoverValue = 0f;
        if (visualEffect != null)
        {
            hoverValue = Mathf.Abs(visualEffect.CurrentHoverOffsetNormalized);
        }

        float scaleMultiplier = 1f - hoverValue * shadowScaleAmount;
        transform.localScale = startLocalScale * scaleMultiplier;
    }
}
