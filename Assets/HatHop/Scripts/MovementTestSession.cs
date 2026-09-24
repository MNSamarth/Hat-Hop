using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HatHop
{
    // Temporary movement-room helper. Replace with LevelFlow for the complete room.
    [RequireComponent(typeof(PlayerMotor2D))]
    public sealed class MovementTestSession : MonoBehaviour
    {
        private PlayerMotor2D motor;
        private Vector2 spawn;
        private void Start()
        {
            motor = GetComponent<PlayerMotor2D>();
            spawn = transform.position;
        }
        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            bool reset = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool reset = Input.GetKeyDown(KeyCode.R);
#else
            bool reset = false;
#endif
            if (reset || transform.position.y < -7) motor.ResetAt(spawn);
        }
    }
}
