using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float max_hp = 50f;

    [Header("Movement")]
    public float move_speed = 2f;
    public float move_accel = 4f;
    public float preferred_range = 3.5f;
    public float too_close_range = 2f;

    [Header("Player Detection")]
    public float detection_range = 6f;
    public float lose_range = 8f;

    [Header("Attack")]
    public float attack_range = 3.5f;
    public float fire_rate = 1.5f;
    public float aim_duration = 0.6f;
    public float bullet_speed = 5f;
    public float bullet_damage = 10f;
    public GameObject bullet_prefab;

    private enum State { WaitState, Chase, Retreat, Aim, Fire }
    private State state = State.WaitState;

    private float hp;
    private bool is_dead = false;

    private Rigidbody2D rigid;

    private Vector2 current_velocity = Vector2.zero;
    
    private float fire_cooldown_timer = 1.5f;
    private float aim_timer = 0f;
    private Vector2 aim_direction = Vector2.zero;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        hp = max_hp;

        if (rigid != null)
        {
            rigid.gravityScale = 0f;
            rigid.freezeRotation = true;
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        if (is_dead) return;

        PlayerCharacter player = PlayerCharacter.GetNearest(transform.position, 999f, true);
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.transform.position);

        UpdateState(player, dist);

        if (fire_cooldown_timer > 0f)
            fire_cooldown_timer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (is_dead) return;
        if (rigid != null)
            rigid.linearVelocity = current_velocity;
    }

    void UpdateState(PlayerCharacter player, float dist)
    {
        switch (state)
        {
            case State.WaitState:
                WaitStateManage(player, dist);
                break;
            case State.Chase:
                ChaseManage(player, dist);
                break;
            case State.Retreat:
                RetreatManage(player, dist);
                break;
            case State.Aim:
                AimManage(player, dist);
                break;
            case State.Fire:
                state = State.Chase;
                fire_cooldown_timer = fire_rate;
                break;
        }
    }

    void WaitStateManage(PlayerCharacter player, float dist)
    {
        SetTargetVelocity(Vector2.zero);
        if (dist <= detection_range)
            state = State.Chase;
    }

    void ChaseManage(PlayerCharacter player, float dist)
    {
        if (dist > lose_range)
        {
            state = State.WaitState;
            SetTargetVelocity(Vector2.zero);
            return;
        }

        if (dist < too_close_range)
        {
            state = State.Retreat;
            return;
        }

        if (dist <= attack_range && fire_cooldown_timer <= 0f)
        {
            aim_direction = (player.transform.position - transform.position).normalized;
            aim_timer = aim_duration;
            state = State.Aim;
            SetTargetVelocity(Vector2.zero);
            return;
        }

        if (dist > preferred_range)
        {
            Vector2 dir = (player.transform.position - transform.position).normalized;
            SetTargetVelocity(dir * move_speed);
        }
        else
        {
            SetTargetVelocity(Vector2.zero);
        }
    }

    void RetreatManage(PlayerCharacter player, float dist)
    {
        if (dist >= too_close_range + 0.5f)
        {
            state = State.Chase;
            return;
        }

        Vector2 dir = (transform.position - player.transform.position).normalized;
        SetTargetVelocity(dir * move_speed);
    }

    void AimManage(PlayerCharacter player, float dist)
    {
        aim_timer -= Time.deltaTime;
        SetTargetVelocity(Vector2.zero);

        if (dist > attack_range)
        {
            state = State.Chase;
            fire_cooldown_timer = fire_rate;
            return;
        }

        if (aim_timer <= 0f)
        {
            FireBullet();
            state = State.Fire;
        }
    }

    void SetTargetVelocity(Vector2 target)
    {
        current_velocity = Vector2.MoveTowards(current_velocity, target, move_accel * Time.deltaTime);
    }

    void FireBullet()
    {
        if (bullet_prefab == null) return;

        Vector2 spawn_pos = (Vector2)transform.position + aim_direction * 0.3f;
        GameObject obj = Instantiate(bullet_prefab, spawn_pos, Quaternion.identity);

        Bullet b = obj.GetComponent<Bullet>();
        if (b != null)
            b.Init(aim_direction, bullet_speed, bullet_damage);
    }

    public void TakeDamage(float damage)
    {
        if (is_dead) return;

        hp -= damage;

        if (state == State.WaitState)
            state = State.Chase;

        if (hp <= 0f)
            Die();
    }

    void Die()
    {
        is_dead = true;
        if (rigid != null) rigid.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }
}