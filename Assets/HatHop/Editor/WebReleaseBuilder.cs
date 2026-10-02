using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HatHop.Editor
{
    public static class WebReleaseBuilder
    {
        public const string Output = "Builds/WebGL";
        private static readonly string[] Scenes = {
            "Assets/HatHop/Scenes/MainMenu.unity", "Assets/HatHop/Scenes/Prologue.unity",
            "Assets/HatHop/Scenes/Easy.unity", "Assets/HatHop/Scenes/Medium.unity", "Assets/HatHop/Scenes/Hard.unity"
        };

        [MenuItem("Leap of Faith/Build Web Release")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Stop Play Mode before building.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string path in Scenes)
                if (!File.Exists(path)) throw new InvalidOperationException("Missing " + path + ". Run Leap of Faith > Prepare Four-Level Release first, test and save the scenes.");
            LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(MainMenuSceneBuilder.CatalogPath);
            if (catalog == null || catalog.prologueScenePath != Scenes[1] || catalog.easyScenePath != Scenes[2] ||
                catalog.mediumScenePath != Scenes[3] || catalog.hardScenePath != Scenes[4])
                throw new InvalidOperationException("Level catalog is not mapped to the four release scenes. Prepare Four-Level Release first.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Install Web Build Support for Unity 6000.3.23f1 in Unity Hub.");
            PlayerSettings.productName = "Leap of Faith";
            // Pages does not provide custom Content-Encoding headers for precompressed Unity files.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            AssetDatabase.SaveAssets();
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = Scenes, locationPathName = Output, target = BuildTarget.WebGL, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Web build failed: " + report.summary.result + ". Inspect the first Console error.");
            File.WriteAllText(Path.Combine(Output, ".nojekyll"), "");
            File.WriteAllText(Path.Combine(Output, "release-info.json"),
                "{\"title\":\"Leap of Faith\",\"levels\":4,\"unity\":\"" + Application.unityVersion +
                "\",\"builtUtc\":\"" + DateTime.UtcNow.ToString("o") + "\"}");
            Debug.Log("Web release built in " + Output + ". Test it over HTTP before publishing.");
        }
    }
}
