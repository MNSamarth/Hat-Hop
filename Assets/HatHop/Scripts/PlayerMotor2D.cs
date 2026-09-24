using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HatHop
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private float horizontalSpeed = 4.5f;
        [FormerlySerializedAs("bigHopSpeed")]
        [SerializeField, Min(0)] private float jumpSpeed = 8f;
        [SerializeField, Min(0)] private float jumpBufferSeconds = 0.12f;
        [SerializeField, Min(0)] private float coyoteSeconds = 0.08f;
        [SerializeField, Range(0, 1)] private float minimumGroundNormal = 0.65f;
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[16];
        private Rigidbody2D body;
        private float steer;
        private float jumpExpiresAt = float.NegativeInfinity;
        private float lastSupportedAt = float.NegativeInfinity;
        private bool suspended;
        public bool IsGrounded { get; private set; }
        public bool IsSuspended => suspended;
        public int ResetVersion { get; private set; }
        public Vector2 Velocity => body == null ? Vector2.zero : ReadVelocity();

        private void Awake() => body = GetComponent<Rigidbody2D>();

        private void Update()
        {
            if (suspended) return;
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            steer = keyboard == null ? 0 :
                (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0);
            bool jumpPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            steer = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space);
#else
            steer = 0;
            bool jumpPressed = false;
#endif
            // Only a fresh press requests a jump. Holding Space never repeats it.
            if (jumpPressed) jumpExpiresAt = Time.time + jumpBufferSeconds;
        }

        private void FixedUpdate()
        {
            if (suspended || !body.simulated)
            {
                IsGrounded = false;
                return;
            }
            Vector2 velocity = ReadVelocity();
            velocity.x = steer * horizontalSpeed;
            // Reject stale landing contacts on the physics step after takeoff.
            IsGrounded = velocity.y <= 0.05f && HasSupport();
            if (IsGrounded) lastSupportedAt = Time.time;
            bool canJump = IsGrounded || Time.time - lastSupportedAt <= coyoteSeconds;
            if (Time.time <= jumpExpiresAt && canJump)
            {
                velocity.y = jumpSpeed;
                jumpExpiresAt = float.NegativeInfinity;
                lastSupportedAt = float.NegativeInfinity;
                IsGrounded = false;
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
            ClearInputAndSupport();
            WriteVelocity(Vector2.zero);
        }

        public void ResetAt(Vector2 position)
        {
            SetSuspended(false);
            body.simulated = true;
            body.position = position;
            body.rotation = 0;
            body.angularVelocity = 0;
            transform.SetPositionAndRotation(new Vector3(position.x, position.y, transform.position.z),
                Quaternion.identity);
            body.WakeUp();
            ResetVersion++;
        }

        private void ClearInputAndSupport()
        {
            steer = 0;
            jumpExpiresAt = float.NegativeInfinity;
            lastSupportedAt = float.NegativeInfinity;
            IsGrounded = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) ClearInputAndSupport();
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
