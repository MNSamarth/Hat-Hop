using UnityEngine;

namespace HatHop
{
    // Predictable, weight-operated kinematic pivot. It is deliberately not a free swinging hinge.
    [DefaultExecutionOrder(-90)]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SeesawPlatform : MonoBehaviour
    {
        [SerializeField] private PlayerMotor2D player;
        [SerializeField] private RotationController rotation;
        [SerializeField] private Transform map;
        [SerializeField] private Collider2D surface;
        [SerializeField] private float halfLength = 2.3f;
        [SerializeField] private float maximumAngle = 18;
        [SerializeField] private float edgeDwell = 0.55f;
        [SerializeField] private float crossingHold = 2f;
        private Rigidbody2D platformBody;
        private Rigidbody2D playerBody;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[24];
        private float angle, targetAngle, heldSide, dwell, holdUntil;
        public bool PlayerOnSurface { get; private set; }
        public string Hint => Mathf.Abs(angle) > 14 ? "Tip raised: cross to the high end and jump!" : "Seesaw: hold at an end to raise the opposite tip.";

        public void Configure(PlayerMotor2D motor, RotationController turn, Transform mapRoot, Collider2D beam)
        { player = motor; rotation = turn; map = mapRoot; surface = beam; }

        private void Awake()
        {
            platformBody = GetComponent<Rigidbody2D>();
            if (player != null) playerBody = player.GetComponent<Rigidbody2D>();
        }

        public void ResetTilt()
        {
            angle = targetAngle = heldSide = dwell = holdUntil = 0;
            PlayerOnSurface = false;
            transform.localRotation = Quaternion.identity;
            if (platformBody != null)
            {
                platformBody.angularVelocity = 0;
                platformBody.rotation = map == null ? 0 : map.eulerAngles.z;
            }
        }

        private void FixedUpdate()
        {
            PlayerOnSurface = false;
            if (player == null || playerBody == null || rotation == null || map == null || surface == null) return;
            if (!rotation.IsInitialized || rotation.IsHalted || player.IsSuspended ||
                rotation.CurrentPhase == RotationController.Phase.Turning || rotation.CurrentPhase == RotationController.Phase.Resuming)
            {
                platformBody.angularVelocity = 0;
                // Extend the hold through a frozen world turn; don't consume the crossing window.
                holdUntil += Time.fixedDeltaTime;
                return;
            }
            int count = playerBody.GetContacts(contacts);
            if (player.Velocity.y <= 0.15f)
                for (int i = 0; i < count; i++)
                    if ((contacts[i].collider == surface || contacts[i].otherCollider == surface) && contacts[i].normal.y > 0.65f)
                    { PlayerOnSurface = true; break; }

            float localX = transform.InverseTransformPoint(player.transform.position).x;
            float side = PlayerOnSurface && Mathf.Abs(localX) > halfLength * 0.60f ? Mathf.Sign(localX) : 0;
            float desired = -side * Mathf.Sign(map.up.y) * maximumAngle;
            bool heldAgainstReverse = Time.time < holdUntil && targetAngle != 0 && Mathf.Sign(desired) != Mathf.Sign(targetAngle);
            if (side != 0 && !heldAgainstReverse)
            {
                dwell = side == heldSide ? dwell + Time.fixedDeltaTime : 0;
                heldSide = side;
                if (dwell >= edgeDwell) { targetAngle = desired; holdUntil = Time.time + crossingHold; }
            }
            else { dwell = 0; heldSide = 0; }
            if (!PlayerOnSurface && Time.time >= holdUntil) targetAngle = 0;
            angle = Mathf.MoveTowards(angle, targetAngle, (targetAngle == 0 ? 8f : 24f) * Time.fixedDeltaTime);
            platformBody.MoveRotation(map.eulerAngles.z + angle);
        }
    }
}
