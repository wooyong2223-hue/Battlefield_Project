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
        [SerializeField] private float _thirdPersonOrbitFollowSpeed = 5f;

        [Header("Third Person Roll")]
        [SerializeField] private float _thirdPersonRollMultiplier = 0.1f;
        [SerializeField] private float _thirdPersonRollFollowSpeed = 5f;
        [SerializeField] private float _thirdPersonRollReturnSpeed = 3f;

        private readonly JetCameraRollFollow _rollFollow = new();
        private float _freeLookYaw;
        private float _freeLookPitch;
        private float _thirdPersonDistance;
        private Quaternion _thirdPersonOrbitRotation;
        private bool _isThirdPersonOrbitInitialized;

        internal Quaternion TargetOrbitRotation { get; private set; }
        internal Quaternion SmoothedOrbitRotation { get; private set; }
        internal Quaternion AppliedOrbitRotation { get; private set; }
        internal float AppliedRoll { get; private set; }
        internal float ThirdPersonDistance => _thirdPersonDistance;
        internal Vector3 AppliedCameraOffset { get; private set; }

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

            float cameraRoll;
            Quaternion baseRotation = GetViewPointRotation(
                viewPoint,
                isFirstPerson,
                isRearView,
                deltaTime,
                out cameraRoll);
            Vector3 cameraOffset = viewPoint.position - fighter.position;

            if (!isFirstPerson && !isRearView)
            {
                Vector3 targetOrbitDirection = cameraOffset.normalized;
                Vector3 targetForward = -targetOrbitDirection;

                if (!_isThirdPersonOrbitInitialized)
                {
                    TargetOrbitRotation = Quaternion.LookRotation(
                        targetForward,
                        baseRotation * Vector3.up);
                    _thirdPersonOrbitRotation = TargetOrbitRotation;
                    _isThirdPersonOrbitInitialized = true;
                }
                else
                {
                    Vector3 currentForward =
                        _thirdPersonOrbitRotation * Vector3.forward;
                    Quaternion directionChange = Quaternion.FromToRotation(
                        currentForward,
                        targetForward);
                    TargetOrbitRotation =
                        directionChange * _thirdPersonOrbitRotation;
                    float followRatio = 1f - Mathf.Exp(
                        -Mathf.Max(0f, _thirdPersonOrbitFollowSpeed)
                        * deltaTime);
                    _thirdPersonOrbitRotation = Quaternion.Slerp(
                        _thirdPersonOrbitRotation,
                        TargetOrbitRotation,
                        followRatio);
                }

                baseRotation = _thirdPersonOrbitRotation
                    * Quaternion.AngleAxis(
                        cameraRoll,
                        Vector3.forward);
                SmoothedOrbitRotation = _thirdPersonOrbitRotation;
                AppliedOrbitRotation = baseRotation;
                AppliedRoll = cameraRoll;
                cameraOffset = -(baseRotation * Vector3.forward)
                    * _thirdPersonDistance;
            }

            Quaternion freeLookRotation = Quaternion.Euler(
                _freeLookPitch,
                _freeLookYaw,
                0f);
            Quaternion worldOrbitRotation = baseRotation
                * freeLookRotation
                * Quaternion.Inverse(baseRotation);

            AppliedCameraOffset = worldOrbitRotation * cameraOffset;
            cameraTransform.position = fighter.position
                + AppliedCameraOffset;
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
            float deltaTime,
            out float cameraRoll)
        {
            if (isFirstPerson || isRearView)
            {
                _isThirdPersonOrbitInitialized = false;
                _rollFollow.Reset();
                cameraRoll = 0f;
                return viewPoint.rotation;
            }

            Quaternion targetOrbitRotation = Quaternion.LookRotation(
                viewPoint.forward,
                Vector3.up);

            float roll = Vector3.SignedAngle(
                targetOrbitRotation * Vector3.up,
                viewPoint.up,
                viewPoint.forward);
            cameraRoll = _rollFollow.Update(
                roll * _thirdPersonRollMultiplier,
                _thirdPersonRollFollowSpeed,
                _thirdPersonRollReturnSpeed,
                deltaTime);
            return targetOrbitRotation;
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
