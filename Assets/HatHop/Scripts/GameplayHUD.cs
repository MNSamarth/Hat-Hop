using UnityEngine;

namespace HatHop
{
    public sealed class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        private GUIStyle label;
        private GUIStyle title;
        public void Configure(LevelFlow owner) => flow = owner;

        private void OnGUI()
        {
            if (flow == null || !flow.enabled) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
                label.normal.textColor = Color.white;
                title = new GUIStyle(label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            }
            float width = Mathf.Min(460, Screen.width - 24);
            string status = "";
            RotationController rotation = flow.Rotation;
            if (flow.State == LevelFlow.RunState.Playing)
            {
                if (rotation.CurrentPhase == RotationController.Phase.Warning)
                    status = $"FLIP IN {rotation.WarningRemaining:0.0}s - find a safe landing!";
                else if (rotation.CurrentPhase == RotationController.Phase.Turning)
                    status = "FLIPPING";
                else status = $"Next warning in {rotation.UntilWarning:0.0}s";
            }
            GUI.Box(new Rect(12, 12, width, 150), GUIContent.none);
            GUI.Label(new Rect(24, 20, width - 24, 136),
                "HAT HOP\nA / D steer | Space big hop | R restart\n" +
                "Reach GREEN. Avoid RED.\n" + status +
                $"\nDeaths: {flow.Deaths}    Flips: {rotation.CompletedTurns}", label);

            if (flow.State == LevelFlow.RunState.Playing) return;
            float panelWidth = Mathf.Min(420, Screen.width - 24);
            Rect panel = new Rect((Screen.width - panelWidth) / 2, (Screen.height - 190) / 2, panelWidth, 190);
            GUI.Box(panel, GUIContent.none);
            string heading = flow.State == LevelFlow.RunState.Won ? "YOU ESCAPED!" : "MISSED THE LANDING";
            GUI.Label(new Rect(panel.x + 12, panel.y + 16, panel.width - 24, 46), heading, title);
            string detail = flow.State == LevelFlow.RunState.Won
                ? $"Finished in {flow.RunSeconds:0.0}s. Press R to play again."
                : $"Restarting in {flow.RespawnRemaining:0.0}s...";
            GUI.Label(new Rect(panel.x + 20, panel.y + 76, panel.width - 40, 52), detail, label);
            if (GUI.Button(new Rect(panel.center.x - 85, panel.y + 136, 170, 34), "Restart"))
                flow.RequestRestart();
        }
    }
}
