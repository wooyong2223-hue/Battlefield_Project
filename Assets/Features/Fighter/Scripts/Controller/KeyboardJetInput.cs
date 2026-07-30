using UnityEngine;
using Battlefield.Input;

namespace Battlefield.Fighter.Controller
{
    public class KeyboardJetInput : MonoBehaviour, IJetInput
    {
        private const float MouseDeltaScale = 0.1f;

        private InputSystem_Actions _inputActions;

        // Controller
        public float Throttle { get; private set; }
        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Yaw { get; private set; }

        // Camera
        public bool ChangeCamera { get; private set; }
        public bool RearView { get; private set; }

        // Weapon
        public bool FireWeapon { get; private set; }


        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Player.Disable();
            ResetInput();
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        private void Update()
        {
            Vector2 move = _inputActions.Player.Move.ReadValue<Vector2>();
            Vector2 look = _inputActions.Player.Look.ReadValue<Vector2>();

            Throttle = move.y;
            Yaw = move.x;
            Pitch = -look.y * MouseDeltaScale;
            Roll = -look.x * MouseDeltaScale;

            ChangeCamera = _inputActions.Player.ChangeCamera.WasPressedThisFrame();
            RearView = _inputActions.Player.RearView.IsPressed();
            FireWeapon = _inputActions.Player.Attack.IsPressed();
        }

        private void ResetInput()
        {
            Throttle = 0f;
            Pitch = 0f;
            Roll = 0f;
            Yaw = 0f;
            ChangeCamera = false;
            RearView = false;
            FireWeapon = false;
        }
    }
}
