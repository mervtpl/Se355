using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapons")]
    [Tooltip("Slots 1-9 (keys Alpha1..Alpha9 switch between them).")]
    public Weapon[] weapons;

    [Header("Fire Origin")]
    [Tooltip("Where bullets spawn from. If null, this transform is used.")]
    public Transform fire_point;

    [Header("Aim Source")]
    [Tooltip("Optional. Aim direction is computed from this point to the mouse. If null, fire_point is used.")]
    public Transform aim_source;

    private int current_index = 0;
    private float cooldown_timer = 0f;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        if (fire_point == null)
            fire_point = transform;
    }

    void Update()
    {
        HandleWeaponSwitch();

        if (cooldown_timer > 0f)
            cooldown_timer -= Time.deltaTime;

        if (Input.GetMouseButton(0) && cooldown_timer <= 0f)
            Fire();
    }

    void HandleWeaponSwitch()
    {
        if (weapons == null || weapons.Length == 0)
            return;

        int max_slots = Mathf.Min(weapons.Length, 9);
        for (int i = 0; i < max_slots; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && weapons[i] != null)
                current_index = i;
        }
    }

    void Fire()
    {
        Weapon weapon = GetCurrentWeapon();
        if (weapon == null || weapon.bullet_prefab == null)
            return;

        Vector3 origin = fire_point.position;
        Vector2 aim_dir = GetAimDirection();

        for (int i = 0; i < weapon.bullets_per_shot; i++)
        {
            Vector2 dir = aim_dir;
            if (weapon.spread_degrees > 0f)
            {
                float angle_offset = Random.Range(-weapon.spread_degrees, weapon.spread_degrees);
                dir = (Vector2)(Quaternion.Euler(0f, 0f, angle_offset) * dir);
            }

            GameObject bullet_obj = Instantiate(weapon.bullet_prefab, origin, Quaternion.identity);
            Bullet bullet = bullet_obj.GetComponent<Bullet>();
            if (bullet != null)
                bullet.Launch(dir, weapon.bullet_speed, weapon.damage);
        }

        cooldown_timer = 1f / Mathf.Max(0.01f, weapon.fire_rate);
    }

    Vector2 GetAimDirection()
    {
        if (cam == null)
            cam = Camera.main;

        Transform source = aim_source != null ? aim_source : fire_point;

        if (cam == null)
            return Vector2.right;

        Vector3 mouse_screen = Input.mousePosition;
        mouse_screen.z = Mathf.Abs(cam.transform.position.z - source.position.z);
        Vector3 mouse_world = cam.ScreenToWorldPoint(mouse_screen);

        Vector2 dir = (Vector2)(mouse_world - source.position);
        return dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
    }

    public Weapon GetCurrentWeapon()
    {
        if (weapons == null || weapons.Length == 0)
            return null;
        if (current_index < 0 || current_index >= weapons.Length)
            return null;
        return weapons[current_index];
    }

    public int GetCurrentIndex()
    {
        return current_index;
    }
}
