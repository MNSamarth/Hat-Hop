using UnityEngine;

namespace HatHop
{
    // Only Hard has this component. Both outer pads must be visited in the same run.
    [DefaultExecutionOrder(-150)]
    public sealed class HardRouteGate : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        [SerializeField] private SpriteRenderer goal;
        [SerializeField] private RoutePad[] pads;
        [SerializeField] private SpriteRenderer[] lights;
        private readonly bool[] active = new bool[2];
        public bool CanExit => active[0] && active[1];

        public void Configure(LevelFlow owner, SpriteRenderer exit, RoutePad[] switches, SpriteRenderer[] indicators)
        { flow = owner; goal = exit; pads = switches; lights = indicators; Refresh(); }

        private void Start()
        {
            if (flow == null || flow.Rotation == null || goal == null || pads == null || pads.Length != 2 ||
                lights == null || lights.Length != 2)
            { Debug.LogError("Hard circuit gate references are incomplete.", this); enabled = false; return; }
            flow.Rotation.RoomReset += ResetAttempt;
            ResetAttempt();
        }

        public void Touch(int index, Rigidbody2D body)
        {
            if (!isActiveAndEnabled || index < 0 || index >= active.Length || flow == null ||
                !flow.AcceptsPlayerContact(body) || active[index]) return;
            active[index] = true;
            Refresh();
        }

        private void ResetAttempt() { active[0] = active[1] = false; Refresh(); }
        private void Refresh()
        {
            if (goal != null) goal.color = CanExit ? new Color(.25f, 1f, .52f) : new Color(.3f, .34f, .4f);
            for (int i = 0; i < 2; i++)
            {
                Color color = i == 0 ? new Color(.2f, .85f, 1f) : new Color(.85f, .5f, 1f);
                if (!active[i]) color *= .4f;
                color.a = 1;
                if (pads != null && i < pads.Length && pads[i] != null) pads[i].SetColor(color);
                if (lights != null && i < lights.Length && lights[i] != null) lights[i].color = color;
            }
        }
        private void OnDestroy()
        { if (flow != null && flow.Rotation != null) flow.Rotation.RoomReset -= ResetAttempt; }
    }
}
