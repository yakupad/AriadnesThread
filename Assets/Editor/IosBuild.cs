using UnityEditor;
using UnityEditor.Build.Reporting;

namespace AriadnesThread.EditorTools
{
    public static class IosBuild
    {
        private const string DeviceOutputPath = "Builds/iOS";
        private const string SimulatorOutputPath = "Builds/iOSSimulator";

        [MenuItem("AriadnesThread/Build iOS Xcode Project (Device)")]
        public static void BuildDeviceXcodeProject()
        {
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            Build(DeviceOutputPath);
        }

        [MenuItem("AriadnesThread/Build iOS Xcode Project (Simulator)")]
        public static void BuildSimulatorXcodeProject()
        {
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;
            Build(SimulatorOutputPath);
        }

        private static void Build(string outputPath)
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Prototype.unity" },
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            });

            UnityEngine.Debug.Log($"iOS build result: {report.summary.result}, total errors: {report.summary.totalErrors}, output: {outputPath}");
        }
    }
}
