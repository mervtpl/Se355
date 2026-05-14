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

            float desiredSpeedX = Mathf.Abs(move_input.x) > 0.1f ? move_input.x * move_max : 0f;
            float accelerationX = Mathf.Abs(move_input.x) > 0.1f ? move_accel : move_deccel;
            move.x = Mathf.MoveTowards(move.x, desiredSpeedX, accelerationX * Time.fixedDeltaTime);

            float desiredSpeedY = Mathf.Abs(move_input.y) > 0.1f ? move_input.y * move_max : 0f;
            float accelerationY = Mathf.Abs(move_input.y) > 0.1f ? move_accel : move_deccel;
            move.y = Mathf.MoveTowards(move.y, desiredSpeedY, accelerationY * Time.fixedDeltaTime);

            rigid.linearVelocity = move;
        }

        void Update()
        {
            hit_timer += Time.deltaTime;

            move_input = Vector2.zero;

            if (!disable_controls && !is_dead)
            {
                PlayerControls controls = PlayerControls.Get(player_id);

                if (controls != null)
                {
                    move_input = controls.GetMove();
                }
            }

            if (move.magnitude > 0.1f)
                lookat = move.normalized;

            if (Mathf.Abs(lookat.x) > 0.02f)
                side = Mathf.Sign(lookat.x);

            if (animator != null)
            {
                animator.SetFloat("Speed", move.magnitude);
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
