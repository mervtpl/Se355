using UnityEngine;

public class FlyingVisualEffect : MonoBehaviour
{
    public float visualHoverAmount = 0.18f;
    public float visualHoverSpeed = 3.2f;
    public float tiltAmount = 8f;
    public float tiltSpeed = 2.6f;
    public float pulseAmount = 0.06f;
    public float pulseSpeed = 2.2f;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private Vector3 startLocalScale;
    private float randomOffset;

    public float CurrentHoverOffsetNormalized { get; private set; }

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        startLocalScale = transform.localScale;
        randomOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float hoverWave = Mathf.Sin((Time.time + randomOffset) * visualHoverSpeed);
        float tiltWave = Mathf.Sin((Time.time + randomOffset) * tiltSpeed);
        float pulseWave = Mathf.Sin((Time.time + randomOffset) * pulseSpeed);

        CurrentHoverOffsetNormalized = hoverWave;
        transform.localPosition = startLocalPosition + Vector3.up * (hoverWave * visualHoverAmount);
        transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, tiltWave * tiltAmount);

        float pulseScale = 1f + pulseWave * pulseAmount;
        transform.localScale = startLocalScale * pulseScale;
    }
}
