using UnityEngine;

namespace DripKings.MVP
{
    [RequireComponent(typeof(Collider))]
    public sealed class RaceCheckpoint : MonoBehaviour
    {
        [SerializeField] private int index;
        // Must be serialized: scene generation configures this in edit mode,
        // and Unity recreates components when the saved scene is loaded.
        [SerializeField] private CheckpointRace race;

        public void Configure(CheckpointRace owner, int checkpointIndex)
        {
            race = owner;
            index = checkpointIndex;
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (race != null && other.GetComponentInParent<ArcadeCarController>() != null)
                race.ReachCheckpoint(index);
        }
    }
}
