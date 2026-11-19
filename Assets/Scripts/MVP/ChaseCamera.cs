using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>Simple local follow camera for the single-player driving prototype.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class ChaseCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 4f, -8f);
        [SerializeField, Min(0.01f)] private float positionSharpness = 6f;
        [SerializeField, Min(0.01f)] private float rotationSharpness = 8f;
        [SerializeField] private Vector3 lookAtOffset = new Vector3(0f, 1f, 0f);

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            float positionBlend = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            float rotationBlend = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            Vector3 desiredPosition = target.TransformPoint(localOffset);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, positionBlend);

            Vector3 toTarget = target.TransformPoint(lookAtOffset) - transform.position;
            if (toTarget.sqrMagnitude < 0.0001f) return;
            Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationBlend);
        }

        private void OnValidate()
        {
            positionSharpness = Mathf.Max(0.01f, positionSharpness);
            rotationSharpness = Mathf.Max(0.01f, rotationSharpness);
        }
    }
}
