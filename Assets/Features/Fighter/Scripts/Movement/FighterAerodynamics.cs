using System;
using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [Serializable]
    internal sealed class FighterAerodynamics
    {
        [SerializeField, Min(1f)] private float _levelFlightSpeed = 60f;
        [SerializeField, Min(0f)] private float _liftPerDegree = 0.10f;
        [SerializeField, Min(0f)] private float _stallAngle = 16f;
        [SerializeField, Min(1f)] private float _stallFadeAngle = 35f;
        [SerializeField, Min(0f)] private float _dragAtLevelFlightSpeed = 0.36f;
        [SerializeField, Min(0f)] private float _sideSlipDamping = 1.35f;

        [Header("Unpowered Glide")]
        [SerializeField, Min(0f)] private float _coastBaseDeceleration = 1f;
        [SerializeField, Min(0f)] private float _coastInducedDeceleration = 1.4f;
        [SerializeField, Min(1f)] private float _coastMinimumSpeed = 25f;

        [Header("Pitch Response")]
        [SerializeField, Min(0f)] private float _flightPathResponse = 2f;
        [SerializeField, Min(0f)] private float _maximumFlightPathAcceleration = 22f;

        private const float PitchAssistDeadZone = 5f;
        private const float PitchAssistFullAngle = 15f;
        private const float PitchInputAccelerationFraction = 0.65f;
        private const float MinimumBankLiftProjection = 0.5f;
        private const float BankPitchFullAuthorityProjection = 0.25f;
        private const float MaximumBankPitchBlend = 0.75f;

        internal float LevelFlightSpeed => Mathf.Max(1f, _levelFlightSpeed);

        internal Vector3 CalculateAcceleration(
            Vector3 velocity, Quaternion attitude, float gravity, float sideSlipMultiplier,
            bool useCoastDrag, bool useFlightAssist, float pitchInput, float directFlightWeight)
        {
            float speed = velocity.magnitude;
            if (speed < 0.01f) return Vector3.zero;

            float levelFlightSpeed = LevelFlightSpeed;
            Vector3 direction = velocity / speed;
            float speedRatio = speed / levelFlightSpeed;
            Vector3 localVelocity = Quaternion.Inverse(attitude) * velocity;
            float dragMagnitude = _dragAtLevelFlightSpeed * speedRatio * speedRatio;
            if (useCoastDrag)
                dragMagnitude = CalculateCoastDrag(speed, localVelocity.z, dragMagnitude);
            Vector3 drag = -direction * dragMagnitude;
            if (localVelocity.z <= 0f) return drag;

            float forwardRatio = Mathf.Clamp01(localVelocity.z / levelFlightSpeed);
            float angleOfAttack = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
            float stall = Mathf.InverseLerp(_stallAngle, Mathf.Max(_stallAngle + 1f, _stallFadeAngle),
                Mathf.Abs(angleOfAttack));
            float liftFactor = Mathf.Clamp(1f + angleOfAttack * _liftPerDegree, -1f, 2.6f) * (1f - stall);
            Vector3 liftDirection = Vector3.ProjectOnPlane(attitude * Vector3.up, direction).normalized;
            Vector3 pathUp = useFlightAssist ? Vector3.ProjectOnPlane(Vector3.up, direction).normalized : Vector3.zero;
            if (useFlightAssist && liftFactor > 0f)
                liftFactor *= Mathf.Lerp(CalculateBankLiftMultiplier(liftDirection, pathUp), 1f, directFlightWeight);
            Vector3 lift = liftDirection * (gravity * forwardRatio * forwardRatio * liftFactor);
            Vector3 sideSlip = (attitude * Vector3.right) *
                (-localVelocity.x * _sideSlipDamping * forwardRatio * sideSlipMultiplier);

            Vector3 pitchAssist = useFlightAssist
                ? CalculatePitchAssist(velocity, direction, attitude, speed, forwardRatio)
                : Vector3.zero;
            if (useFlightAssist && pitchInput != 0f)
                pitchAssist = ApplyPitchInput(pitchAssist, pitchInput, direction, attitude, pathUp, forwardRatio);

            return lift + drag + sideSlip + pitchAssist * (1f - directFlightWeight);
        }

        private static float CalculateBankLiftMultiplier(Vector3 liftDirection, Vector3 pathUp)
        {
            return BankLiftMultiplier(Vector3.Dot(liftDirection, pathUp));
        }

        private static float BankLiftMultiplier(float upwardProjection)
        {
            float weight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, MinimumBankLiftProjection, upwardProjection));
            // Fade out near knife-edge and inverted flight instead of dividing by a vanishing projection.
            return Mathf.Lerp(1f, 1f / Mathf.Max(MinimumBankLiftProjection, upwardProjection), weight);
        }

        private Vector3 ApplyPitchInput(Vector3 attitudeAssist, float pitchInput, Vector3 direction,
            Quaternion attitude, Vector3 pathUp, float forwardRatio)
        {
            float accelerationLimit = Mathf.Max(0f, _maximumFlightPathAcceleration) * forwardRatio * forwardRatio;
            Vector3 wingAxis = attitude * Vector3.right;
            Vector3 turnDirection = Vector3.Cross(direction, wingAxis).normalized;
            float bank = Mathf.Abs(Vector3.Dot(wingAxis, pathUp));
            float upwardProjection = Vector3.Dot(turnDirection, pathUp);
            float weight = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(0f, BankPitchFullAuthorityProjection, upwardProjection));
            turnDirection = Vector3.Slerp(turnDirection, pathUp, bank * weight * MaximumBankPitchBlend);
            Vector3 inputAssist = turnDirection * (-pitchInput * accelerationLimit * PitchInputAccelerationFraction);

            // While the nose catches up, opposing attitude assist would delay a reversal.
            if (Vector3.Dot(attitudeAssist, inputAssist) < 0f)
                attitudeAssist = Vector3.zero;

            return Vector3.ClampMagnitude(attitudeAssist + inputAssist, accelerationLimit);
        }

        private Vector3 CalculatePitchAssist(
            Vector3 velocity, Vector3 direction, Quaternion attitude, float speed, float forwardRatio)
        {
            float noseVertical = (attitude * Vector3.forward).y;
            float pitch = Mathf.Asin(Mathf.Clamp(noseVertical, -1f, 1f)) * Mathf.Rad2Deg;
            float pitchWeight = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(PitchAssistDeadZone, PitchAssistFullAngle, Mathf.Abs(pitch)));
            if (pitchWeight <= 0f) return Vector3.zero;

            float verticalError = noseVertical * speed - velocity.y;
            if (noseVertical * verticalError <= 0f) return Vector3.zero;

            float upright = Mathf.Clamp01((attitude * Vector3.up).y);
            float maxAcceleration = Mathf.Max(0f, _maximumFlightPathAcceleration);
            float acceleration = Mathf.Clamp(verticalError * _flightPathResponse, -maxAcceleration, maxAcceleration);

            // Turn the flight path without directly adding speed along it.
            Vector3 turnAxis = Vector3.ProjectOnPlane(Vector3.up, direction).normalized;
            return turnAxis * (acceleration * pitchWeight * upright * forwardRatio * forwardRatio);
        }

        private float CalculateCoastDrag(float airspeed, float forwardSpeed, float standardDrag)
        {
            float minimumSpeed = Mathf.Max(1f, _coastMinimumSpeed);
            float inverseSpeedRatio = LevelFlightSpeed / Mathf.Max(airspeed, minimumSpeed);
            float coastDrag = _coastBaseDeceleration +
                _coastInducedDeceleration * inverseSpeedRatio * inverseSpeedRatio;

            // Fade the steady-lift approximation when there is little forward airflow.
            float forwardFlow = Mathf.SmoothStep(0f, 1f, forwardSpeed / minimumSpeed);
            return Mathf.Lerp(standardDrag, coastDrag, forwardFlow);
        }
    }
}
