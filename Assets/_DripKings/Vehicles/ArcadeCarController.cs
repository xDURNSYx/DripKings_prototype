using UnityEngine;

namespace DripKings.Vehicles
{
    /// <summary>Offline MVP car controller. Attach to a car root with a Rigidbody and collider.
    /// Uses legacy Input axes (Horizontal/Vertical) until an Input System adapter is added.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ArcadeCarController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float acceleration = 24f;
        [SerializeField, Min(0f)] private float reverseAcceleration = 10f;
        [SerializeField, Min(0f)] private float maxForwardSpeed = 30f;
        [SerializeField, Min(0f)] private float maxReverseSpeed = 10f;
        [SerializeField, Min(0f)] private float steeringDegreesPerSecond = 105f;
        [SerializeField, Range(0f, 1f)] private float lateralGrip = 0.85f;
        [SerializeField, Range(0f, 1f)] private float driftingGrip = 0.12f;
        [SerializeField, Min(0f)] private float brakeStrength = 28f;
        private Rigidbody body;
        private float throttle;
        private float steer;
        private bool brake;
        private bool drift;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.centerOfMass = new Vector3(0f, -0.45f, 0f);
        }

        /// <summary>Input adapter API: values are clamped to [-1,1].</summary>
        public void SetInput(float throttleInput, float steeringInput, bool brakeInput, bool driftInput)
        {
            throttle = Mathf.Clamp(throttleInput, -1f, 1f);
            steer = Mathf.Clamp(steeringInput, -1f, 1f);
            brake = brakeInput;
            drift = driftInput;
        }

        private void FixedUpdate()
        {
            Vector3 velocity = body.linearVelocity;
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            float requestedAcceleration = throttle >= 0f ? acceleration : reverseAcceleration;
            float speedLimit = throttle >= 0f ? maxForwardSpeed : maxReverseSpeed;
            if (Mathf.Abs(forwardSpeed) < speedLimit || Mathf.Sign(throttle) != Mathf.Sign(forwardSpeed))
                body.AddForce(transform.forward * (throttle * requestedAcceleration), ForceMode.Acceleration);

            if (brake && velocity.sqrMagnitude > 0.01f)
                body.AddForce(-velocity.normalized * brakeStrength, ForceMode.Acceleration);

            float grip = drift ? driftingGrip : lateralGrip;
            Vector3 sideways = transform.right * Vector3.Dot(body.linearVelocity, transform.right);
            body.AddForce(-sideways * (grip * 10f), ForceMode.Acceleration);

            float steeringScale = Mathf.Clamp01(Mathf.Abs(forwardSpeed) / 3f);
            float direction = forwardSpeed < -0.2f ? -1f : 1f;
            float yaw = steer * direction * steeringDegreesPerSecond * steeringScale * Time.fixedDeltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, yaw, 0f));
        }
    }
}
