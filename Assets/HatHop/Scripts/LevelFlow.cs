using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HatHop
{
    // Resolve all outcomes from the previous physics step before rotation or movement.
    [DefaultExecutionOrder(-200)]
    public sealed class LevelFlow : MonoBehaviour
    {
        public enum RunState { Playing, Dead, Won }
        [SerializeField] private RotationController rotation;
        [SerializeField] private PlayerMotor2D player;
        [SerializeField] private Transform mapRoot;
        [SerializeField, Min(0.1f)] private float respawnDelay = 0.6f;
        [SerializeField, Min(1f)] private float outOfBoundsDistance = 25f;
        private Rigidbody2D body;
        private LevelStars stars;
        private SeesawPlatform[] seesaws;
        public LevelStars Stars => stars;
        public string PlatformHint
        {
            get
            {
                if (seesaws != null) foreach (SeesawPlatform seesaw in seesaws)
                    if (seesaw != null && seesaw.PlayerOnSurface) return seesaw.Hint;
                return "";
            }
        }
        private bool hazardPending;
        private bool goalPending;
        private bool restartPending;
        private float deathElapsed;
        public RunState State { get; private set; } = RunState.Playing;
        public int Deaths { get; private set; }
        public float RunSeconds { get; private set; }
        public float RespawnRemaining => Mathf.Max(0, respawnDelay - deathElapsed);
        public RotationController Rotation => rotation;

        public void Configure(RotationController controller, PlayerMotor2D motor, Transform map)
        {
            rotation = controller;
            player = motor;
            mapRoot = map;
        }

        private void Start()
        {
            if (rotation == null || player == null || mapRoot == null)
            {
                Debug.LogError("LevelFlow needs RotationController, Player and MapRoot.", this);
                enabled = false;
                return;
            }
            body = player.GetComponent<Rigidbody2D>();
            stars = GetComponent<LevelStars>();
            seesaws = mapRoot.GetComponentsInChildren<SeesawPlatform>(true);
            rotation.RoomReset += ResetLevelObjects;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                RequestRestart();
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R)) RequestRestart();
#endif
        }

        public void RequestRestart() => restartPending = true;

        public bool AcceptsPlayerContact(Rigidbody2D enteringBody) => isActiveAndEnabled && body != null &&
            enteringBody == body && State == RunState.Playing && rotation != null &&
            rotation.IsInitialized && !rotation.IsHalted && !restartPending &&
            rotation.CurrentPhase != RotationController.Phase.Turning;

        public void ReportTrigger(LevelTrigger2D.Kind kind, Rigidbody2D enteringBody)
        {
            if (!AcceptsPlayerContact(enteringBody)) return;
            // Death, stars and exit are resolved once at the next physics boundary.
            if (kind == LevelTrigger2D.Kind.Hazard) hazardPending = true;
            else goalPending = true;
        }

        private void ResetLevelObjects()
        {
            if (stars != null) stars.ResetAttempt();
            if (seesaws != null) foreach (SeesawPlatform seesaw in seesaws) if (seesaw != null) seesaw.ResetTilt();
            hazardPending = goalPending = false;
        }

        private void OnDestroy()
        {
            if (rotation != null) rotation.RoomReset -= ResetLevelObjects;
        }

        private void FixedUpdate()
        {
            if (!rotation.IsInitialized) return;
            if (restartPending)
            {
                Restart(true);
                return;
            }
            if (State == RunState.Dead)
            {
                deathElapsed += Time.fixedDeltaTime;
                if (deathElapsed >= respawnDelay) Restart(false);
                return;
            }
            if (State == RunState.Won) return;

            RunSeconds += Time.fixedDeltaTime;
            if (rotation.CurrentPhase != RotationController.Phase.Turning &&
                Vector2.Distance(body.position, mapRoot.position) > outOfBoundsDistance)
                hazardPending = true;

            if (hazardPending)
            {
                if (stars != null) stars.DiscardPending();
                State = RunState.Dead;
                Deaths++;
                deathElapsed = 0;
                rotation.HaltForOutcome();
            }
            else
            {
                if (stars != null) stars.ResolvePending();
                if (goalPending)
                {
                    State = RunState.Won;
                    if (stars != null) stars.SaveCompletedRun();
                    rotation.HaltForOutcome();
                }
            }
            hazardPending = false;
            goalPending = false;
        }

        private void Restart(bool clearDeaths)
        {
            restartPending = false;
            hazardPending = false;
            goalPending = false;
            deathElapsed = 0;
            RunSeconds = 0;
            if (clearDeaths) Deaths = 0;
            State = RunState.Playing;
            rotation.RestartFromLevel();
        }
    }
}
