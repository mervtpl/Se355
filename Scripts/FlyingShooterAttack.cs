using UnityEngine;

public class FlyingShooterAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootRange = 8f;
    public float fireRate = 1.2f;
    public float projectileSpeed = 8f;
    public float projectileLifeTime = 3f;
    public int projectileDamage = 1;

    private Transform playerTarget;
    private float nextShotTime;

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        if (projectilePrefab == null || firePoint == null || Time.time < nextShotTime)
        {
            return;
        }

        Vector2 direction = playerTarget.position - firePoint.position;
        if (direction.magnitude > shootRange)
        {
            return;
        }

        Fire(direction.normalized);
        nextShotTime = Time.time + fireRate;
    }

    private void Fire(Vector2 direction)
    {
        GameObject projectileObject = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, direction));
        EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.speed = projectileSpeed;
            projectile.lifeTime = projectileLifeTime;
            projectile.damage = projectileDamage;
            projectile.Configure(direction, gameObject);
        }

        projectileObject.SetActive(true);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
        }
    }
}
