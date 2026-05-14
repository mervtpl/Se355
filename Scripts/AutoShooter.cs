using UnityEngine;

public class AutoShooter : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRange = 7f;
    public float fireRate = 0.35f;
    public float bulletSpeed = 9f;
    public int bulletDamage = 1;
    public float bulletLifeTime = 2f;

    private float nextFireTime;

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.AllowsGameplayActions)
        {
            return;
        }

        if (bulletPrefab == null || Time.time < nextFireTime)
        {
            return;
        }

        Transform target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        Vector2 origin = firePoint != null ? firePoint.position : transform.position;
        Vector2 direction = (Vector2)target.position - origin;
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Fire(direction.normalized);
        nextFireTime = Time.time + fireRate;
    }

    private void Fire(Vector2 direction)
    {
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Quaternion rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        GameObject bulletObject = Instantiate(bulletPrefab, spawnPosition, rotation);

        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.speed = bulletSpeed;
            bullet.damage = bulletDamage;
            bullet.lifeTime = bulletLifeTime;
            bullet.Configure(direction, gameObject);
        }

        bulletObject.SetActive(true);
    }

    private Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float nearestDistanceSqr = fireRange * fireRange;
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy == null || !enemy.activeInHierarchy)
            {
                continue;
            }

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null && health.IsDead)
            {
                continue;
            }

            float distanceSqr = (enemy.transform.position - origin).sqrMagnitude;
            if (distanceSqr <= nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearest = enemy.transform;
            }
        }

        return nearest;
    }
}
