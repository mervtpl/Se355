using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float damage;
    public float lifetime = 4f;
    
    public void Init(Vector2 dir, float spd, float dmg)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position,
            (Vector2)transform.position + direction, speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}