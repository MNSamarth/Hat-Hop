using UnityEngine;

namespace HatHop
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        [SerializeField] private LevelCatalog catalog;
        [SerializeField] private int levelIndex = -1;
        [SerializeField] private string levelTitle = "HAT HOP";
        private GUIStyle label;
        private GUIStyle title;
        private string navigationError = "";
        public void Configure(LevelFlow owner) => flow = owner;

        public void ConfigureLevel(LevelCatalog levels, int index, string heading)
        {
            catalog = levels;
            levelIndex = index;
            levelTitle = heading;
        }

        private string NextScene => catalog == null ? "" :
            levelIndex == 0 ? catalog.mediumScenePath : levelIndex == 1 ? catalog.hardScenePath : "";

        private void OnGUI()
        {
            if (flow == null || !flow.enabled) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
                label.normal.textColor = Color.white;
                title = new GUIStyle(label) { fontSize = 25, alignment = TextAnchor.MiddleCenter };
            }
            float width = Mathf.Min(460, Screen.width - 24);
            string status = "";
            RotationController rotation = flow.Rotation;
            if (flow.State == LevelFlow.RunState.Playing)
            {
                if (rotation.CurrentPhase == RotationController.Phase.Warning)
                    status = $"FLIP IN {rotation.WarningRemaining:0.0}s - prepare to land!";
                else if (rotation.CurrentPhase == RotationController.Phase.Turning)
                    status = "FLIPPING";
                else status = $"Next warning in {rotation.UntilWarning:0.0}s";
            }
            GUI.Box(new Rect(12, 12, width, 150), GUIContent.none);
            GUI.Label(new Rect(24, 20, width - 24, 136),
                (string.IsNullOrEmpty(levelTitle) ? "HAT HOP" : levelTitle) +
                "\nA / D move | Space jump | R restart\nReach GREEN. Avoid RED.\n" + status +
                $"\nDeaths: {flow.Deaths}    Flips: {rotation.CompletedTurns}", label);

            bool oldEnabled = GUI.enabled;
            GUI.enabled = oldEnabled && !SceneNavigation.IsLoading;
            if (GUI.Button(new Rect(Screen.width - 162, Screen.height - 48, 150, 36),
                SceneNavigation.IsLoading ? "Loading..." : "Main Menu"))
                Open(SceneNavigation.MenuScenePath);
            if (flow.State == LevelFlow.RunState.Playing &&
                GUI.Button(new Rect(12, Screen.height - 48, 120, 36), "Restart"))
                flow.RequestRestart();
            GUI.enabled = oldEnabled;
            if (!string.IsNullOrEmpty(navigationError))
                GUI.Label(new Rect(12, Screen.height - 105, Mathf.Min(460, Screen.width - 24), 48), navigationError, label);
            if (flow.State == LevelFlow.RunState.Playing) return;

            bool won = flow.State == LevelFlow.RunState.Won;
            float panelWidth = Mathf.Min(440, Screen.width - 24);
            Rect panel = new Rect((Screen.width - panelWidth) / 2, (Screen.height - 245) / 2, panelWidth, 245);
            GUI.Box(panel, GUIContent.none);
            string heading = won ? (levelIndex == 2 ? "FINAL LEVEL COMPLETE!" : "YOU ESCAPED!") : "MISSED THE LANDING";
            GUI.Label(new Rect(panel.x + 12, panel.y + 16, panel.width - 24, 60), heading, title);
            string detail = won
                ? $"Finished in {flow.RunSeconds:0.0}s. Deaths: {flow.Deaths}."
                : $"Restarting in {flow.RespawnRemaining:0.0}s...";
            GUI.Label(new Rect(panel.x + 20, panel.y + 80, panel.width - 40, 48), detail, label);
            GUI.enabled = oldEnabled && !SceneNavigation.IsLoading;
            float buttonWidth = (panel.width - 52) / 2;
            if (GUI.Button(new Rect(panel.x + 20, panel.y + 137, buttonWidth, 36), won ? "Retry" : "Restart"))
                flow.RequestRestart();
            if (won && levelIndex >= 0 && levelIndex < 2)
            {
                GUI.enabled = oldEnabled && !SceneNavigation.IsLoading && SceneNavigation.CanLoad(NextScene);
                if (GUI.Button(new Rect(panel.x + 32 + buttonWidth, panel.y + 137, buttonWidth, 36), "Next Level"))
                    Open(NextScene);
            }
            GUI.enabled = oldEnabled && !SceneNavigation.IsLoading;
            if (GUI.Button(new Rect(panel.x + 20, panel.y + 187, panel.width - 40, 36), "Main Menu"))
                Open(SceneNavigation.MenuScenePath);
            GUI.enabled = oldEnabled;
        }

        private void Open(string path) => SceneNavigation.TryLoad(path, out navigationError);
    }
}
