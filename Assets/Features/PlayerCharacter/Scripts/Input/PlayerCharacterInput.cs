using Battlefield.Features.Weapon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(-100)]
    public sealed class PlayerCharacterInput : MonoBehaviour, IPlayerCharacterThrowInput, IPlayerCharacterInput, IPlayerCharacterViewInput, IPlayerCharacterStanceInput, IPlayerCharacterTraversalInput, IPlayerCharacterTacticalInput, IPlayerCharacterLifeInput, IPlayerCharacterCombatInput, IPlayerCharacterInteractionInput, IWeaponInput
    {
        private const float MouseDeltaScale = 0.1f;
        [SerializeField] private InputActionAsset _inputActions;

        private InputActionAsset _runtimeInputActions;
        private InputActionMap _actions;
        private InputAction _throw;
        public bool EquipGrenadePressed => HasControl && _throw != null && _throw.WasPressedThisFrame();
        public bool ThrowGrenadePressed => HasControl && !_suppressFireUntilRelease && _attack != null && _attack.WasPressedThisFrame();
        // Grenade release must remain visible after its press consumes weapon fire.
        public bool ThrowGrenadeHeld => HasControl && _attack != null && _attack.IsPressed();
        public bool ThrowGrenadeReleased => HasControl && _attack != null && _attack.WasReleasedThisFrame();
        public bool CanControlGrenade => HasControl;
        public bool ToggleGrenadeThrowModePressed => HasControl && _aim != null && _aim.WasPressedThisFrame();
        public void ConsumePrimaryUntilRelease() => _suppressFireUntilRelease = true;
        public void ConsumeAimUntilRelease() => _suppressAimUntilRelease = true;
        private InputAction _move;
        private InputAction _look;
        private InputAction _attack;
        private InputAction _aim;
        private InputAction _togglePerspective;
        private InputAction _freeLook;
        private InputAction _releaseCursor;
        private InputAction _sprint;
        private InputAction _jump;
        private InputAction _interact;
        private InputAction _crouch;
        private InputAction _prone;
        private InputAction _slide;
        private InputAction _lean, _mount;
        private InputAction _toggleFireMode;
        private InputAction _giveUp, _respawn;
        private InputAction _reload, _melee, _selectPistol, _selectUnarmed, _selectRifle;

        private bool _captured;
        private bool _suppressFireUntilRelease;
        private bool _suppressAimUntilRelease;
        private int _captureFrame = -1;
        private CursorLockMode _previousLockState;
        private bool _previousCursorVisible;
        private bool _gameplayEnabled = true;

        public bool HasPointerCapture => isActiveAndEnabled && _captured && Time.frameCount != _captureFrame;
        public bool HasControl => _gameplayEnabled && HasPointerCapture;
        public bool CanFireWithCapturedPointer => HasPointerCapture && !_suppressFireUntilRelease;
        public bool InteractPressed => HasPointerCapture && _interact != null && _interact.WasPressedThisFrame();
        public bool CrouchPressed => HasControl && _crouch != null && _crouch.WasPressedThisFrame();
        public bool SlidePressed => HasControl && _slide != null && _slide.WasPressedThisFrame();
        public bool PronePressed => HasControl && _prone != null && _prone.WasPressedThisFrame();
        public float Lean => HasControl && _lean != null ? _lean.ReadValue<float>() : 0f;
        public bool MountHeld => HasControl && _mount != null && _mount.IsPressed();
        public bool InteractHeld => HasControl && _interact != null && _interact.IsPressed();
        public bool GiveUpPressed => HasPointerCapture && _giveUp != null && _giveUp.WasPressedThisFrame();
        public bool RespawnPressed => HasPointerCapture && _respawn != null && _respawn.WasPressedThisFrame();
        public Vector2 Move => HasControl && _move != null
            ? _move.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => HasControl && _look != null
            ? _look.ReadValue<Vector2>() * MouseDeltaScale : Vector2.zero;
        public bool TogglePerspectivePressed => HasControl && _togglePerspective.WasPressedThisFrame();
        public bool FreeLookHeld => HasControl && _freeLook.IsPressed();
        public bool AimHeld => HasControl && !_suppressAimUntilRelease && _aim.IsPressed();
        public bool AimPressed => HasControl && !_suppressAimUntilRelease && _aim.WasPressedThisFrame();
        public bool AimReleased => HasControl && !_suppressAimUntilRelease && _aim.WasReleasedThisFrame();
        public bool SprintHeld => HasControl && _sprint != null && _sprint.IsPressed();
        public bool JumpPressed => HasControl && _jump != null && _jump.WasPressedThisFrame();
        public bool FireWeapon => HasControl && !_suppressFireUntilRelease && _attack != null && _attack.IsPressed();
        public bool FirePressed => HasControl && !_suppressFireUntilRelease && _attack != null && _attack.WasPressedThisFrame();
        public bool ToggleFireModePressed => HasControl && _toggleFireMode != null && _toggleFireMode.WasPressedThisFrame();
        public bool ReloadPressed => HasControl && _reload.WasPressedThisFrame();
        public bool MeleePressed => HasControl && _melee.WasPressedThisFrame();
        public int WeaponSlotSelection => !HasControl ? 0 : _selectPistol.WasPressedThisFrame() ? PlayerCharacterLoadout.PistolSlot :
            _selectUnarmed.WasPressedThisFrame() ? PlayerCharacterLoadout.UnarmedSlot : _selectRifle != null && _selectRifle.WasPressedThisFrame() ? PlayerCharacterLoadout.RifleSlot : 0;

        private void Awake()
        {
            if (_inputActions == null)
            {
                Debug.LogError("Player character input actions are missing.", this);
                enabled = false;
                return;
            }

            _runtimeInputActions = Instantiate(_inputActions);
            _actions = _runtimeInputActions.FindActionMap("PlayerCharacter", true);
            _throw = _actions.FindAction("ThrowGrenade", false);
            _move = _actions.FindAction("Move", true);
            _look = _actions.FindAction("Look", true);
            _attack = _actions.FindAction("Attack", true);
            _aim = _actions.FindAction("Aim", true);
            _togglePerspective = _actions.FindAction("TogglePerspective", true);
            _freeLook = _actions.FindAction("FreeLook", true);
            _releaseCursor = _actions.FindAction("ReleaseCursor", true);
            _sprint = _actions.FindAction("Sprint", true);
            _jump = _actions.FindAction("Jump", true);
            _interact = _actions.FindAction("Interact", true);
            _crouch = _actions.FindAction("Crouch", true);
            _prone = _actions.FindAction("Prone", true);
            _slide = _actions.FindAction("Slide", true);
            _lean = _actions.FindAction("Lean", true);
            _mount = _actions.FindAction("Mount", true);
            _toggleFireMode = _actions.FindAction("ToggleFireMode", true);
            _giveUp = _actions.FindAction("GiveUp", true);
            _respawn = _actions.FindAction("Respawn", true);
            _reload = _actions.FindAction("Reload", true);
            _melee = _actions.FindAction("Melee", true);
            _selectPistol = _actions.FindAction("SelectPistol", true);
            _selectUnarmed = _actions.FindAction("SelectUnarmed", true);
            _selectRifle = _actions.FindAction("SelectRifle", false);
        }

        public void SetGameplayEnabled(bool enabledForWalking)
        {
            _gameplayEnabled = enabledForWalking;
            _captureFrame = Time.frameCount;
            _suppressFireUntilRelease = true;
        }

        private void OnEnable()
        {
            if (_actions == null) return;
            _actions?.Enable();
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            SetCaptured(true);
        }

        private void Update()
        {
            if (_actions == null) return;
            if (_releaseCursor.WasPressedThisFrame()) SetCaptured(false);
            else if (!_captured && _attack.WasPressedThisFrame())
            {
                SetCaptured(true);
                _suppressFireUntilRelease = true;
            }
            if (!_attack.IsPressed()) _suppressFireUntilRelease = false;
            if (!_aim.IsPressed()) _suppressAimUntilRelease = false;
        }

        private void SetCaptured(bool captured)
        {
            _captured = captured;
            _captureFrame = captured ? Time.frameCount : -1;
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) SetCaptured(false);
        }

        private void OnDisable()
        {
            _actions?.Disable();
            _captured = false;
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;
        }

        private void OnDestroy()
        {
            if (_runtimeInputActions != null) Destroy(_runtimeInputActions);
        }
    }
}
