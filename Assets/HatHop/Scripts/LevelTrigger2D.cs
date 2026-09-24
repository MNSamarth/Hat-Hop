using UnityEngine;

namespace HatHop
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelTrigger2D : MonoBehaviour
    {
        public enum Kind { Hazard, Goal }
        [SerializeField] private Kind kind;
        [SerializeField] private LevelFlow flow;

        public void Configure(Kind value, LevelFlow owner)
        {
            kind = value;
            flow = owner;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other) => Report(other);
        // Stay also catches a player already overlapping a trigger after rotation resumes.
        private void OnTriggerStay2D(Collider2D other) => Report(other);

        private void Report(Collider2D other)
        {
            // Unity can send trigger callbacks to disabled behaviours.
            if (isActiveAndEnabled && flow != null && other.attachedRigidbody != null)
                flow.ReportTrigger(kind, other.attachedRigidbody);
        }
    }
}
