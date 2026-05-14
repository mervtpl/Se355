using UnityEngine;

public class IdleBreath : MonoBehaviour
{
    [Tooltip("Y-axis scale variation. 0.025 = ±2.5%.")]
    public float amplitude = 0.025f;

    [Tooltip("Breath cycles per second. 0.35 = one breath every ~2.85s.")]
    public float frequency = 0.35f;

    [Tooltip("Above this velocity magnitude, breathing eases out to neutral.")]
    public float velocityThreshold = 0.1f;

    private Vector3 baseScale;
    private Rigidbody2D rigid;
    private float phase;

    void Awake()
    {
        baseScale = transform.localScale;
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        bool isIdle = rigid == null || rigid.linearVelocity.magnitude < velocityThreshold;

        float targetOffset;
        if (isIdle)
        {
            phase += Time.deltaTime * frequency * Mathf.PI * 2f;
            targetOffset = Mathf.Sin(phase) * amplitude;
        }
        else
        {
            phase = 0f;
            targetOffset = 0f;
        }

        float currentOffset = (transform.localScale.y / baseScale.y) - 1f;
        float newOffset = Mathf.MoveTowards(currentOffset, targetOffset, amplitude * 4f * Time.deltaTime);

        transform.localScale = new Vector3(
            baseScale.x,
            baseScale.y * (1f + newOffset),
            baseScale.z
        );
    }
}
