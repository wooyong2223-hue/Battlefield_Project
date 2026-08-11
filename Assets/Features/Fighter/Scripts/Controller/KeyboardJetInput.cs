using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlefield.Features.Fighter
{
    public class KeyboardJetInput : MonoBehaviour, IJetInput
    {
        private const float MouseDeltaScale = 0.1f;

        [SerializeField] private InputActionAsset _inputActions;

        private InputActionAsset _runtimeInputActions;
        private InputActionMap _playerActions;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _pitchAction;
        private InputAction _rollAction;
        private InputAction _attackAction;
        private InputAction _changeCameraAction;
        private InputAction _rearViewAction;
        private InputAction _afterburnerAction;
        private InputAction _freeLookAction;
        private InputAction _zoomAction;
        private InputAction _cameraDistanceAction;
        private bool _isFreeLookAllowed = true;

        // Controller
        public float Throttle { get; private set; }
        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Yaw { get; private set; }
        public bool Afterburner { get; private set; }

        // Camera
        public bool ChangeCamera { get; private set; }
        public bool RearView { get; private set; }
        public Vector2 CameraLook { get; private set; }
        public float CameraDistanceDelta { get; private set; }
        public bool FreeLook { get; private set; }
        public bool Zoom { get; private set; }

        // Weapon
        public bool FireWeapon { get; private set; }

        private void Awake()
        {
            _runtimeInputActions = Instantiate(_inputActions);
            _playerActions =
                _runtimeInputActions.FindActionMap("Fighter", true);

            _moveAction = _playerActions.FindAction("Move", true);
            _lookAction = _playerActions.FindAction("Look", true);
            _pitchAction = _playerActions.FindAction("Pitch", true);
            _rollAction = _playerActions.FindAction("Roll", true);
            _attackAction = _playerActions.FindAction("Attack", true);
            _changeCameraAction =
                _playerActions.FindAction("ChangeCamera", true);
            _rearViewAction =
                _playerActions.FindAction("RearView", true);
            _afterburnerAction =
                _playerActions.FindAction("Afterburner", true);
            _freeLookAction =
                _playerActions.FindAction("FreeLook", true);
            _zoomAction = _playerActions.FindAction("Zoom", true);
            _cameraDistanceAction =
                _playerActions.FindAction("CameraDistance", true);
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
            Vector2 scaledLook = look * MouseDeltaScale;
            FreeLook = _isFreeLookAllowed &&
                       _freeLookAction.IsPressed();

            Throttle = move.y;
            Yaw = move.x;
            Pitch = _pitchAction.ReadValue<float>();
            Roll = _rollAction.ReadValue<float>();

            if (!FreeLook)
            {
                Pitch += -scaledLook.y;
                Roll += -scaledLook.x;
            }

            Pitch = Mathf.Clamp(Pitch, -1f, 1f);
            Roll = Mathf.Clamp(Roll, -1f, 1f);

            Afterburner = _afterburnerAction.IsPressed();

            ChangeCamera = _changeCameraAction.WasPressedThisFrame();
            RearView = _rearViewAction.IsPressed();
            CameraLook = FreeLook ? scaledLook : Vector2.zero;
            CameraDistanceDelta =
                _cameraDistanceAction.ReadValue<float>();
            Zoom = _zoomAction.IsPressed();
            FireWeapon = _attackAction.IsPressed();
        }

        public void SetFreeLookAllowed(bool isAllowed)
        {
            _isFreeLookAllowed = isAllowed;
            if (isAllowed)
            {
                return;
            }

            FreeLook = false;
            CameraLook = Vector2.zero;
        }

        private void ResetInput()
        {
            Throttle = 0f;
            Pitch = 0f;
            Roll = 0f;
            Yaw = 0f;
            Afterburner = false;
            ChangeCamera = false;
            RearView = false;
            CameraLook = Vector2.zero;
            CameraDistanceDelta = 0f;
            FreeLook = false;
            Zoom = false;
            FireWeapon = false;
        }
    }
}
