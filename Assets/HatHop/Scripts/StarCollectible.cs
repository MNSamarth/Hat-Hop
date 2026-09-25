using UnityEngine;

namespace HatHop
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class StarCollectible : MonoBehaviour
    {
        [SerializeField] private LevelStars owner;
        public bool Collected { get; private set; }
        public void Configure(LevelStars run) { owner = run; GetComponent<Collider2D>().isTrigger = true; }
        public void SetCollected(bool value)
        {
            Collected = value;
            SpriteRenderer image = GetComponent<SpriteRenderer>();
            Collider2D trigger = GetComponent<Collider2D>();
            if (image != null) image.enabled = !value;
            if (trigger != null) trigger.enabled = !value;
        }
        private void OnTriggerEnter2D(Collider2D other) => Report(other);
        private void OnTriggerStay2D(Collider2D other) => Report(other);
        private void Report(Collider2D other)
        {
            if (isActiveAndEnabled && !Collected && owner != null && other.attachedRigidbody != null)
                owner.Report(this, other.attachedRigidbody);
        }
    }
}
