using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class PlayerCubeController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float sprintMultiplier = 1.8f;

    private PlayerHealthFeedback healthFeedback;

    private void Awake()
    {
        healthFeedback = GetComponent<PlayerHealthFeedback>();
    }

    private void Update()
    {
        if (healthFeedback != null && healthFeedback.IsInfected)
        {
            return;
        }

        var moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude < 0.0001f)
        {
            return;
        }

        var speed = IsSprintHeld() ? moveSpeed * sprintMultiplier : moveSpeed;
        var movement = new Vector3(moveInput.x, 0f, moveInput.y).normalized * speed * Time.deltaTime;
        transform.position += movement;
    }

    private static Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        var input = Vector2.zero;
        if (keyboard.aKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            input.x += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            input.y -= 1f;
        }

        if (keyboard.wKey.isPressed)
        {
            input.y += 1f;
        }

        return input;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#else
        return Vector2.zero;
#endif
    }

    private static bool IsSprintHeld()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#else
        return false;
#endif
    }
}
