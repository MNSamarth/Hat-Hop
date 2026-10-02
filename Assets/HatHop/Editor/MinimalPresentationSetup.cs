using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop.Editor
{
    public static class MinimalPresentationSetup
    {
        // Countdown includes its last one-second warning. Animation duration is separate.
        public const float HardTraversalSeconds = 4f;
        public const float NormalTraversalSeconds = 5f;
        public const float WarningSeconds = 1f;
        private static readonly string[] Keys = { "Prologue", "Easy", "Medium", "Hard" };

        [MenuItem("Leap of Faith/Apply Timing and Menu Only")]
        public static void Apply()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            { Debug.LogWarning("Stop Play Mode before applying the update."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUtility.DisplayDialog("Update timing and menu?",
                "Rebuild MainMenu and set Beginner/Easy/Medium to 6 seconds and Hard to 5 seconds. " +
                "All gameplay layouts are preserved.", "Apply", "Cancel")) return;
            ValidateExistingScenes();
            ApplyExistingTimers();
            PlayerSettings.productName = "Leap of Faith";
            AssetDatabase.SaveAssets();
            if (!MainMenuSceneBuilder.Build(false)) throw new InvalidOperationException("Menu creation did not complete.");
        }

        public static void ValidateExistingScenes()
        {
            foreach (string key in Keys) Visit(key, (scene, controller) => {
                SerializedObject data = new SerializedObject(controller);
                if (data.FindProperty("traversalSeconds") == null || data.FindProperty("warningSeconds") == null)
                    throw new InvalidOperationException("Timer fields missing in " + key);
            });
        }

        public static void ApplyExistingTimers()
        {
            foreach (string key in Keys) Visit(key, (scene, controller) => {
                SerializedObject data = new SerializedObject(controller);
                data.FindProperty("traversalSeconds").floatValue = key == "Hard" ? HardTraversalSeconds : NormalTraversalSeconds;
                data.FindProperty("warningSeconds").floatValue = WarningSeconds;
                if (data.ApplyModifiedPropertiesWithoutUndo())
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    if (!EditorSceneManager.SaveScene(scene)) throw new IOException("Could not save timing in " + key);
                }
            });
        }

        private static void Visit(string key, Action<Scene, RotationController> action)
        {
            string path = "Assets/HatHop/Scenes/" + key + ".unity";
            if (!File.Exists(path)) throw new FileNotFoundException("Missing " + key + ". Import your tested four-level project first.", path);
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                RotationController[] timers = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<RotationController>(true)).ToArray();
                if (timers.Length != 1) throw new InvalidOperationException("Expected exactly one rotation controller in " + key);
                action(scene, timers[0]);
            }
            finally { if (openedHere) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
