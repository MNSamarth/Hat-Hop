using UnityEngine;

namespace HatHop
{
    // Retained for compatibility with existing Prologue scenes. The beginner room
    // now runs freely: no hidden action checklist, story text or locked exit.
    [DefaultExecutionOrder(-50)]
    public sealed class TutorialGuide : MonoBehaviour
    {
        [SerializeField] private LevelFlow flow;
        public bool CanExit => true;
        public void Configure(LevelFlow owner, PlayerMotor2D motor, Transform root) => flow = owner;
        private void Start()
        {
            if (flow == null || flow.Rotation == null) { enabled = false; return; }
            flow.Rotation.RoomReset += ResumeSchedule;
            ResumeSchedule();
        }
        private void ResumeSchedule() => flow.Rotation.SetSchedulePaused(false);
        private void OnDestroy()
        {
            if (flow != null && flow.Rotation != null) flow.Rotation.RoomReset -= ResumeSchedule;
        }
    }
}
