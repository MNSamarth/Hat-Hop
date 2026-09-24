using UnityEngine;

namespace HatHop
{
    // Temporary test overlay; stays upright because it is drawn in screen coordinates.
    public sealed class RotationTestHUD : MonoBehaviour
    {
        [SerializeField] private RotationController controller;
        private GUIStyle textStyle;
        public void Configure(RotationController value) => controller = value;

        private void OnGUI()
        {
            if (controller == null || !controller.enabled) return;
            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
                textStyle.normal.textColor = Color.white;
            }
            string status;
            switch (controller.CurrentPhase)
            {
                case RotationController.Phase.Warning:
                    status = $"FLIP IN {controller.WarningRemaining:0.0}s — prepare your landing!";
                    break;
                case RotationController.Phase.Turning:
                    status = "FLIPPING";
                    break;
                case RotationController.Phase.Resuming:
                    status = "Resuming";
                    break;
                default:
                    status = $"Next warning in {controller.UntilWarning:0.0}s";
                    break;
            }
            float width = Mathf.Min(440, Screen.width - 24);
            GUI.Box(new Rect(12, 12, width, 142), GUIContent.none);
            GUI.Label(new Rect(24, 20, width - 24, 130),
                "HAT HOP — ROTATION TEST\nA / D move | Space jumps | R resets\n" +
                status + $"\nCompleted flips: {controller.CompletedTurns}", textStyle);
        }
    }
}
