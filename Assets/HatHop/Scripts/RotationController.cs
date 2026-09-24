using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HatHop
{
    // Run before the motor: suspension takes effect before it changes velocity.
    [DefaultExecutionOrder(-100)]
    public sealed class RotationController : MonoBehaviour
    {
        public enum Phase { Traversal, Warning, Turning, Resuming }

        [SerializeField] private Transform mapRoot;
        [SerializeField] private PlayerMotor2D player;
        [SerializeField] private bool managedByLevel;
        [SerializeField, Min(0.1f)] private float traversalSeconds = 6f;
        [SerializeField, Min(0.1f)] private float warningSeconds = 2f;
        [SerializeField, Min(0.1f)] private float turnSeconds = 0.6f;
        [SerializeField, Min(1f)] private float resetDistance = 20f;

        private Rigidbody2D body;
        private Collider2D playerCollider;
        private Collider2D[] mapColliders;
        private RigidbodyInterpolation2D normalInterpolation;
        private Vector3 initialMapPosition;
        private Quaternion initialMapRotation;
        private Vector2 spawn;
        private Vector2 turnStartPosition;
        private Quaternion turnStartRotation;
        private float elapsed;
        private bool flipped;
        private bool resetRequested;
        private bool initialized;
        private bool halted;
        private bool deferResumeOneStep;
        public bool IsInitialized => initialized;
        public bool IsHalted => halted;
        public Phase CurrentPhase { get; private set; }
        public int CompletedTurns { get; private set; }
        public float WarningRemaining => Mathf.Max(0f, warningSeconds - elapsed);
        public float UntilWarning => Mathf.Max(0f, traversalSeconds - elapsed);

        // Called only by the Editor scene builder; Inspector references are serialized.
        public void Configure(Transform map, PlayerMotor2D motor, bool externalLifecycle = false)
        {
            mapRoot = map;
            player = motor;
            managedByLevel = externalLifecycle;
        }

        private void Start()
        {
            if (mapRoot == null || player == null)
            {
                Debug.LogError("RotationController needs MapRoot and Player references.", this);
                enabled = false;
                return;
            }
            body = player.GetComponent<Rigidbody2D>();
            playerCollider = player.GetComponent<Collider2D>();
            mapColliders = mapRoot.GetComponentsInChildren<Collider2D>();
            normalInterpolation = body.interpolation;
            initialMapPosition = mapRoot.position;
            initialMapRotation = mapRoot.rotation;
            spawn = body.position;
            initialized = true;
            ResetRoom();
        }

        private void Update()
        {
            if (managedByLevel) return;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                resetRequested = true;
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R)) resetRequested = true;
#endif
        }

        private void FixedUpdate()
        {
            if (!initialized) return;
            if (resetRequested)
            {
                ResetRoom();
                return;
            }
            // A reset requested by LevelFlow happens before our FixedUpdate.
            // Leave one simulation step for the teleported body to refresh contacts.
            if (deferResumeOneStep)
            {
                deferResumeOneStep = false;
                return;
            }
            if (halted) return;
            if (!managedByLevel && CurrentPhase != Phase.Turning &&
                Vector2.Distance(body.position, initialMapPosition) > resetDistance)
            {
                ResetRoom();
                return;
            }

            // Simulation already ran once with the new pose and the motor suspended.
            // This replaces stale contacts before the motor is allowed to launch again.
            if (CurrentPhase == Phase.Resuming)
            {
                body.interpolation = normalInterpolation;
                player.SetSuspended(false);
                CurrentPhase = Phase.Traversal;
                elapsed = 0;
                return;
            }

            elapsed += Time.fixedDeltaTime;
            switch (CurrentPhase)
            {
                case Phase.Traversal:
                    if (elapsed >= traversalSeconds)
                    {
                        CurrentPhase = Phase.Warning;
                        elapsed = 0;
                    }
                    break;
                case Phase.Warning:
                    if (elapsed >= warningSeconds) BeginTurn();
                    break;
                case Phase.Turning:
                    AnimateTurn();
                    break;
            }
        }

        private void BeginTurn()
        {
            player.SetSuspended(true);
            body.angularVelocity = 0;
            body.interpolation = RigidbodyInterpolation2D.None;
            body.simulated = false;
            turnStartPosition = body.position;
            turnStartRotation = mapRoot.rotation;
            CurrentPhase = Phase.Turning;
            elapsed = 0;
        }

        private void AnimateTurn()
        {
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.1f, turnSeconds));
            float eased = t * t * (3f - 2f * t);
            Quaternion delta = Quaternion.Euler(0, 0, 180f * eased);
            Vector3 pivot = initialMapPosition;
            Vector3 offset = new Vector3(turnStartPosition.x, turnStartPosition.y, pivot.z) - pivot;
            mapRoot.rotation = delta * turnStartRotation;
            SetPlayerPose(pivot + delta * offset);
            if (t < 1f) return;

            flipped = !flipped;
            // Snap both endpoints to exact half-turn results; avoid accumulated drift.
            mapRoot.rotation = Quaternion.Euler(0, 0, flipped ? 180f : 0f) * initialMapRotation;
            SetPlayerPose(new Vector2(2f * pivot.x - turnStartPosition.x,
                2f * pivot.y - turnStartPosition.y));
            body.simulated = true;
            Physics2D.SyncTransforms();
            if (!HasValidClearance())
            {
                Debug.LogWarning("Rotation endpoint overlap detected; room reset. Check platform clearance.", this);
                ResetRoom();
                return;
            }
            CompletedTurns++;
            CurrentPhase = Phase.Resuming;
            elapsed = 0;
        }

        private void SetPlayerPose(Vector2 position)
        {
            body.position = position;
            body.rotation = 0;
            player.transform.SetPositionAndRotation(
                new Vector3(position.x, position.y, player.transform.position.z), Quaternion.identity);
        }

        private bool HasValidClearance()
        {
            foreach (Collider2D solid in mapColliders)
            {
                if (solid == null || !solid.enabled || solid.isTrigger || !solid.gameObject.activeInHierarchy)
                    continue;
                ColliderDistance2D distance = playerCollider.Distance(solid);
                if (!distance.isValid || (distance.isOverlapped && distance.distance < -0.03f))
                    return false;
            }
            return true;
        }

        // LevelFlow calls these at a physics boundary. Keep the component enabled:
        // disabling it invokes OnDisable's cleanup and would unfreeze a finished run.
        public void HaltForOutcome()
        {
            if (!initialized) return;
            halted = true;
            player.SetSuspended(true);
            body.angularVelocity = 0;
            body.simulated = false;
        }

        public void RestartFromLevel()
        {
            if (!initialized) return;
            ResetRoom();
            deferResumeOneStep = true;
        }

        private void ResetRoom()
        {
            resetRequested = false;
            halted = false;
            deferResumeOneStep = false;
            player.SetSuspended(true);
            body.simulated = false;
            body.interpolation = RigidbodyInterpolation2D.None;
            mapRoot.SetPositionAndRotation(initialMapPosition, initialMapRotation);
            SetPlayerPose(spawn);
            player.ResetAt(spawn);
            player.SetSuspended(true);
            Physics2D.SyncTransforms();
            flipped = false;
            CompletedTurns = 0;
            elapsed = 0;
            CurrentPhase = Phase.Resuming;
        }

        private void OnDisable()
        {
            if (!initialized || body == null || player == null || mapRoot == null) return;
            ResetRoom();
            body.interpolation = normalInterpolation;
            player.SetSuspended(false);
        }
    }
}
