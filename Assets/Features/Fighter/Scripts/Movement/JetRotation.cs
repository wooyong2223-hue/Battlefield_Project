using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(JetMovement))]
    public class JetRotation : MonoBehaviour
    {
        private const float PitchLimitStartAngle = 17f;
        private const float PitchLimitEndAngle = 24f;
        private const float PitchRecoveryStartAngle = 5f;
        private const float PitchRecoveryFullAngle = 15f;
        private const float PitchRecoveryStartSpeed = 5f;
        private const float PitchRecoveryFullSpeed = 20f;
        private const float MaximumPitchRecoveryAuthority = 0.35f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _pitchSpeed = 1f;
        [SerializeField] private float _rollSpeed = 1f;
        [SerializeField] private float _yawSpeed = 1f;
        [SerializeField, Min(0f)] private float _minimumControlSpeed = 20f;
        [SerializeField, Min(0f)] private float _fullControlSpeed = 60f;
        [SerializeField, Min(0f)] private float _takeoffPitchRate = 12f;
        [SerializeField, Min(0.1f)] private float _takeoffControlClearance = 5f;
        [SerializeField] private FighterLandingGear _landingGear;

        [Header("High Speed Control")]
        [SerializeField, Min(0f), Tooltip("Speed for minimum high-speed control. Zero uses the movement speed limit.")]
        private float _highSpeedControlReference;
        [SerializeField, Min(0f)]
        private float _maximumSpeedControlMultiplier = 0.65f;

        [Header("Turn Energy Loss")]
        [SerializeField, Min(0f)] private float _turnEnergyLossPerSecond = 45f;
        [SerializeField, Min(0f)] private float _pitchEnergyLossMultiplier = 1f;
        [SerializeField, Min(0f)] private float _rollEnergyLossMultiplier = 0.15f;
        [SerializeField, Min(0f)] private float _yawEnergyLossMultiplier = 0.35f;

        private IJetInput _input;
        private JetMovement _movement;
        private Rigidbody _rigidbody;
        private ThrustVectoring _thrustVectoring;
        private FighterCollisionConstraint _collisionConstraint;
        private float _lastGroundedHeight;
        private bool _leavingRunway;

        private void Awake()
        {
            _input = GetComponent<IJetInput>();
            _movement = GetComponent<JetMovement>();
            _rigidbody = GetComponent<Rigidbody>();
            _thrustVectoring = GetComponent<ThrustVectoring>();
            _collisionConstraint = GetComponent<FighterCollisionConstraint>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void FixedUpdate()
        {
            if (_input == null || _movement == null || _rigidbody == null) return;

            bool hasGroundContact = _landingGear != null && _landingGear.TryGetGroundNormal(out _);
            bool hasContact = hasGroundContact || (_collisionConstraint != null && _collisionConstraint.HasContact);
            float takeoffAuthority = UpdateTakeoffAuthority(hasGroundContact);
            float forwardSpeed = _movement.EffectiveForwardSpeed;
            bool allowAirPitchAndRoll = !hasGroundContact && forwardSpeed >= _minimumControlSpeed;
            // A negative rotation around local X raises the nose.
            bool allowTakeoffPitch = hasGroundContact && forwardSpeed > _minimumControlSpeed &&
                _movement.IsEngineRunning && _input.Throttle > 0f && _input.Pitch < 0f;
            float pitchInput = allowAirPitchAndRoll || allowTakeoffPitch ? _input.Pitch : 0f;
            float rollInput = allowAirPitchAndRoll ? _input.Roll : 0f;
            float yawInput = _input.Yaw;

            float controlMultiplier = CalculateControlMultiplier();
            float pitchControlMultiplier = _thrustVectoring != null
                ? _thrustVectoring.GetPitchControlMultiplier(controlMultiplier)
                : controlMultiplier;
            float yawControlMultiplier = _thrustVectoring != null
                ? _thrustVectoring.GetYawControlMultiplier(controlMultiplier)
                : controlMultiplier;

            if (!hasContact && _input.Pitch != 0f)
                AdjustAirPitch(_input.Pitch, ref pitchInput, ref pitchControlMultiplier);

            Vector3 rotationRate = new Vector3(
                pitchInput * _pitchSpeed * pitchControlMultiplier,
                yawInput * _yawSpeed * yawControlMultiplier,
                rollInput * _rollSpeed * controlMultiplier
            ) * _rotationSpeed;
            if (rotationRate.x < 0f && _input.Throttle > 0f && takeoffAuthority < 1f)
                rotationRate.x = Mathf.Lerp(Mathf.Max(rotationRate.x, -_takeoffPitchRate),
                    rotationRate.x, takeoffAuthority);

            if (hasContact)
                ApplyContactAngularVelocity(rotationRate);
            else
                ApplyAngularVelocity(Quaternion.Euler(rotationRate * Time.fixedDeltaTime), Time.fixedDeltaTime);
            ApplyTurnEnergyLoss(pitchInput, rollInput, yawInput, controlMultiplier);
            ApplyThrustVectoringEnergyLoss(pitchInput, yawInput);
        }

        private void AdjustAirPitch(float requestedPitch, ref float pitchInput, ref float pitchControlMultiplier)
        {
            Vector3 localVelocity = Quaternion.Inverse(_rigidbody.rotation) * _rigidbody.linearVelocity;
            float pitchError = Mathf.Atan2(localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.Abs(pitchError);
            float errorChange = requestedPitch * pitchError;

            // Positive errorChange means the requested rotation would increase the gap.
            if (errorChange > 0f)
            {
                float limit = Mathf.InverseLerp(PitchLimitStartAngle, PitchLimitEndAngle, angle);
                pitchInput *= 1f - Mathf.SmoothStep(0f, 1f, limit);
                return;
            }

            if (errorChange >= 0f) return;

            float pitchPlaneSpeed = new Vector2(localVelocity.y, localVelocity.z).magnitude;
            float speedWeight = Mathf.InverseLerp(PitchRecoveryStartSpeed, PitchRecoveryFullSpeed, pitchPlaneSpeed);
            float angleWeight = Mathf.InverseLerp(PitchRecoveryStartAngle, PitchRecoveryFullAngle, angle);
            float recoveryAuthority = MaximumPitchRecoveryAuthority * speedWeight * Mathf.SmoothStep(0f, 1f, angleWeight);
            pitchInput = requestedPitch;
            pitchControlMultiplier = Mathf.Max(pitchControlMultiplier, recoveryAuthority);
        }

        private float UpdateTakeoffAuthority(bool hasGroundContact)
        {
            float height = _rigidbody.position.y;
            if (hasGroundContact)
            {
                _lastGroundedHeight = height;
                _leavingRunway = true;
                return 0f;
            }

            if (!_leavingRunway) return 1f;

            float clearance = Mathf.Max(0.1f, _takeoffControlClearance);
            float progress = Mathf.Clamp01((height - _lastGroundedHeight) / clearance);
            if (progress >= 1f) _leavingRunway = false;
            return Mathf.SmoothStep(0f, 1f, progress);
        }

        private void ApplyContactAngularVelocity(Vector3 rotationRate)
        {
            if (rotationRate.x == 0f && rotationRate.y == 0f && rotationRate.z == 0f) return;

            Quaternion bodyRotation = _rigidbody.rotation;
            Vector3 localAngularVelocity = Quaternion.Inverse(bodyRotation) * _rigidbody.angularVelocity;
            if (rotationRate.x != 0f) localAngularVelocity.x = rotationRate.x * Mathf.Deg2Rad;
            if (rotationRate.y != 0f) localAngularVelocity.y = rotationRate.y * Mathf.Deg2Rad;
            if (rotationRate.z != 0f) localAngularVelocity.z = rotationRate.z * Mathf.Deg2Rad;
            _rigidbody.angularVelocity = bodyRotation * localAngularVelocity;
        }

        private void ApplyAngularVelocity(Quaternion localDelta, float deltaTime)
        {
            Vector3 axis = new Vector3(localDelta.x, localDelta.y, localDelta.z);
            float axisLength = axis.magnitude;
            if (axisLength <= Mathf.Epsilon || deltaTime <= 0f)
            {
                _rigidbody.angularVelocity = Vector3.zero;
                return;
            }

            if (localDelta.w < 0f) axis = -axis;
            float angleRadians = 2f * Mathf.Atan2(axisLength, Mathf.Abs(localDelta.w));
            Vector3 localAngularVelocity = axis * (angleRadians / (axisLength * deltaTime));
            _rigidbody.angularVelocity = _rigidbody.rotation * localAngularVelocity;
        }

        private void OnDisable()
        {
            _leavingRunway = false;
            if (_rigidbody != null && !_rigidbody.isKinematic)
                _rigidbody.angularVelocity = Vector3.zero;
        }

        private float CalculateControlMultiplier()
        {
            float speed = _movement.EffectiveForwardSpeed;
            if (speed <= _fullControlSpeed)
            {
                return Mathf.InverseLerp(
                    _minimumControlSpeed,
                    _fullControlSpeed,
                    speed);
            }

            float referenceSpeed = _highSpeedControlReference > 0f ? _highSpeedControlReference : _movement.MaxSpeed;
            float highSpeedRatio = Mathf.InverseLerp(
                _fullControlSpeed,
                Mathf.Max(_fullControlSpeed, referenceSpeed),
                speed);
            float maximumSpeedMultiplier = Mathf.Clamp01(
                _maximumSpeedControlMultiplier);

            return Mathf.Lerp(
                1f,
                maximumSpeedMultiplier,
                highSpeedRatio);
        }

        private void ApplyTurnEnergyLoss(float pitchInput, float rollInput, float yawInput, float controlMultiplier)
        {
            float weightedTurnInput =
                Mathf.Clamp01(Mathf.Abs(pitchInput)) * _pitchEnergyLossMultiplier +
                Mathf.Clamp01(Mathf.Abs(rollInput)) * _rollEnergyLossMultiplier +
                Mathf.Clamp01(Mathf.Abs(yawInput)) * _yawEnergyLossMultiplier;

            float speedLoss =
                weightedTurnInput *
                controlMultiplier *
                _turnEnergyLossPerSecond *
                Time.fixedDeltaTime;

            _movement.ApplyTurnEnergyLoss(speedLoss);
        }

        private void ApplyThrustVectoringEnergyLoss(float pitchInput, float yawInput)
        {
            if (_thrustVectoring == null)
            {
                return;
            }

            float speedLoss = _thrustVectoring.CalculateEnergyLoss(
                pitchInput,
                yawInput,
                Time.fixedDeltaTime);
            _movement.QueueEnergyLoss(speedLoss);
        }
    }
}
