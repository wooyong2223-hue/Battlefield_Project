using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Resolve heading before CharacterController movement uses transform.forward/right.
    [DefaultExecutionOrder(-50)]
    public sealed class PlayerCharacterView : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float _lookSensitivity = 3f;
        [SerializeField, Min(0f)] private float _pitchLimit = 80f;
        [SerializeField, Min(0f)] private float _freeLookYawLimit = 180f;
        [SerializeField, Min(0f)] private float _firstPersonFreeLookYawLimit = 90f;
        [SerializeField, Min(0.01f)] private float _freeLookReturnSpeed = 12f;
        [SerializeField, Min(0f)] private float _adsTapWindow = 0.2f;
        [SerializeField] private PlayerCharacterStance _stance;
        [SerializeField] private PlayerCharacterActionPolicy _actions;
        [SerializeField] private PlayerCharacterRecoil _recoil;

        [SerializeField] private ProneCameraProfile _proneCamera;
        private float _proneBlend, _proneBlendStart, _proneAimYaw;
        private uint _proneVersion;
        private bool _proneTransition;
        public ProneCameraProfile ProneCamera => _proneCamera;
        public float ProneBlend => _proneBlend;
        private float AimPitchUp => Mathf.Lerp(_pitchLimit, _proneCamera != null ? Mathf.Min(_pitchLimit, _proneCamera.AimPitchUp) : _pitchLimit, _proneBlend);
        private float AimPitchDown => Mathf.Lerp(_pitchLimit, _proneCamera != null ? Mathf.Min(_pitchLimit, _proneCamera.AimPitchDown) : _pitchLimit, _proneBlend);
        private float FreePitchUp => Mathf.Lerp(_pitchLimit, _proneCamera != null ? Mathf.Min(_pitchLimit, _proneCamera.FreePitchUp) : _pitchLimit, _proneBlend);
        private float FreePitchDown => Mathf.Lerp(_pitchLimit, _proneCamera != null ? Mathf.Min(_pitchLimit, _proneCamera.FreePitchDown) : _pitchLimit, _proneBlend);
        private float ClampAimPitch(float pitch) => Mathf.Clamp(pitch, -AimPitchUp, AimPitchDown);
        private float ClampFreePitch(float pitch) => Mathf.Clamp(pitch, -FreePitchUp, FreePitchDown);

        private IPlayerCharacterViewInput _input;
        private float _pitch;
        private float _freeYaw;
        private float _freePitch;
        private float _returnYaw;
        private float _returnPitch;
        private float _aimHeldTime;
        private bool _trackingAimPress;

        public bool IsFirstPerson { get; private set; }
        public bool IsAds { get; private set; }
        public bool IsAiming { get; private set; }
        public bool IsFreeLooking { get; private set; }
        public bool IsFirstPersonView => IsFirstPerson || IsAds;
        private Vector2 Recoil => _recoil != null ? _recoil.Offset : Vector2.zero;
        public Quaternion AimRotation => Quaternion.Euler(ClampAimPitch(_pitch - Recoil.x), transform.eulerAngles.y + _proneAimYaw + Recoil.y, 0f);
        public Quaternion LookRotation => IsFreeLooking
            ? Quaternion.Euler(ClampFreePitch(_freePitch), transform.eulerAngles.y + _proneAimYaw + _freeYaw, 0f)
            : Quaternion.Euler(ClampFreePitch(_pitch + _returnPitch - Recoil.x),
                transform.eulerAngles.y + _proneAimYaw + _returnYaw + Recoil.y, 0f);

        private void Awake() => _input = GetComponent<IPlayerCharacterViewInput>();

        private void OnEnable()
        {
            _proneAimYaw = 0f;
            _proneBlend = _stance != null && _stance.IsProne && _proneCamera != null ? 1f : 0f;
            _proneVersion = _stance != null ? _stance.ChangeVersion : 0;
            _proneTransition = false;
        }
        private void Update() => Tick(_input, Time.deltaTime);
        private void UpdateProneBlend(float deltaTime)
        {
            if (_stance == null || _proneCamera == null) { _proneBlend = _proneAimYaw = 0f; _proneTransition = false; return; }
            if (_proneVersion != _stance.ChangeVersion)
            {
                _proneVersion = _stance.ChangeVersion;
                _proneBlendStart = _proneBlend;
                _proneTransition = _stance.IsProneTransitioning;
            }
            if (!_stance.IsProne && !_stance.IsProneTransitioning && Mathf.Abs(_proneAimYaw) > .0001f)
            {
                // Preserve the viewed heading when the standing body takes ownership again.
                transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + _proneAimYaw, 0f);
                _proneAimYaw = 0f;
            }
            float target = _stance.IsProne ? 1f : 0f;
            _proneBlend = _proneTransition
                ? Mathf.Lerp(_proneBlendStart, target, Mathf.SmoothStep(0f, 1f, _stance.ProneTransitionProgress))
                : Mathf.MoveTowards(_proneBlend, target, deltaTime * _freeLookReturnSpeed);
            if (!_stance.IsProneTransitioning) _proneTransition = false;
        }

        public void Tick(IPlayerCharacterViewInput input, float deltaTime)
        {
            if (!isActiveAndEnabled || deltaTime <= 0f) return;
            UpdateProneBlend(deltaTime);
            _pitch = ClampAimPitch(_pitch);
            if (input == null || !input.HasControl || (_actions != null && !_actions.IsAlive))
            {
                ResetTransientState();
                return;
            }

            if (input.TogglePerspectivePressed)
            {
                IsFirstPerson = !IsFirstPerson;
                IsAds = false;
            }

            if (_actions != null && !_actions.CanAimWeapon)
            {
                IsAds = IsAiming = _trackingAimPress = false;
                _aimHeldTime = 0f;
            }
            else
            {
                if (input.AimPressed)
                {
                    _aimHeldTime = 0f;
                    _trackingAimPress = true;
                }
                if (input.AimHeld && _trackingAimPress) _aimHeldTime += deltaTime;
                if (input.AimReleased)
                {
                    if (_trackingAimPress && _aimHeldTime <= _adsTapWindow) IsAds = !IsAds;
                    _trackingAimPress = false;
                }
                IsAiming = input.AimHeld && !IsAds;
            }

            bool freeLook = input.FreeLookHeld && !IsAiming && !IsAds;
            Vector2 look = input.Look * _lookSensitivity;
            if (freeLook)
            {
                if (!IsFreeLooking)
                {
                    _freeYaw = _returnYaw;
                    _freePitch = _pitch + _returnPitch;
                }
                float yawLimit = IsFirstPersonView ? _firstPersonFreeLookYawLimit : _freeLookYawLimit;
                _freeYaw = Mathf.Clamp(_freeYaw + look.x, -yawLimit, yawLimit);
                _freePitch = ClampFreePitch(_freePitch - look.y);
            }
            else
            {
                if (IsFreeLooking)
                {
                    _returnYaw = _freeYaw;
                    _returnPitch = _freePitch - _pitch;
                }
                if (IsAiming || IsAds)
                {
                    _returnYaw = 0f;
                    _returnPitch = 0f;
                }
                float ratio = 1f - Mathf.Exp(-_freeLookReturnSpeed * deltaTime);
                _returnYaw = Mathf.Lerp(_returnYaw, 0f, ratio);
                _returnPitch = Mathf.Lerp(_returnPitch, 0f, ratio);
                _pitch = ClampAimPitch(_pitch - look.y);
                if (_proneCamera != null && _stance != null && _stance.IsProne)
                {
                    float desired = _proneAimYaw + look.x;
                    float held = Mathf.Clamp(desired, -_proneCamera.AimYawLimit, _proneCamera.AimYawLimit);
                    Quaternion heading = Quaternion.Euler(0f, transform.eulerAngles.y + desired - held, 0f);
                    if (_stance.AllowsHeading(heading)) transform.rotation = heading;
                    _proneAimYaw = held;
                }
                else
                {
                    Quaternion heading = Quaternion.Euler(0f, transform.eulerAngles.y + look.x, 0f);
                    if (_stance == null || _stance.AllowsHeading(heading)) transform.rotation = heading;
                }
            }
            IsFreeLooking = freeLook;
        }

        private void ResetTransientState()
        {
            IsAds = false;
            IsAiming = false;
            IsFreeLooking = false;
            _trackingAimPress = false;
            _returnYaw = 0f;
            _returnPitch = 0f;
        }

        private void OnDisable() => ResetTransientState();
    }
}
