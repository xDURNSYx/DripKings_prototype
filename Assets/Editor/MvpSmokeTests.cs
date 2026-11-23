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
                var raceSettings = new SerializedObject(race);
                raceSettings.FindProperty("checkpointCount").intValue = 0;
                raceSettings.FindProperty("lapsToFinish").intValue = -2;
                raceSettings.ApplyModifiedPropertiesWithoutUndo();
                race.ResetRace();
                raceSettings.Update();
                Check(raceSettings.FindProperty("checkpointCount").intValue == 1 && raceSettings.FindProperty("lapsToFinish").intValue == 1, "invalid race settings clamp to one");
                race.ReachCheckpoint(0);
                Check(race.Finished && race.CompletedLaps == 1, "clamped race completes only after gate");
                raceSettings.FindProperty("checkpointCount").intValue = 4;
                raceSettings.FindProperty("lapsToFinish").intValue = 1;
                raceSettings.ApplyModifiedPropertiesWithoutUndo();
                race.ResetRace();
                // Preserve the operator's existing local progress even if an assertion fails.
                const string creditsKey = "DripKings.MVP.Credits";
                const string paintKey = "DripKings.MVP.Color";
                bool hadCredits = PlayerPrefs.HasKey(creditsKey);
                bool hadPaint = PlayerPrefs.HasKey(paintKey);
                int oldCredits = PlayerPrefs.GetInt(creditsKey);
                int oldPaint = PlayerPrefs.GetInt(paintKey);
                try
                {
                    PlayerPrefs.SetInt(creditsKey, 25);
                    PlayerPrefs.SetInt(paintKey, 0);
                    PlayerPrefs.Save();
                    Check(LocalGarage.BuySavedPaint(), "menu paint purchase succeeds");
                    Check(LocalGarage.SavedCredits == 0 && LocalGarage.SavedPaintIndex == 1, "menu paint purchase persists");
                    Check(!LocalGarage.BuySavedPaint() && LocalGarage.SavedCredits == 0, "insufficient credits rejected");
                    var garage = go.AddComponent<LocalGarage>();
                    garage.AwardCredits(30);
                    Check(LocalGarage.SavedCredits == 30 && LocalGarage.SavedPaintIndex == 1, "race reward preserves menu paint");
                    Check(garage.BuyNextPaint(), "race paint purchase succeeds");
                    Check(LocalGarage.SavedCredits == 5 && LocalGarage.SavedPaintIndex == 2, "race paint purchase persists");
                    garage.AwardCredits(0);
                    garage.AwardCredits(-10);
                    Check(LocalGarage.SavedCredits == 5 && LocalGarage.SavedPaintIndex == 2, "nonpositive reward ignored");
                    PlayerPrefs.SetInt(creditsKey, int.MaxValue - 2);
                    PlayerPrefs.Save();
                    garage.AwardCredits(10);
                    Check(LocalGarage.SavedCredits == int.MaxValue && LocalGarage.SavedPaintIndex == 2, "reward saturates without overflow");
                    // A corrupted serialized price must never mint credits or allow free paint.
                    var serializedGarage = new SerializedObject(garage);
                    var serializedPrice = serializedGarage.FindProperty("paintCost");
                    serializedPrice.intValue = -50;
                    serializedGarage.ApplyModifiedPropertiesWithoutUndo();
                    PlayerPrefs.SetInt(creditsKey, 0);
                    PlayerPrefs.Save();
                    Check(!garage.BuyNextPaint() && LocalGarage.SavedCredits == 0 && LocalGarage.SavedPaintIndex == 2, "negative paint price cannot grant free paint");
                    PlayerPrefs.SetInt(creditsKey, 1);
                    PlayerPrefs.Save();
                    Check(garage.BuyNextPaint() && LocalGarage.SavedCredits == 0 && LocalGarage.SavedPaintIndex == 3, "negative paint price clamps to one credit");
                }
                finally
                {
                    if (hadCredits) PlayerPrefs.SetInt(creditsKey, oldCredits);
                    else PlayerPrefs.DeleteKey(creditsKey);
                    if (hadPaint) PlayerPrefs.SetInt(paintKey, oldPaint);
                    else PlayerPrefs.DeleteKey(paintKey);
                    PlayerPrefs.Save();
                }
                Debug.Log("DRIP_KINGS_MVP_SMOKE_PASS: 18 assertions passed");
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
