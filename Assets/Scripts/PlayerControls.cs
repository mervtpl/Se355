using System.Collections;
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

        [Header("Action Keys")]
        public KeyCode dash_key = KeyCode.Space;
        public KeyCode sprint_key = KeyCode.LeftShift;

        private Vector2 move = Vector2.zero;

        private bool dash_press = false;
        private bool dash_hold = false;

        private bool sprint_press = false;
        private bool sprint_hold = false;

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

            dash_press = false;
            dash_hold = false;

            sprint_press = false;
            sprint_hold = false;

            if (Input.GetKey(left_key))
                move += Vector2.left;
            if (Input.GetKey(right_key))
                move += Vector2.right;
            if (Input.GetKey(up_key))
                move += Vector2.up;
            if (Input.GetKey(down_key))
                move += Vector2.down;

            if (Input.GetKey(dash_key))
                dash_hold = true;
            if (Input.GetKeyDown(dash_key))
                dash_press = true;

            if (Input.GetKey(sprint_key))
                sprint_hold = true;
            if (Input.GetKeyDown(sprint_key))
                sprint_press = true;

            move = Vector2.ClampMagnitude(move, 1f);
        }

        public Vector2 GetMove()
        {
            return move;
        }

        public bool GetDashDown()
        {
            return dash_press;
        }

        public bool GetDashHold()
        {
            return dash_hold;
        }

        public bool GetSprintDown()
        {
            return sprint_press;
        }

        public bool GetSprintHold()
        {
            return sprint_hold;
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