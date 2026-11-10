using Battlefield.Features.Vehicles;
using Battlefield.Framework.Core;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerCharacterInput))]
    public sealed class PlayerVehicleOccupant : MonoBehaviour, IVehicleSeatOccupant
    {
        [SerializeField] private PlayerCharacterInput _input;
        [SerializeField] private SeatedCharacterPose _seatedPose;
        [SerializeField] private Behaviour[] _onFootComponents;
        [SerializeField] private Behaviour _vehicleCamera;
        [SerializeField] private GameObject _vehicleHud;

        private CharacterController _body;
        private Health _health;
        private Transform _standingParent;
        private Vector3 _standingScale;
        private Vector3 _entryPosition;
        private Quaternion _entryRotation;
        private bool[] _standingComponentEnabled;
        private bool _bodyWasEnabled;
        private bool _configured;

        public Transform Root => transform;
        public CharacterController CollisionBody => _body;
        public VehicleSeat CurrentSeat { get; private set; }
        public bool IsSeated => CurrentSeat != null;
        public bool HasInputControl => _input != null && _input.HasPointerCapture &&
                                       (_health == null || !_health.IsDead);
        public bool CanFireWeapon => HasInputControl && _input.CanFireWithCapturedPointer;
        public bool CanEnterSeat => _configured && isActiveAndEnabled && !IsSeated &&
                                    _seatedPose.CanPose && (_health == null || !_health.IsDead);

        private void Awake()
        {
            _body = GetComponent<CharacterController>();
            _health = GetComponent<Health>();
            _configured = _input != null && _seatedPose != null && _vehicleCamera != null &&
                          _onFootComponents != null && _onFootComponents.Length > 0;
            if (!_configured)
            {
                Debug.LogError("Player vehicle occupant references are missing.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (IsSeated || !_configured) return;
            _vehicleCamera.enabled = false;
            if (_vehicleHud != null) _vehicleHud.SetActive(false);
        }

        public bool TryAttachToSeat(VehicleSeat seat)
        {
            if (seat == null || seat.SeatPoint == null || !CanEnterSeat) return false;
            _standingParent = transform.parent;
            _standingScale = transform.localScale;
            _entryPosition = transform.position;
            _entryRotation = transform.rotation;
            _bodyWasEnabled = _body.enabled;
            _standingComponentEnabled = new bool[_onFootComponents.Length];
            for (int i = 0; i < _onFootComponents.Length; i++)
            {
                Behaviour component = _onFootComponents[i];
                _standingComponentEnabled[i] = component != null && component.enabled;
                if (component != null) component.enabled = false;
            }
            _body.enabled = false;
            transform.SetPositionAndRotation(seat.SeatPoint.position, seat.SeatPoint.rotation);
            if (!_seatedPose.BeginPose(seat.SeatPoint))
            {
                RestoreStanding(_entryPosition, _entryRotation);
                return false;
            }

            CurrentSeat = seat;
            // Keep this input component alive for F/Esc/capture, but suspend all on-foot commands.
            _input.SetGameplayEnabled(false);
            _vehicleCamera.enabled = true;
            if (_vehicleHud != null) _vehicleHud.SetActive(true);
            return true;
        }

        public void DetachFromSeat(VehicleSeat seat, Vector3 position, Quaternion rotation)
        {
            if (CurrentSeat != seat) return;
            CurrentSeat = null;
            if (_vehicleCamera != null) _vehicleCamera.enabled = false;
            if (_vehicleHud != null) _vehicleHud.SetActive(false);
            if (_seatedPose != null) _seatedPose.EndPose();
            // Scene teardown may have already destroyed cameras/input before the seat is released.
            if (_input == null || _body == null || !gameObject.scene.isLoaded) return;
            RestoreStanding(position, rotation);
            _input.SetGameplayEnabled(true);
        }

        private void RestoreStanding(Vector3 position, Quaternion rotation)
        {
            if (_standingParent != null && transform.parent != _standingParent)
                transform.SetParent(_standingParent, true);
            transform.localScale = _standingScale;
            transform.SetPositionAndRotation(position, rotation);
            _body.enabled = _bodyWasEnabled;
            if (_standingComponentEnabled == null) return;
            for (int i = 0; i < _onFootComponents.Length; i++)
                if (_onFootComponents[i] != null) _onFootComponents[i].enabled = _standingComponentEnabled[i];
        }

        private void OnDisable()
        {
            if (CurrentSeat != null) CurrentSeat.ReleaseOccupant(this);
        }
    }
}
