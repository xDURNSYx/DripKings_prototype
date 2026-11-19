using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>Offline checkpoint sequence and lap timer. Trigger gates require a collider marked Is Trigger.</summary>
    public sealed class CheckpointRace : MonoBehaviour
    {
        [SerializeField] private int checkpointCount = 4;
        [SerializeField] private int lapsToFinish = 1;
        private int nextCheckpoint;
        private int completedLaps;
        private float startTime;
        private float finishTime;
        private bool finished;

        public int NextCheckpoint => nextCheckpoint;
        public int CompletedLaps => completedLaps;
        public bool Finished => finished;
        public float ElapsedSeconds => finished ? finishTime : Time.time - startTime;

        private void Start() { ResetRace(); }

        public void ResetRace()
        {
            nextCheckpoint = 0;
            completedLaps = 0;
            finished = false;
            startTime = Time.time;
            finishTime = 0f;
        }

        public void ReachCheckpoint(int index)
        {
            if (finished || index != nextCheckpoint) return;
            nextCheckpoint++;
            if (nextCheckpoint < checkpointCount) return;
            nextCheckpoint = 0;
            completedLaps++;
            if (completedLaps >= lapsToFinish)
            {
                finishTime = Time.time - startTime;
                finished = true;
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(16, 16, 420, 28), finished
                ? $"FINISH! {finishTime:F2}s"
                : $"Lap {completedLaps + 1}/{lapsToFinish}  Gate {nextCheckpoint + 1}/{checkpointCount}  Time {ElapsedSeconds:F2}s");
            GUI.Label(new Rect(16, 42, 550, 25), "WASD / Arrows drive | Space drift | Shift brake | R restart");
        }
    }

}
