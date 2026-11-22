#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DripKings.EditorTools
{
    /// <summary>Standalone game builds always open on the branded title menu.</summary>
    public static class MvpWindowsBuild
    {
        [MenuItem("Drip Kings/MVP/Build Windows Development Player")]
        public static void Build() => BuildTo(Path.Combine("Builds", "MVP"));

        // Keep the previously verified Windows executable intact during menu development.
        [MenuItem("Drip Kings/MVP/Build Branded Menu Preview")]
        public static void BuildMenuPreview() => BuildTo(Path.Combine("Builds", "MVPMenu"));

        private static void BuildTo(string outputFolder)
        {
            const string menuScene = "Assets/DripKings_StartMenu.unity";
            const string raceScene = "Assets/DripKings_MVP_Test.unity";
            if (!File.Exists(menuScene) || !File.Exists(raceScene))
                throw new FileNotFoundException("Install the branded menu and preserve the MVP race scene before building.");

            if (Directory.Exists(outputFolder))
                throw new IOException(outputFolder + " already exists. Preserve or rename before generating a new build.");

            Directory.CreateDirectory(outputFolder);
            string executable = Path.Combine(outputFolder, "DripKings.exe");

            var options = new BuildPlayerOptions
            {
                scenes = new[] { menuScene, raceScene },
                locationPathName = executable,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report == null || report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed. Inspect the Unity Editor build report.");

            Debug.Log("DRIP_KINGS_MVP_BUILD_PASS: " + executable + " | bytes: " + report.summary.totalSize);
            Debug.Log("DRIP_KINGS_MENU_BUILD_PASS: " + menuScene + " -> " + raceScene);
        }
    }
}
#endif
