using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildHelper
{
    [UnityEditor.MenuItem("Build/Build Mac (Desktop)")]
    public static void BuildMac()
    {
        string outputPath = System.Environment.GetEnvironmentVariable("BUILD_OUTPUT_PATH")
            ?? "/Users/dimitriberardi/Home/Round To It Studio/Minesweeper/Builds/GardenSweeper.app";

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Game.unity" },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {outputPath}");
        else
            Debug.LogError($"Build failed: {report.summary.totalErrors} errors");
    }
}
