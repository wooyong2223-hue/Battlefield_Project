using UnityEngine;

namespace Battlefield.Features.Fighter
{
    internal sealed class FighterFlightAssist
    {
        private const float AlignmentDegreesPerSecond = 90f;
        private const float LevelNoseThreshold = 0.105f; // sin(6 degrees)
        private const float MaximumAltitudeCaptureSinkSpeed = 2.5f;
        private const float MaximumAltitudeAcceleration = 22f;

        private bool _holdingAltitude;
        private float _targetAltitude;

        internal static float CalculateAuthority(float forwardSpeed, float levelFlightSpeed)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(levelFlightSpeed * 0.6f, levelFlightSpeed, forwardSpeed));
        }

        internal void Reset()
        {
            _holdingAltitude = false;
        }

        internal Vector3 CalculateAcceleration(Vector3 velocity, Vector3 forward, float altitude, float pitchInput,
            Vector3 unassistedAcceleration, float authority, float step)
        {
            if (authority <= 0f || step <= 0f)
            {
                Reset();
                return Vector3.zero;
            }

            bool levelFlight = pitchInput == 0f && Mathf.Abs(forward.y) <= LevelNoseThreshold;
            bool holdAltitude = authority >= 0.5f && levelFlight &&
                (_holdingAltitude || velocity.y >= -MaximumAltitudeCaptureSinkSpeed);
            if (holdAltitude && !_holdingAltitude)
                _targetAltitude = altitude;
            _holdingAltitude = holdAltitude;

            // Level controls steer horizontally, leaving an established landing descent alone.
            Vector3 trackingVelocity = levelFlight ? Vector3.ProjectOnPlane(velocity, Vector3.up) : velocity;
            Vector3 trackingTarget = levelFlight ? Vector3.ProjectOnPlane(forward, Vector3.up) : forward;
            Vector3 alignedVelocity = Vector3.RotateTowards(trackingVelocity, trackingTarget.normalized *
                trackingVelocity.magnitude, AlignmentDegreesPerSecond * Mathf.Deg2Rad * step * authority, 0f);
            Vector3 acceleration = (alignedVelocity - trackingVelocity) / step;

            if (holdAltitude)
            {
                float climbRate = Mathf.Clamp((_targetAltitude - altitude) * 0.55f, -6f, 6f);
                float verticalAcceleration = (climbRate - velocity.y) * 1.8f - unassistedAcceleration.y;
                acceleration.y = Mathf.Clamp(verticalAcceleration, -MaximumAltitudeAcceleration,
                    MaximumAltitudeAcceleration) * authority;
            }

            return acceleration;
        }
    }
}
