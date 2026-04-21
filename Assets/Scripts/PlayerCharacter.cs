using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


    public class PlayerCharacter : MonoBehaviour
    {
        public int player_id;

        [Header("Stats")]
        public float max_hp = 100f;

        [Header("Status")]
        public bool invulnerable = false;

        [Header("Movement")]
        public float move_accel = 12f;
        public float move_deccel = 14f;
        public float move_max = 3.5f;

        [Header("Dash")]
        public float dash_speed = 9f;
        public float dash_duration = 0.15f;
        public float dash_cooldown = 0.8f;

        [Header("Sprint")]
        public float sprint_speed_multiplier = 1.6f;
        public float sprint_duration = 1.2f;
        public float sprint_cooldown = 2.0f;

        public UnityAction onDeath;
        public UnityAction onHit;

        private Rigidbody2D rigid;
        private Animator animator;

        private float hp;
        private bool is_dead = false;
        private Vector2 move;
        private Vector2 move_input;
        private Vector2 lookat = Vector2.down;
        private float side = 1f;
        private bool disable_controls = false;
        private float hit_timer = 0f;

        private bool is_dashing = false;
        private float dash_timer = 0f;
        private float dash_cooldown_timer = 0f;
        private Vector2 dash_direction = Vector2.zero;

        private bool is_sprinting = false;
        private float sprint_timer = 0f;
        private float sprint_cooldown_timer = 0f;

        private static Dictionary<int, PlayerCharacter> character_list = new Dictionary<int, PlayerCharacter>();

        void Awake()
        {
            character_list[player_id] = this;
            rigid = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            hp = max_hp;
        }

        void OnDestroy()
        {
            if (character_list.ContainsKey(player_id) && character_list[player_id] == this)
                character_list.Remove(player_id);
        }

        void FixedUpdate()
        {
            if (rigid == null)
                return;

            if (is_dashing)
            {
                rigid.linearVelocity = dash_direction * dash_speed;
                return;
            }

            float current_max_speed = move_max;
            if (is_sprinting)
                current_max_speed *= sprint_speed_multiplier;

            float desiredSpeedX = Mathf.Abs(move_input.x) > 0.1f ? move_input.x * current_max_speed : 0f;
            float accelerationX = Mathf.Abs(move_input.x) > 0.1f ? move_accel : move_deccel;
            move.x = Mathf.MoveTowards(move.x, desiredSpeedX, accelerationX * Time.fixedDeltaTime);

            float desiredSpeedY = Mathf.Abs(move_input.y) > 0.1f ? move_input.y * current_max_speed : 0f;
            float accelerationY = Mathf.Abs(move_input.y) > 0.1f ? move_accel : move_deccel;
            move.y = Mathf.MoveTowards(move.y, desiredSpeedY, accelerationY * Time.fixedDeltaTime);

            rigid.linearVelocity = move;
        }

        void Update()
        {
            hit_timer += Time.deltaTime;

            if (dash_cooldown_timer > 0f)
                dash_cooldown_timer -= Time.deltaTime;

            if (sprint_cooldown_timer > 0f)
                sprint_cooldown_timer -= Time.deltaTime;

            UpdateDashState();
            UpdateSprintState();

            move_input = Vector2.zero;

            if (!disable_controls && !is_dead)
            {
                PlayerControls controls = PlayerControls.Get(player_id);

                if (controls != null)
                {
                    move_input = controls.GetMove();

                    HandleDashInput(controls);
                    HandleSprintInput(controls);
                }
            }

            if (!is_dashing && move.magnitude > 0.1f)
                lookat = move.normalized;

            if (Mathf.Abs(lookat.x) > 0.02f)
                side = Mathf.Sign(lookat.x);

            if (animator != null)
            {
                animator.SetFloat("MoveX", move.x);
                animator.SetFloat("MoveY", move.y);
                animator.SetFloat("Speed", move.magnitude);
                animator.SetFloat("FaceX", lookat.x);
                animator.SetFloat("FaceY", lookat.y);

                animator.SetBool("IsDashing", is_dashing);
                animator.SetBool("IsSprinting", is_sprinting);
            }
        }

        void HandleDashInput(PlayerControls controls)
        {
            if (is_dashing)
                return;

            if (controls.GetDashDown() && dash_cooldown_timer <= 0f)
            {
                Vector2 desiredDashDirection = move_input;

                if (desiredDashDirection.magnitude <= 0.01f)
                    desiredDashDirection = lookat;

                if (desiredDashDirection.magnitude <= 0.01f)
                    desiredDashDirection = Vector2.down;

                dash_direction = desiredDashDirection.normalized;
                is_dashing = true;
                dash_timer = dash_duration;
                dash_cooldown_timer = dash_cooldown;

                move = Vector2.zero;
                rigid.linearVelocity = Vector2.zero;
            }
        }

        void HandleSprintInput(PlayerControls controls)
        {
            if (is_dashing)
                return;

            if (is_sprinting)
                return;

            if (controls.GetSprintDown() && sprint_cooldown_timer <= 0f && move_input.magnitude > 0.1f)
            {
                is_sprinting = true;
                sprint_timer = sprint_duration;
            }
        }

        void UpdateDashState()
        {
            if (!is_dashing)
                return;

            dash_timer -= Time.deltaTime;

            if (dash_timer <= 0f)
            {
                is_dashing = false;
            }
        }

        void UpdateSprintState()
        {
            if (!is_sprinting)
                return;

            sprint_timer -= Time.deltaTime;

            if (sprint_timer <= 0f)
            {
                is_sprinting = false;
                sprint_cooldown_timer = sprint_cooldown;
            }
        }

        public void HealDamage(float heal)
        {
            if (!is_dead)
            {
                hp += heal;
                hp = Mathf.Min(hp, max_hp);
            }
        }

        public void TakeDamage(float damage)
        {
            if (!is_dead && !invulnerable && hit_timer > 0f)
            {
                hp -= damage;
                hit_timer = -1f;

                if (hp <= 0f)
                {
                    Kill();
                }
                else
                {
                    onHit?.Invoke();
                }
            }
        }

        public void Kill()
        {
            if (!is_dead)
            {
                is_dead = true;

                if (rigid != null)
                    rigid.linearVelocity = Vector2.zero;

                move = Vector2.zero;
                move_input = Vector2.zero;

                is_dashing = false;
                is_sprinting = false;

                onDeath?.Invoke();
            }
        }

        public void Teleport(Vector3 pos)
        {
            transform.position = pos;
            move = Vector2.zero;
            move_input = Vector2.zero;

            if (rigid != null)
                rigid.linearVelocity = Vector2.zero;
        }

        public Vector2 GetMove()
        {
            return move;
        }

        public Vector2 GetFacing()
        {
            return lookat;
        }

        public int GetSortOrder()
        {
            return 0;
        }

        public float GetSide()
        {
            return side;
        }

        public int GetSideAnim()
        {
            return (side >= 0) ? 1 : 3;
        }

        public bool IsDead()
        {
            return is_dead;
        }

        public bool IsDashing()
        {
            return is_dashing;
        }

        public bool IsSprinting()
        {
            return is_sprinting;
        }

        public float GetDashCooldownRemaining()
        {
            return Mathf.Max(0f, dash_cooldown_timer);
        }

        public float GetSprintCooldownRemaining()
        {
            return Mathf.Max(0f, sprint_cooldown_timer);
        }

        public void DisableControls() { disable_controls = true; }
        public void EnableControls() { disable_controls = false; }

        public static PlayerCharacter GetNearest(Vector3 pos, float range = 999f, bool alive_only = true)
        {
            PlayerCharacter nearest = null;
            float min_dist = range;

            foreach (PlayerCharacter character in character_list.Values)
            {
                if (!alive_only || !character.IsDead())
                {
                    float dist = (pos - character.transform.position).magnitude;
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nearest = character;
                    }
                }
            }
            return nearest;
        }

        public static PlayerCharacter Get(int player_id)
        {
            foreach (PlayerCharacter character in character_list.Values)
            {
                if (character.player_id == player_id)
                    return character;
            }
            return null;
        }

        public static PlayerCharacter[] GetAll()
        {
            PlayerCharacter[] list = new PlayerCharacter[character_list.Count];
            character_list.Values.CopyTo(list, 0);
            return list;
        }
    }
   