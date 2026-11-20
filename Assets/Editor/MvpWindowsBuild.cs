#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DripKings.EditorTools
{
    /// <summary>Explicit Windows development build; never overwrites an existing output.</summary>
    public static class MvpWindowsBuild
    {
        [MenuItem("Drip Kings/MVP/Build Windows Development Player")]
        public static void Build()
        {
            const string scene = "Assets/DripKings_MVP_Test.unity";
            if (!File.Exists(scene))
                throw new FileNotFoundException("Generate and save the MVP test scene before building.", scene);

            string outputFolder = Path.Combine("Builds", "MVP");
            string executable = Path.Combine(outputFolder, "DripKings.exe");
            if (Directory.Exists(outputFolder))
                throw new IOException("Builds/MVP already exists. Move or rename it before creating another build.");

            Directory.CreateDirectory(outputFolder);
            try
            {
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = executable,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development | BuildOptions.AllowDebugging
                };
                BuildReport report = BuildPipeline.BuildPlayer(options);
                if (report == null || report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("MVP Windows build failed. Inspect the Unity Editor log and build report.");

                Debug.Log("DRIP_KINGS_MVP_BUILD_PASS: " + executable +
                          " | bytes: " + report.summary.totalSize);
            }
            catch
            {
                // Preserve any partial output for investigation; do not delete user files.
                throw;
            }
        }
    }
}
#endif
