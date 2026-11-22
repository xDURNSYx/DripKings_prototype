using UnityEngine;
using UnityEngine.InputSystem;

namespace DripKings.MVP
{
    /// <summary>Reset the offline test vehicle and race using R; independent of network gameplay.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class MvpRaceRestart : MonoBehaviour
    {
        [SerializeField] private CheckpointRace race;
        private Rigidbody body;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;

        public void Configure(CheckpointRace targetRace) => race = targetRace;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void Update()
        {
            if (DripKingsMenuController.IsGameplayActive && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                Restart();
        }

        public void Restart()
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = spawnPosition;
            body.rotation = spawnRotation;
            body.Sleep();
            if (race != null) race.ResetRace();
        }
    }
}
