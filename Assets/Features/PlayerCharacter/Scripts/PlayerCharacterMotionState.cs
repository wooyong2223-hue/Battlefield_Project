using System;
using UnityEngine;

namespace Battlefield.Features.PlayerCharacter
{
    // Presentation only: never changes movement, input, ammunition or aiming authority.
    public sealed class PlayerCharacterMotionState
    {
        private readonly PlayerCharacterMotionProfile _profile;
        private Quaternion _previousAim;
        private Vector3 _sway;
        private float _phase, _gait, _sprint, _landing, _landingVelocity, _previousVertical;
        private bool _initialized, _wasGrounded;
        private float _jumpLag, _jumpVelocity;
        public Vector3 JumpCameraOffset { get; private set; }
        public float CameraPitch { get; private set; }
        public Vector3 CameraOffset { get; private set; }
        // Expose the already-computed landing contribution so asset-driven camera
        // motion can replace it without altering the profile or the spring state.
        public Vector3 LandingCameraOffset { get; private set; }
        public float CameraRoll { get; private set; }
        public Vector3 WeaponOffset { get; private set; }
        public Vector3 WeaponEuler { get; private set; }
        public float AimBlend { get; private set; }

        public PlayerCharacterMotionState(PlayerCharacterMotionProfile profile)
        {
            _profile = profile != null ? profile : throw new ArgumentNullException(nameof(profile));
        }

        public void Reset()
        {
            _initialized = _wasGrounded = false;
            _phase = _gait = _sprint = _landing = _landingVelocity = _previousVertical = AimBlend = CameraRoll = 0f;
            _sway = CameraOffset = WeaponOffset = WeaponEuler = JumpCameraOffset = Vector3.zero;
            LandingCameraOffset = Vector3.zero;
            _jumpLag = _jumpVelocity = CameraPitch = 0f;
        }

        public void Tick(Vector3 localVelocity, bool grounded, bool sprinting, bool ads,
            Quaternion aim, float walkSpeed, bool active, float deltaTime)
        {
            if (deltaTime <= 0f) return;
            if (!active)
            {
                Reset();
                AimBlend = ads ? 1f : 0f;
                return;
            }
            if (!_initialized)
            {
                _previousAim = aim;
                _wasGrounded = grounded;
                _previousVertical = localVelocity.y;
                _initialized = true;
            }
            float response = 1f - Mathf.Exp(-_profile.Response * deltaTime);
            AimBlend = Mathf.Lerp(AimBlend, ads ? 1f : 0f, 1f - Mathf.Exp(-_profile.AimResponse * deltaTime));
            float speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
            _gait = Mathf.Lerp(_gait, grounded ? Mathf.Clamp01(speed / Mathf.Max(.01f, walkSpeed)) : 0f, response);
            _sprint = Mathf.Lerp(_sprint, sprinting && grounded && !ads ? 1f : 0f, response);
            // Integrate distance, not elapsed time, so idle/wall-blocked motion has no running cycle.
            if (grounded && speed > .05f)
                _phase = Mathf.Repeat(_phase + speed * deltaTime / Mathf.Lerp(_profile.WalkStride, _profile.SprintStride, _sprint) * Mathf.PI * 2f, Mathf.PI * 2f);

            Vector3 angles = aim.eulerAngles;
            Vector3 previous = _previousAim.eulerAngles;
            Vector3 angular = new(Mathf.DeltaAngle(previous.x, angles.x) / deltaTime,
                Mathf.DeltaAngle(previous.y, angles.y) / deltaTime, 0f);
            Vector3 sway = new(angular.x * _profile.LookSway.x, -angular.y * _profile.LookSway.y,
                -angular.y * _profile.LookSway.z);
            sway = Vector3.ClampMagnitude(sway, _profile.MaximumSway);
            _sway = Vector3.Lerp(_sway, sway, response);
            _previousAim = aim;

            if (grounded && !_wasGrounded && _previousVertical < -1f)
                _landingVelocity -= Mathf.Min(-_previousVertical * _profile.LandingImpulse, _profile.MaximumLandingImpulse);
            // Exact critically damped spring step: stable for different frame rates, no accumulating drift.
            float omega = _profile.LandingRecovery;
            float spring = _landingVelocity + omega * _landing;
            float decay = Mathf.Exp(-omega * deltaTime);
            _landing = (_landing + spring * deltaTime) * decay;
            _landingVelocity = (_landingVelocity - omega * spring * deltaTime) * decay;
            // Follow vertical acceleration with bounded camera inertia, not a new movement force.
            float targetLag = grounded ? 0f : Mathf.Clamp(-localVelocity.y * _profile.JumpVerticalLag,
                -_profile.MaximumJumpLag, _profile.MaximumJumpLag);
            float error = _jumpLag - targetLag;
            float jumpSpring = _jumpVelocity + _profile.JumpRecovery * error;
            float jumpDecay = Mathf.Exp(-_profile.JumpRecovery * deltaTime);
            _jumpLag = targetLag + (error + jumpSpring * deltaTime) * jumpDecay;
            _jumpVelocity = (_jumpVelocity - _profile.JumpRecovery * jumpSpring * deltaTime) * jumpDecay;
            _wasGrounded = grounded;
            _previousVertical = localVelocity.y;

            float side = Mathf.Sin(_phase);
            float down = (Mathf.Cos(_phase * 2f) - 1f) * .5f;
            float stabilization = Mathf.Lerp(1f, _profile.AdsMotionMultiplier, AimBlend);
            Vector2 amplitude = Vector2.Lerp(_profile.WalkCameraBob, _profile.SprintCameraBob, _sprint) * (_gait * stabilization);
            LandingCameraOffset = Vector3.up * (_landing * stabilization);
            CameraOffset = new Vector3(side * amplitude.x, down * amplitude.y, 0f) + LandingCameraOffset;
            CameraRoll = side * _gait * _profile.CameraRoll * stabilization;
            // CameraOffset already contains one landing displacement for first person.
            JumpCameraOffset = Vector3.up * (_jumpLag + _landing * (_profile.LandingCameraMultiplier - 1f)) * stabilization;
            CameraPitch = (_jumpLag * _profile.JumpPitchPerMeter - _landing * _profile.LandingPitchPerMeter) * stabilization;
            WeaponOffset = Vector3.Scale(_profile.WeaponBob, new Vector3(side, down, -Mathf.Abs(side))) * (_gait * stabilization)
                + _profile.SprintOffset * _sprint;
            WeaponEuler = _sway * stabilization + _profile.SprintEuler * _sprint;
        }
    }
}
