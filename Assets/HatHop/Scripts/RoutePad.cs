using UnityEngine;

namespace HatHop
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoutePad : MonoBehaviour
    {
        [SerializeField] private HardRouteGate gate;
        [SerializeField] private int index;
        [SerializeField] private SpriteRenderer[] markers;
        public void Configure(HardRouteGate owner, int number, SpriteRenderer[] visuals)
        { gate = owner; index = number; markers = visuals; GetComponent<Collider2D>().isTrigger = true; }
        public void SetColor(Color color)
        { if (markers != null) foreach (SpriteRenderer marker in markers) if (marker != null) marker.color = color; }
        private void OnTriggerEnter2D(Collider2D other) => Report(other);
        private void OnTriggerStay2D(Collider2D other) => Report(other);
        private void Report(Collider2D other)
        { if (isActiveAndEnabled && gate != null && other.attachedRigidbody != null) gate.Touch(index, other.attachedRigidbody); }
    }
}
