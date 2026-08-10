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
        [SerializeField] private float _thirdPersonRollMultiplier = 1f;
        [SerializeField] private float _cameraDistanceStep = 2f;
        [SerializeField] private float _minimumThirdPersonDistance = 5f;
        [SerializeField] private float _maximumThirdPersonDistance = 40f;

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
            bool isRearView)
        {
            UpdateFreeLook(input);
            UpdateCameraDistance(
                input.CameraDistanceDelta,
                isFirstPerson,
                isRearView);

            Quaternion baseRotation = GetViewPointRotation(
                viewPoint,
                isFirstPerson,
                isRearView);
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
            bool isRearView)
        {
            if (isFirstPerson || isRearView) return viewPoint.rotation;

            Quaternion noRollRotation = Quaternion.LookRotation(
                viewPoint.forward,
                Vector3.up);
            float roll = Vector3.SignedAngle(
                noRollRotation * Vector3.up,
                viewPoint.up,
                viewPoint.forward);

            return noRollRotation * Quaternion.AngleAxis(
                roll * _thirdPersonRollMultiplier,
                Vector3.forward);
        }
    }
}
