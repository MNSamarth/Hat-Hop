using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatHop
{
    public static class SceneNavigation
    {
        public const string MenuScenePath = "Assets/HatHop/Scenes/MainMenu.unity";
        public static bool IsLoading { get; private set; }

        // Also runs when Enter Play Mode has domain reload disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() => IsLoading = false;

        public static bool CanLoad(string path) => !string.IsNullOrEmpty(path) &&
            SceneUtility.GetBuildIndexByScenePath(path) >= 0;

        public static bool TryLoad(string path, out string error)
        {
            error = "";
            if (IsLoading) return false;
            if (!CanLoad(path))
            {
                error = "This destination is unavailable. Please try another level.";
                Debug.LogWarning("Hat Hop: destination missing from enabled build scenes: " + path);
                return false;
            }
            IsLoading = true;
            try
            {
                Time.timeScale = 1f;
                AsyncOperation load = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
                if (load == null) throw new InvalidOperationException("No scene load operation returned.");
                load.completed += _ => IsLoading = false;
                return true;
            }
            catch (Exception exception)
            {
                IsLoading = false;
                error = "Couldn't open the level. Please try again.";
                Debug.LogException(exception);
                return false;
            }
        }
    }
}
