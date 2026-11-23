using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>Offline checkpoint sequence and lap timer. Trigger gates require a collider marked Is Trigger.</summary>
    public sealed class CheckpointRace : MonoBehaviour
    {
        [SerializeField] private int checkpointCount = 4;
        [SerializeField] private int lapsToFinish = 1;
        [SerializeField] private LocalGarage garage;
        [SerializeField, Min(0)] private int finishReward = 50;

        public void Configure(LocalGarage targetGarage) => garage = targetGarage;
        private int nextCheckpoint;
        private int completedLaps;
        private float startTime;
        private float finishTime;
        private bool finished;
        private int awardedCredits;

        public int NextCheckpoint => nextCheckpoint;
        public int CompletedLaps => completedLaps;
        public bool Finished => finished;
        public int AwardedCredits => awardedCredits;
        public float ElapsedSeconds => finished ? finishTime : Time.time - startTime;

        private void Start() { ResetRace(); }

        public void ResetRace()
        {
            // Serialized values can be corrupted or edited outside the Inspector.
            // Never allow a zero-gate or zero-lap race to award credits.
            checkpointCount = Mathf.Max(1, checkpointCount);
            lapsToFinish = Mathf.Max(1, lapsToFinish);
            nextCheckpoint = 0;
            completedLaps = 0;
            finished = false;
            startTime = Time.time;
            finishTime = 0f;
            awardedCredits = 0;
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
                if (garage != null && finishReward > 0)
                {
                    int before = LocalGarage.SavedCredits;
                    garage.AwardCredits(finishReward);
                    awardedCredits = Mathf.Max(0, LocalGarage.SavedCredits - before);
                }
            }
        }

        private void OnGUI()
        {
            if (!DripKingsMenuController.IsGameplayActive) return;
            GUI.Label(new Rect(16, 16, 420, 28), finished
                ? $"FINISH! {finishTime:F2}s | +{awardedCredits} credits"
                : $"Lap {completedLaps + 1}/{lapsToFinish}  Gate {nextCheckpoint + 1}/{checkpointCount}  Time {ElapsedSeconds:F2}s");
            GUI.Label(new Rect(16, 42, 550, 25), "WASD / Arrows drive | Space drift | Shift brake | R restart");
        }
    }

}
