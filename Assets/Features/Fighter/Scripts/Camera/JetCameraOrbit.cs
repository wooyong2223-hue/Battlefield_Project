using System;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [Serializable]
    public sealed class JetCameraOrbit
    {
        [Header("Free Look")]
        [SerializeField] private float _freeLookSensitivity = 1f;
        [SerializeField] private float _maximumFreeLookPitch = 80f;

        [Header("Third Person")]
        [SerializeField] private float _cameraDistanceStep = 2f;
        [SerializeField] private float _minimumThirdPersonDistance = 5f;
        [SerializeField] private float _maximumThirdPersonDistance = 40f;

        [Header("Third Person Direction")]
        [SerializeField] private float _thirdPersonPitchFollowSpeed = 14f;
        [SerializeField] private float _maximumPitchLagAngle = 4f;
        [SerializeField] private float _thirdPersonYawFollowSpeed = 7f;
        [SerializeField] private float _maximumYawLagAngle = 10f;

        [Header("Third Person Roll")]
        [SerializeField] private float _thirdPersonRollMultiplier = 0.1f;
        [SerializeField] private float _thirdPersonRollFollowSpeed = 5f;
        [SerializeField] private float _thirdPersonRollReturnSpeed = 3f;

        private readonly JetCameraDirectionFollow _directionFollow = new();
        private readonly JetCameraRollFollow _rollFollow = new();
        private float _freeLookYaw;
        private float _freeLookPitch;
        private float _thirdPersonDistance;

        public void Initialize(
            Transform fighter,
            Transform thirdPersonCameraPoint)
        {
            float authoredDistance = Vector3.Distance(
                fighter.position,
                thirdPersonCameraPoint.position);
            _thirdPersonDistance = Mathf.Clamp(
                authoredDistance,
                _minimumThirdPersonDistance,
                _maximumThirdPersonDistance);
        }

        public void UpdatePose(
            Transform cameraTransform,
            Transform viewPoint,
            Transform fighter,
            IJetInput input,
            bool isFirstPerson,
            bool isRearView,
            float deltaTime)
        {
            UpdateFreeLook(input);
            UpdateCameraDistance(
                input.CameraDistanceDelta,
                isFirstPerson,
                isRearView);

            Quaternion baseRotation = GetViewPointRotation(
                viewPoint,
                isFirstPerson,
                isRearView,
                deltaTime);
            Quaternion freeLookRotation = Quaternion.Euler(
                _freeLookPitch,
                _freeLookYaw,
                0f);
            Quaternion worldOrbitRotation = baseRotation
                * freeLookRotation
                * Quaternion.Inverse(baseRotation);
            Vector3 cameraOffset = viewPoint.position - fighter.position;

            if (!isFirstPerson && !isRearView)
            {
                cameraOffset = cameraOffset.normalized
                    * _thirdPersonDistance;
            }

            cameraTransform.position = fighter.position
                + worldOrbitRotation * cameraOffset;
            cameraTransform.rotation = baseRotation * freeLookRotation;
        }

        private void UpdateFreeLook(IJetInput input)
        {
            if (input.FreeLook)
            {
                Vector2 look = input.CameraLook * _freeLookSensitivity;
                _freeLookYaw = Mathf.Repeat(
                    _freeLookYaw + look.x + 180f,
                    360f) - 180f;
                _freeLookPitch = Mathf.Clamp(
                    _freeLookPitch - look.y,
                    -_maximumFreeLookPitch,
                    _maximumFreeLookPitch);
                return;
            }

            _freeLookYaw = 0f;
            _freeLookPitch = 0f;
        }

        private void UpdateCameraDistance(
            float scroll,
            bool isFirstPerson,
            bool isRearView)
        {
            if (isFirstPerson || isRearView || Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            _thirdPersonDistance = Mathf.Clamp(
                _thirdPersonDistance - Mathf.Sign(scroll) * _cameraDistanceStep,
                _minimumThirdPersonDistance,
                _maximumThirdPersonDistance);
        }

        private Quaternion GetViewPointRotation(
            Transform viewPoint,
            bool isFirstPerson,
            bool isRearView,
            float deltaTime)
        {
            if (isFirstPerson || isRearView)
            {
                _directionFollow.Reset();
                _rollFollow.Reset();
                return viewPoint.rotation;
            }

            Quaternion targetNoRollRotation = Quaternion.LookRotation(
                viewPoint.forward,
                Vector3.up);
            float roll = Vector3.SignedAngle(
                targetNoRollRotation * Vector3.up,
                viewPoint.up,
                viewPoint.forward);
            Quaternion noRollRotation = _directionFollow.Update(
                targetNoRollRotation * Vector3.forward,
                _thirdPersonPitchFollowSpeed,
                _maximumPitchLagAngle,
                _thirdPersonYawFollowSpeed,
                _maximumYawLagAngle,
                deltaTime);
            float cameraRoll = _rollFollow.Update(
                roll * _thirdPersonRollMultiplier,
                _thirdPersonRollFollowSpeed,
                _thirdPersonRollReturnSpeed,
                deltaTime);
            return noRollRotation * Quaternion.AngleAxis(
                cameraRoll,
                Vector3.forward);
        }
    }

    internal sealed class JetCameraDirectionFollow
    {
        private float _pitch;
        private float _yaw;
        private bool _isInitialized;

        public Quaternion Update(
            Vector3 targetForward,
            float pitchFollowSpeed,
            float maximumPitchLagAngle,
            float yawFollowSpeed,
            float maximumYawLagAngle,
            float deltaTime)
        {
            Vector3 normalizedForward = targetForward.normalized;
            float targetPitch = -Mathf.Asin(
                Mathf.Clamp(normalizedForward.y, -1f, 1f))
                * Mathf.Rad2Deg;
            Vector3 horizontalForward = Vector3.ProjectOnPlane(
                normalizedForward,
                Vector3.up);
            float targetYaw = horizontalForward.sqrMagnitude > 0.1f
                ? Mathf.Atan2(horizontalForward.x, horizontalForward.z)
                    * Mathf.Rad2Deg
                : _yaw;

            if (!_isInitialized)
            {
                _pitch = targetPitch;
                _yaw = targetYaw;
                _isInitialized = true;
                return Quaternion.Euler(_pitch, _yaw, 0f);
            }

            _pitch = FollowAngle(
                _pitch,
                targetPitch,
                pitchFollowSpeed,
                maximumPitchLagAngle,
                deltaTime);
            _yaw = FollowAngle(
                _yaw,
                targetYaw,
                yawFollowSpeed,
                maximumYawLagAngle,
                deltaTime);

            return Quaternion.Euler(_pitch, _yaw, 0f);
        }

        public void Reset()
        {
            _isInitialized = false;
        }

        private static float FollowAngle(
            float currentAngle,
            float targetAngle,
            float followSpeed,
            float maximumLagAngle,
            float deltaTime)
        {
            float followRatio = 1f - Mathf.Exp(
                -Mathf.Max(0f, followSpeed) * deltaTime);
            float smoothedAngle = Mathf.LerpAngle(
                currentAngle,
                targetAngle,
                followRatio);
            float maximumLag = Mathf.Max(0f, maximumLagAngle);
            float remainingAngle = Mathf.Abs(
                Mathf.DeltaAngle(smoothedAngle, targetAngle));

            return remainingAngle <= maximumLag
                ? smoothedAngle
                : Mathf.MoveTowardsAngle(
                    targetAngle,
                    smoothedAngle,
                    maximumLag);
        }
    }

    internal sealed class JetCameraRollFollow
    {
        private float _roll;
        private bool _isInitialized;

        public float Update(
            float targetRoll,
            float followSpeed,
            float returnSpeed,
            float deltaTime)
        {
            if (!_isInitialized)
            {
                _roll = targetRoll;
                _isInitialized = true;
                return _roll;
            }

            bool isReturningToLevel =
                Mathf.Abs(targetRoll) < Mathf.Abs(_roll);
            float changeSpeed = isReturningToLevel
                ? returnSpeed
                : followSpeed;
            float changeRatio = 1f - Mathf.Exp(
                -Mathf.Max(0f, changeSpeed) * deltaTime);
            _roll = Mathf.LerpAngle(
                _roll,
                targetRoll,
                changeRatio);

            return _roll;
        }

        public void Reset()
        {
            _isInitialized = false;
        }
    }
}
