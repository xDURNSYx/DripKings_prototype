using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    // Unity CLI arg: -buildName "DripKings"
    private static string GetArg(string name, string defaultValue)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return defaultValue;
    }

    public static void BuildWindows()
    {
        var buildName = GetArg("-buildName", "DripKings");
        var outputDir = Path.Combine("Builds", "Windows");
        Directory.CreateDirectory(outputDir);

        var exePath = Path.Combine(outputDir, buildName + ".exe");

        // Use enabled Build Settings scenes (fixes hardcoded SampleScene path failures)
        var scenes = EditorBuildSettings.scenes
            .Where(s => s != null && s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("No enabled scenes found in Build Settings (File > Build Settings...).");
            throw new Exception("Build failed: no enabled scenes.");
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        Debug.Log("Building Windows player to: " + exePath);
        Debug.Log("Scenes: " + string.Join(", ", scenes));

        BuildReport report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        Debug.Log($"Build Finished, Result: {summary.result}. Size: {summary.totalSize} bytes. Time: {summary.totalTime}.");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Build failed");
        }
    }
}
