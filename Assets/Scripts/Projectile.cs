using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 30f;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, moveDistance);

        if (hit.collider != null)
        {
            int layer = hit.collider.gameObject.layer;

            if (layer == LayerMask.NameToLayer("Enemy"))
            {
                Destroy(hit.collider.gameObject); 
                HitTarget(hit.point);             
                return;                           
            }
            else if (layer == LayerMask.NameToLayer("Obstacle"))
            {
                HitTarget(hit.point);             
                return;                           
            }
        }

        transform.position += transform.up * moveDistance;
    }

    void HitTarget(Vector2 hitPoint)
    {
        transform.position = hitPoint;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collidedObject)
    {
        int layer = collidedObject.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            Destroy(collidedObject.gameObject);
            HitTarget(transform.position);
        }
        else if (layer == LayerMask.NameToLayer("Obstacle"))
        {
            HitTarget(transform.position);
        }
    }
}
