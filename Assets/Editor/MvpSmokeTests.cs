#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using DripKings.MVP;

namespace DripKings.EditorTools
{
    /// <summary>Deterministic Edit Mode smoke checks; callable from menu or Unity -executeMethod.</summary>
    public static class MvpSmokeTests
    {
        [MenuItem("Drip Kings/MVP/Run Smoke Tests")]
        public static void Run()
        {
            GameObject go = new GameObject("MVP Smoke Test Race");
            try
            {
                var race = go.AddComponent<CheckpointRace>();
                race.ResetRace();
                Check(race.NextCheckpoint == 0 && race.CompletedLaps == 0 && !race.Finished, "initial state");

                race.ReachCheckpoint(1);
                Check(race.NextCheckpoint == 0, "out-of-order gate ignored");

                race.ReachCheckpoint(0);
                race.ReachCheckpoint(0);
                Check(race.NextCheckpoint == 1, "duplicate gate ignored");

                race.ReachCheckpoint(1);
                race.ReachCheckpoint(2);
                race.ReachCheckpoint(3);
                Check(race.Finished && race.CompletedLaps == 1, "four gates finish lap");

                race.ReachCheckpoint(0);
                Check(race.CompletedLaps == 1, "finish cannot repeat");

                race.ResetRace();
                Check(!race.Finished && race.CompletedLaps == 0 && race.NextCheckpoint == 0, "restart clears progress");
                Debug.Log("DRIP_KINGS_MVP_SMOKE_PASS: 6 assertions passed");
            }
            catch (Exception exception)
            {
                Debug.LogError("DRIP_KINGS_MVP_SMOKE_FAIL: " + exception);
                // In headless automation, explicitly return failure rather than relying on Unity logs.
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static void Check(bool condition, string label)
        {
            if (!condition) throw new InvalidOperationException("DRIP_KINGS_MVP_SMOKE_FAIL: " + label);
        }
    }
}
#endif
