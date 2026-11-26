#if UNITY_EDITOR
using System;
using DripKings.MVP;
using UnityEditor;
using UnityEngine;

namespace DripKings.EditorTools
{
    /// <summary>Validate tablet app routing and run the existing commerce/race regressions.</summary>
    public static class TabletSmokeTests
    {
        [MenuItem("Drip Kings/MVP/Run Tablet and Economy Smoke Tests")]
        public static void Run()
        {
            GameObject objectForTest = null;
            try
            {
                objectForTest = new GameObject("Drip Kings Tablet Route Smoke");
                var tablet = objectForTest.AddComponent<DripKingsTabletView>();
                int count = 0;
                Assert(tablet.AtHome, "initial page is app launcher", ref count);
                foreach (DripKingsTabletView.App app in Enum.GetValues(typeof(DripKingsTabletView.App)))
                {
                    tablet.OpenApp(app);
                    Assert(tablet.CurrentApp == app, "app route " + app, ref count);
                    tablet.GoHome();
                    Assert(tablet.AtHome, "home return for " + app, ref count);
                }
                tablet.OpenApp((DripKingsTabletView.App)999);
                Assert(tablet.AtHome, "unknown app rejected", ref count);

                // Existing save-state tests snapshot and restore game credits, colors and ownership.
                ClassifiedsSmokeTests.Run();
                MvpSmokeTests.Run();
                Debug.Log("DRIP_KINGS_TABLET_PASS: " + count +
                    " navigation assertions; classifieds and racing regression suites invoked.");
            }
            catch (Exception exception)
            {
                Debug.LogError("DRIP_KINGS_TABLET_FAIL: " + exception);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
            finally
            {
                if (objectForTest != null)
                    UnityEngine.Object.DestroyImmediate(objectForTest);
            }
        }

        private static void Assert(bool condition, string message, ref int count)
        {
            if (!condition) throw new InvalidOperationException(message);
            count++;
        }
    }
}
#endif
