using UnityEngine;
using UnityEngine.InputSystem;

namespace DripKings.MVP
{
    /// <summary>Standalone Rigidbody arcade car for the first offline driving test.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float acceleration = 22f;
        [SerializeField, Min(0f)] private float reverseAcceleration = 11f;
        [SerializeField, Min(0f)] private float topSpeed = 30f;
        [SerializeField, Min(0f)] private float steeringDegreesPerSecond = 76f;
        [SerializeField, Min(0f)] private float steeringResponse = 3.5f;
        [SerializeField, Min(0f)] private float normalGrip = 11f;
        [SerializeField, Min(0f)] private float handbrakeGrip = 2.5f;
        [SerializeField, Min(0f)] private float handbrakeDeceleration = 14f;
        [SerializeField, Min(0f)] private float braking = 35f;
        [SerializeField, Min(0f)] private float rollingDrag = 0.7f;

        private Rigidbody body;
        private float smoothedSteering;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            float throttle = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
                           - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            float steer = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
                        - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            // Space is a handbrake, not a magic drift command: it slows the rear axle's
            // grip and decelerates the car; slides emerge from speed and steering.
            bool handbrake = keyboard.spaceKey.isPressed;
            bool brakingNow = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            smoothedSteering = Mathf.MoveTowards(smoothedSteering, steer,
                steeringResponse * Time.fixedDeltaTime);

            Vector3 velocity = body.linearVelocity;
            Vector3 horizontal = Vector3.ProjectOnPlane(velocity, Vector3.up);
            float forwardSpeed = Vector3.Dot(horizontal, transform.forward);
            float steerScale = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 4f);
            float speedSteerReduction = Mathf.Lerp(1f, 0.55f, Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 25f));
            float direction = Mathf.Abs(forwardSpeed) < 0.1f ? 1f : Mathf.Sign(forwardSpeed);
            float yaw = smoothedSteering * direction * steerScale * speedSteerReduction
                        * steeringDegreesPerSecond * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw, 0f));

            float drive = throttle >= 0f ? acceleration : reverseAcceleration;
            if (horizontal.magnitude < topSpeed || Vector3.Dot(horizontal, transform.forward * throttle) < 0f)
                body.AddForce(transform.forward * (throttle * drive), ForceMode.Acceleration);

            Vector3 lateral = Vector3.Project(horizontal, transform.right);
            body.AddForce(-lateral * (handbrake ? handbrakeGrip : normalGrip), ForceMode.Acceleration);
            body.AddForce(-horizontal * rollingDrag, ForceMode.Acceleration);
            // Foot brake and emergency brake are physical deceleration forces.
            // Combining them does not apply two full stops in one physics frame.
            float deceleration = Mathf.Max(brakingNow ? braking : 0f,
                handbrake ? handbrakeDeceleration : 0f);
            if (deceleration > 0f && horizontal.sqrMagnitude > 0.001f)
            {
                float reduction = Mathf.Min(horizontal.magnitude, deceleration * Time.fixedDeltaTime);
                body.AddForce(-horizontal.normalized * reduction, ForceMode.VelocityChange);
            }
        }
    }
}
