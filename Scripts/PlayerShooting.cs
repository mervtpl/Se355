using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 14f;
    public float fireRate = 0.18f;
    public float bulletLifetime = 2.5f;
    public int bulletDamage = 1;

    private float nextShotTime;

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            return;
        }

        if (Mouse.current == null || bulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (Mouse.current.leftButton.isPressed && Time.time >= nextShotTime)
        {
            Fire();
        }
    }

    private void Fire()
    {
        GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.damage = bulletDamage;
            bullet.speed = bulletSpeed;
            bullet.lifeTime = bulletLifetime;
            bullet.Configure(firePoint.right, gameObject);
        }

        bulletObject.SetActive(true);

        nextShotTime = Time.time + fireRate;
    }
}
