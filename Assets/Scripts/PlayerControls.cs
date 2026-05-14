using System.Collections.Generic;
using UnityEngine;


    public class PlayerControls : MonoBehaviour
    {
        public int player_id;

        [Header("Move Keys")]
        public KeyCode left_key = KeyCode.A;
        public KeyCode right_key = KeyCode.D;
        public KeyCode up_key = KeyCode.W;
        public KeyCode down_key = KeyCode.S;

        private Vector2 move = Vector2.zero;

        private static Dictionary<int, PlayerControls> controls = new Dictionary<int, PlayerControls>();

        void Awake()
        {
            controls[player_id] = this;
        }

        void OnDestroy()
        {
            if (controls.ContainsKey(player_id) && controls[player_id] == this)
                controls.Remove(player_id);
        }

        void Update()
        {
            move = Vector2.zero;

            if (Input.GetKey(left_key))
                move += Vector2.left;
            if (Input.GetKey(right_key))
                move += Vector2.right;
            if (Input.GetKey(up_key))
                move += Vector2.up;
            if (Input.GetKey(down_key))
                move += Vector2.down;

            move = Vector2.ClampMagnitude(move, 1f);
        }

        public Vector2 GetMove()
        {
            return move;
        }

        public static PlayerControls Get(int player_id)
        {
            controls.TryGetValue(player_id, out PlayerControls control);
            return control;
        }

        public static PlayerControls[] GetAll()
        {
            PlayerControls[] list = new PlayerControls[controls.Count];
            controls.Values.CopyTo(list, 0);
            return list;
        }
    }
