using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HatHop
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private float horizontalSpeed = 4.5f;
        [SerializeField, Min(0)] private float smallHopSpeed = 4f;
        [SerializeField, Min(0)] private float bigHopSpeed = 8f;
        [SerializeField, Min(0)] private float jumpBufferSeconds = 0.2f;
        [SerializeField, Range(0, 1)] private float minimumGroundNormal = 0.65f;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
        private Rigidbody2D body;
        private float steer;
        private float jumpExpiresAt = float.NegativeInfinity;
        private bool suspended;

        private void Awake() => body = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (suspended) return;
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            steer = keyboard == null ? 0 :
                (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            bool bigPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            steer = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            bool bigPressed = Input.GetKeyDown(KeyCode.Space);
#else
            steer = 0;
            bool bigPressed = false;
#endif
            if (bigPressed) jumpExpiresAt = Time.time + jumpBufferSeconds;
        }

        private void FixedUpdate()
        {
            if (suspended || !body.simulated) return;
            Vector2 velocity = ReadVelocity();
            velocity.x = steer * horizontalSpeed;
            // Ascending bodies may retain contacts from the previous simulation step.
            if (velocity.y <= 0.05f && HasSupport())
            {
                velocity.y = Time.time <= jumpExpiresAt ? bigHopSpeed : smallHopSpeed;
                jumpExpiresAt = float.NegativeInfinity;
            }
            WriteVelocity(velocity);
        }

        private bool HasSupport()
        {
            int count = body.GetContacts(contacts);
            for (int i = 0; i < count; i++)
                if (contacts[i].normal.y >= minimumGroundNormal) return true;
            return false;
        }

        public void SetSuspended(bool value)
        {
            suspended = value;
            steer = 0;
            jumpExpiresAt = float.NegativeInfinity;
            WriteVelocity(Vector2.zero);
        }

        public void ResetAt(Vector2 position)
        {
            SetSuspended(false);
            body.simulated = true;
            body.position = position;
            body.rotation = 0;
            body.angularVelocity = 0;
            body.WakeUp();
        }

        private Vector2 ReadVelocity()
        {
#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }

        private void WriteVelocity(Vector2 value)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = value;
#else
            body.velocity = value;
#endif
        }
    }
}
