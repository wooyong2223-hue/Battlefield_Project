using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlefield.Fighter.Controller
{
    public class KeyboardJetInput : MonoBehaviour, IJetInput
    {
        private const float MouseDeltaScale = 0.1f;

        [SerializeField] private InputActionAsset _inputActions;

        private InputActionAsset _runtimeInputActions;
        private InputActionMap _playerActions;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _attackAction;
        private InputAction _changeCameraAction;
        private InputAction _rearViewAction;

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
            _runtimeInputActions = Instantiate(_inputActions);
            _playerActions =
                _runtimeInputActions.FindActionMap("Fighter", true);

            _moveAction = _playerActions.FindAction("Move", true);
            _lookAction = _playerActions.FindAction("Look", true);
            _attackAction = _playerActions.FindAction("Attack", true);
            _changeCameraAction =
                _playerActions.FindAction("ChangeCamera", true);
            _rearViewAction =
                _playerActions.FindAction("RearView", true);
        }

        private void OnEnable()
        {
            _playerActions.Enable();
        }

        private void OnDisable()
        {
            _playerActions.Disable();
            ResetInput();
        }

        private void OnDestroy()
        {
            Destroy(_runtimeInputActions);
        }

        private void Update()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            Vector2 look = _lookAction.ReadValue<Vector2>();

            Throttle = move.y;
            Yaw = move.x;
            Pitch = -look.y * MouseDeltaScale;
            Roll = -look.x * MouseDeltaScale;

            ChangeCamera = _changeCameraAction.WasPressedThisFrame();
            RearView = _rearViewAction.IsPressed();
            FireWeapon = _attackAction.IsPressed();
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
