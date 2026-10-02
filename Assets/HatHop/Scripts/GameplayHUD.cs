using UnityEngine;

namespace HatHop
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        [SerializeField] private LevelCatalog catalog;
        [SerializeField] private int levelIndex = -1;
        private GUIStyle timerStyle;
        private GUIStyle buttonStyle;
        public static Rect TimerRect => new Rect((Screen.width - 180) / 2f, 12, 180, 44);
        public void Configure(LevelFlow owner) => flow = owner;
        // Preserve the builder API; level headings are intentionally not displayed.
        public void ConfigureLevel(LevelCatalog levels, int index, string heading)
        { catalog = levels; levelIndex = index; }
        private string NextScene => catalog == null ? "" :
            levelIndex == 3 ? catalog.easyScenePath : levelIndex == 0 ? catalog.mediumScenePath :
            levelIndex == 1 ? catalog.hardScenePath : "";

        private void OnGUI()
        {
            if (flow == null || !flow.enabled || flow.Rotation == null) return;
            if (timerStyle == null)
            {
                timerStyle = new GUIStyle(GUI.skin.label) { fontSize = 21, alignment = TextAnchor.MiddleCenter };
                timerStyle.normal.textColor = Color.white;
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            }
            bool playing = flow.State == LevelFlow.RunState.Playing;
            bool won = flow.State == LevelFlow.RunState.Won;
            if (playing)
            {
                RotationController turn = flow.Rotation;
                Color previous = GUI.color;
                GUI.color = turn.CurrentPhase == RotationController.Phase.Warning ? new Color(1f, .65f, .25f) : Color.white;
                GUI.Box(TimerRect, GUIContent.none);
                GUI.Label(TimerRect, $"FLIP IN {turn.SecondsUntilTurn:0.0}s", timerStyle);
                GUI.color = previous;
            }
            bool wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && !SceneNavigation.IsLoading;
            if (GUI.Button(new Rect(Screen.width - 122, Screen.height - 56, 110, 44), "Home", buttonStyle))
                Open(SceneNavigation.MenuScenePath);
            if (playing && GUI.Button(new Rect(12, Screen.height - 56, 110, 44), "Restart", buttonStyle))
                flow.RequestRestart();
            if (won)
            {
                float width = Mathf.Min(300, Screen.width - 24);
                Rect panel = new Rect((Screen.width - width) / 2, (Screen.height - 164) / 2, width, 164);
                GUI.Box(panel, GUIContent.none);
                if (flow.Stars != null && flow.Stars.Icon != null)
                {
                    Color previous = GUI.color;
                    for (int i = 0; i < 5; i++)
                    {
                        GUI.color = i < flow.Stars.Collected ? Color.white : new Color(.25f, .25f, .25f);
                        GUI.DrawTexture(new Rect(panel.center.x - 86 + i * 36, panel.y + 28, 28, 28), flow.Stars.Icon);
                    }
                    GUI.color = previous;
                }
                if (GUI.Button(new Rect(panel.center.x - 118, panel.y + 88, 110, 44), "Restart", buttonStyle))
                    flow.RequestRestart();
                bool hasNext = !string.IsNullOrEmpty(NextScene);
                GUI.enabled = wasEnabled && !SceneNavigation.IsLoading && (!hasNext || SceneNavigation.CanLoad(NextScene));
                if (GUI.Button(new Rect(panel.center.x + 8, panel.y + 88, 110, 44), hasNext ? "Next" : "Home", buttonStyle))
                    Open(hasNext ? NextScene : SceneNavigation.MenuScenePath);
            }
            // Death still auto-respawns; there is no death message or statistics panel.
            GUI.enabled = wasEnabled;
        }

        private void Open(string path)
        {
            if (!SceneNavigation.TryLoad(path, out string error)) Debug.LogError(error, this);
        }
    }
}
