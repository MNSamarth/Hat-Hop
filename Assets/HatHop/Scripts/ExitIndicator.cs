using UnityEngine;

namespace HatHop
{
    public sealed class ExitIndicator : MonoBehaviour
    {
        [SerializeField] private Camera view;
        [SerializeField] private Transform player;
        [SerializeField] private Transform exit;
        [SerializeField] private LevelFlow flow;
        [SerializeField] private Texture2D arrow;
        private GUIStyle label;
        public void Configure(Camera camera, Transform motor, Transform goal, LevelFlow level, Texture2D image)
        { view = camera; player = motor; exit = goal; flow = level; arrow = image; }

        private void OnGUI()
        {
            if (view == null || player == null || exit == null || arrow == null || flow == null ||
                flow.State != LevelFlow.RunState.Playing || SceneNavigation.IsLoading) return;
            Vector3 point = view.WorldToViewportPoint(exit.position);
            Vector2 goalOnGUI = new Vector2(point.x * Screen.width, (1 - point.y) * Screen.height);
            Rect hud = new Rect(12, 12, Mathf.Min(460, Screen.width - 24), 178);
            bool clearlyVisible = point.z > 0 && point.x > 0.06f && point.x < 0.94f &&
                point.y > 0.12f && point.y < 0.92f && !hud.Contains(goalOnGUI);
            if (clearlyVisible) return;
            Vector3 from = view.WorldToScreenPoint(player.position);
            Vector3 to = view.WorldToScreenPoint(exit.position);
            Vector2 direction = new Vector2(to.x - from.x, from.y - to.y);
            if (direction.sqrMagnitude < 0.01f) return;
            Vector2 center = new Vector2(Screen.width - 46, Screen.height * 0.5f + Mathf.Sin(Time.unscaledTime * 2) * 4);
            Matrix4x4 previous = GUI.matrix;
            Color previousColor = GUI.color;
            GUI.color = new Color(0.25f, 1f, 0.52f);
            GUIUtility.RotateAroundPivot(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90, center);
            GUI.DrawTexture(new Rect(center.x - 19, center.y - 19, 38, 38), arrow);
            GUI.matrix = previous;
            GUI.color = previousColor;
            if (label == null) label = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            GUI.Label(new Rect(center.x - 33, center.y + 25, 66, 24), "EXIT", label);
        }
    }
}
