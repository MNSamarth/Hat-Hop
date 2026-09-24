using UnityEngine;

namespace HatHop
{
    // Animate a child sprite only. Never move the Rigidbody2D or collision shape.
    public sealed class PlayerHopVisual : MonoBehaviour
    {
        [SerializeField] private PlayerMotor2D motor;
        [SerializeField] private Transform visual;
        [SerializeField, Min(0)] private float hopHeight = 0.12f;
        [SerializeField, Min(0.05f)] private float hopPeriod = 0.32f;
        private Vector3 restPosition;
        private float phase;
        private int resetVersion;
        private bool initialized;

        public void Configure(PlayerMotor2D player, Transform sprite)
        {
            motor = player;
            visual = sprite;
        }

        private void Start()
        {
            if (motor == null || visual == null || visual == transform)
            {
                Debug.LogError("PlayerHopVisual needs the motor and a separate child Visual transform.", this);
                enabled = false;
                return;
            }
            restPosition = visual.localPosition;
            resetVersion = motor.ResetVersion;
            initialized = true;
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            if (resetVersion != motor.ResetVersion)
            {
                resetVersion = motor.ResetVersion;
                phase = 0;
            }
            if (motor.IsSuspended || !motor.IsGrounded)
            {
                phase = 0;
                visual.localPosition = restPosition;
                return;
            }
            phase = Mathf.Repeat(phase + Time.deltaTime / Mathf.Max(0.05f, hopPeriod), 1f);
            float height = 4f * phase * (1f - phase) * hopHeight;
            // Height is in world units even though the player's root is scaled.
            float parentScale = visual.parent == null ? 1f : Mathf.Abs(visual.parent.lossyScale.y);
            visual.localPosition = restPosition + Vector3.up * (height / Mathf.Max(0.001f, parentScale));
        }

        private void OnDisable()
        {
            if (initialized && visual != null) visual.localPosition = restPosition;
        }
    }
}
