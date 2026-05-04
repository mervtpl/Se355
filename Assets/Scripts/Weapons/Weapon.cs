using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class Weapon : ScriptableObject
{
    [Header("Identity")]
    public string weapon_name = "Pistol";

    [Header("Damage")]
    public float damage = 10f;

    [Header("Fire Rate")]
    [Tooltip("Shots per second.")]
    public float fire_rate = 5f;

    [Header("Bullet")]
    public GameObject bullet_prefab;
    public float bullet_speed = 15f;

    [Header("Spread")]
    [Tooltip("Bullets fired per shot. 1 for pistol/rifle, 6 for shotgun.")]
    public int bullets_per_shot = 1;

    [Tooltip("Random spread in degrees. 0 = pinpoint accurate, 15 = shotgun-like.")]
    public float spread_degrees = 0f;
}
