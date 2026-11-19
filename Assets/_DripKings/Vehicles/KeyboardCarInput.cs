using UnityEngine;
using UnityEngine.InputSystem;

namespace DripKings.Vehicles
{
    /// <summary>Temporary keyboard adapter for the offline MVP. Attach beside ArcadeCarController.</summary>
    [RequireComponent(typeof(ArcadeCarController))]
    public sealed class KeyboardCarInput : MonoBehaviour
    {
        private ArcadeCarController controller;

        private void Awake() => controller = GetComponent<ArcadeCarController>();

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                controller.SetInput(0f, 0f, false, false);
                return;
            }

            float throttle = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                           - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            float steering = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                           - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            controller.SetInput(throttle, steering, keyboard.leftCtrlKey.isPressed, keyboard.spaceKey.isPressed);
        }
    }
}
