using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class SpriteFlipper : MonoBehaviour
{
    [Tooltip("Minimum horizontal velocity required to flip. Prevents jitter when nearly stopped.")]
    public float threshold = 0.05f;

    private SpriteRenderer sprite;
    private Rigidbody2D rigid;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float vx = rigid.linearVelocity.x;
        if (vx < -threshold)
            sprite.flipX = true;
        else if (vx > threshold)
            sprite.flipX = false;
    }
}
