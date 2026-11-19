using UnityEngine;

namespace DripKings.MVP
{
    [RequireComponent(typeof(Collider))]
    public sealed class RaceCheckpoint : MonoBehaviour
    {
        [SerializeField] private int index;
        private CheckpointRace race;

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
