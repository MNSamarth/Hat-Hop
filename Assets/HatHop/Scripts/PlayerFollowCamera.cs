using UnityEngine;

namespace HatHop
{
    [RequireComponent(typeof(Camera))]
    public sealed class PlayerFollowCamera : MonoBehaviour
    {
        [SerializeField] private PlayerMotor2D player;
        [SerializeField] private RotationController rotation;
        [SerializeField, Min(0.1f)] private float overviewSize = 10f;
        [SerializeField, Min(1f)] private float zoom = 2f;
        [SerializeField, Min(0.01f)] private float followSmoothTime = 0.15f;
        [SerializeField, Min(0)] private float horizontalLookAhead = 0.65f;
        [SerializeField, Min(0)] private float verticalLookAhead = 0.7f;
        [SerializeField] private float verticalOffset = 0.3f;
        private Camera view;
        private Vector3 smoothVelocity;
        private Vector2 lookAhead;
        private int resetVersion;
        private float cameraZ;
        private bool initialized;
        public float OverviewSize => overviewSize;

        public void Configure(PlayerMotor2D motor, RotationController controller, float referenceSize)
        {
            player = motor;
            rotation = controller;
            overviewSize = Mathf.Max(0.1f, referenceSize);
            zoom = 2f;
        }

        private void Start()
        {
            if (player == null)
            {
                Debug.LogError("PlayerFollowCamera needs a player reference.", this);
                enabled = false;
                return;
            }
            view = GetComponent<Camera>();
            view.orthographic = true;
            cameraZ = transform.position.z;
            resetVersion = player.ResetVersion;
            initialized = true;
            SnapToPlayer();
        }

        private void LateUpdate()
        {
            if (!initialized) return;
            view.orthographicSize = overviewSize / Mathf.Max(1f, zoom);
            transform.rotation = Quaternion.identity;
            bool turning = rotation != null &&
                (rotation.CurrentPhase == RotationController.Phase.Turning ||
                 rotation.CurrentPhase == RotationController.Phase.Resuming);
            if (resetVersion != player.ResetVersion || turning)
            {
                resetVersion = player.ResetVersion;
                SnapToPlayer();
                return;
            }
            Vector2 velocity = player.Velocity;
            Vector2 desiredLook = new Vector2(
                Mathf.Clamp(velocity.x / 4.5f, -1f, 1f) * horizontalLookAhead,
                Mathf.Clamp(velocity.y / 8f, -1f, 1f) * verticalLookAhead);
            lookAhead = Vector2.Lerp(lookAhead, desiredLook, 1f - Mathf.Exp(-6f * Time.deltaTime));
            Vector3 position = player.transform.position;
            Vector3 target = new Vector3(position.x + lookAhead.x,
                position.y + verticalOffset + lookAhead.y, cameraZ);
            Vector3 next = Vector3.SmoothDamp(transform.position, target, ref smoothVelocity,
                Mathf.Max(0.01f, followSmoothTime));
            // Keep the body inside the central part of the view, including fast descents.
            float maxX = view.orthographicSize * view.aspect * 0.45f;
            float maxY = view.orthographicSize * 0.45f;
            float x = Mathf.Clamp(next.x, position.x - maxX, position.x + maxX);
            float y = Mathf.Clamp(next.y, position.y - maxY, position.y + maxY);
            if (x != next.x) smoothVelocity.x = 0;
            if (y != next.y) smoothVelocity.y = 0;
            transform.position = new Vector3(x, y, cameraZ);
        }

        private void SnapToPlayer()
        {
            view.orthographicSize = overviewSize / Mathf.Max(1f, zoom);
            Vector3 position = player.transform.position;
            transform.SetPositionAndRotation(new Vector3(position.x, position.y, cameraZ), Quaternion.identity);
            smoothVelocity = Vector3.zero;
            lookAhead = Vector2.zero;
        }
    }
}
