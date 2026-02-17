using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMover : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        var moveInput = ReadMoveInput();
        var move = new Vector3(moveInput.x, 0f, moveInput.y);
        if (move.sqrMagnitude > 0.0001f)
        {
            SubmitMoveServerRpc(move * speed * Time.deltaTime);
        }
    }

    private static Vector2 ReadMoveInput()
    {
        var input = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            var stick = gamepad.leftStick.ReadValue();
            if (stick.sqrMagnitude > input.sqrMagnitude)
            {
                input = stick;
            }
        }

        return input;
    }

    [ServerRpc]
    private void SubmitMoveServerRpc(Vector3 delta)
    {
        transform.position += delta;
    }
}
