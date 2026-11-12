using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FighterCollisionConstraint))]
    [RequireComponent(typeof(Afterburner))]
    public class JetMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _acceleration = 30f;
        [SerializeField] private float _deceleration = 40f;
        [SerializeField] private float _maxSpeed = 300f;
        [SerializeField] private float _minSpeed = 0f;
        [SerializeField] private float _stopSpeedThreshold = 50f;
        [SerializeField] private FighterLandingGear _landingGear;
        [SerializeField] private FighterAerodynamics _aerodynamics = new();

        private const float SpeedFeedback = 2f;
        private const float AccelerationFadeStartSpeed = 65f;
        private const float AccelerationFadeEndSpeed = 100f;
        private const float HighSpeedAccelerationMultiplier = 0.8f;
        private readonly FighterFlightAssist _flightAssist = new FighterFlightAssist();

        private Rigidbody _rigidbody;
        private IJetInput _input;
        private FighterCollisionConstraint _collisionConstraint;
        private Afterburner _afterburner;
        private ThrustVectoring _thrustVectoring;
        private float _targetSpeed;
        private float _queuedEnergyLoss;

        public float CurrentSpeed { get; private set; }
        public float EffectiveForwardSpeed { get; private set; }
        public float MaxSpeed => _maxSpeed;
        public bool IsEngineRunning { get; private set; } = true;
        public bool IsUnpowered =>
            !IsEngineRunning || _targetSpeed <= _minSpeed + Mathf.Epsilon;

        public void SetEngineRunning(bool running)
        {
            if (IsEngineRunning == running) return;
            IsEngineRunning = running;
            if (running) return;
            _targetSpeed = 0f;
            _queuedEnergyLoss = 0f;
            _afterburner?.Stop();
            _landingGear?.SetRollingAssist(false);
        }

        public void ApplyTurnEnergyLoss(float speedLoss)
        {
            if (speedLoss <= 0f) return;

            _targetSpeed = Mathf.Max(_minSpeed, _targetSpeed - speedLoss);
        }

        public void QueueEnergyLoss(float speedLoss)
        {
            if (speedLoss <= 0f) return;

            _targetSpeed = Mathf.Max(_minSpeed, _targetSpeed - speedLoss);
            _queuedEnergyLoss += speedLoss;
        }

        public void StopImmediately()
        {
            _targetSpeed = 0f;
            _afterburner.Stop();
            _landingGear?.SetRollingAssist(false);
            CurrentSpeed = 0f;
            EffectiveForwardSpeed = 0f;
            _queuedEnergyLoss = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<IJetInput>();
            _collisionConstraint = GetComponent<FighterCollisionConstraint>();
            _afterburner = GetComponent<Afterburner>();
            _thrustVectoring = GetComponent<ThrustVectoring>();
            _targetSpeed = Mathf.Clamp(
                Vector3.Dot(_rigidbody.linearVelocity, transform.forward),
                _minSpeed,
                _maxSpeed);
            UpdateSpeedReadouts(_rigidbody.linearVelocity, transform.forward);
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void OnDisable()
        {
            _afterburner?.Stop();
            _flightAssist.Reset();
            _landingGear?.SetRollingAssist(false);
        }

        private void Update()
        {
            if (_input == null) return;

            if (!IsEngineRunning)
            {
                _afterburner.Tick(false, Time.deltaTime);
                return;
            }

            _afterburner.Tick(_input.Afterburner, Time.deltaTime);

            float acceleration = CalculateAvailableAcceleration();

            float thrustInput = _afterburner.IsActive
                ? 1f
                : Mathf.Max(0f, _input.Throttle);
            _targetSpeed +=
                thrustInput * acceleration * Time.deltaTime;

            if (_input.Throttle < 0f)
            {
                _targetSpeed +=
                    _input.Throttle * _deceleration * Time.deltaTime;
            }

            float maximumSpeed =
                _maxSpeed * _afterburner.MaxSpeedMultiplier;
            _targetSpeed = Mathf.Clamp(
                _targetSpeed,
                _minSpeed,
                maximumSpeed);
        }

        private void FixedUpdate()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            Quaternion attitude = _rigidbody.rotation;
            Vector3 forward = attitude * Vector3.forward;
            UpdateSpeedReadouts(velocity, forward);

            Vector3 groundNormal = Vector3.up;
            bool isGrounded = _landingGear != null && _landingGear.TryGetGroundNormal(out groundNormal);
            float throttle = IsEngineRunning && _input != null ? _input.Throttle : 0f;
            bool boosting = IsEngineRunning && _afterburner.IsActive;

            if (isGrounded && !boosting && throttle <= 0f &&
                _targetSpeed <= Mathf.Max(0f, _stopSpeedThreshold))
            {
                _targetSpeed = 0f;
            }

            float sideSlipMultiplier = _thrustVectoring != null
                ? _thrustVectoring.VelocityAlignmentMultiplier
                : 1f;
            bool useCoastDrag = !isGrounded && throttle <= 0f && !boosting;
            bool usePitchAssist = IsEngineRunning && !isGrounded && !_collisionConstraint.HasContact;
            float pitchInput = _input != null ? _input.Pitch : 0f;
            float directFlightWeight = usePitchAssist
                ? FighterFlightAssist.CalculateAuthority(EffectiveForwardSpeed, _aerodynamics.LevelFlightSpeed)
                : 0f;
            directFlightWeight *= Mathf.Clamp01(sideSlipMultiplier);
            Vector3 acceleration = _aerodynamics.CalculateAcceleration(
                velocity, attitude, Physics.gravity.magnitude, sideSlipMultiplier, useCoastDrag, usePitchAssist,
                pitchInput, directFlightWeight);

            float thrust = IsEngineRunning ? CalculateThrust(throttle, boosting, isGrounded) : 0f;
            _landingGear?.SetRollingAssist(isGrounded && thrust > 0f);
            if (IsEngineRunning)
            {
                acceleration += forward * thrust;
                acceleration += CalculateBraking(velocity, groundNormal, isGrounded, throttle);
            }

            acceleration += _flightAssist.CalculateAcceleration(velocity, forward, _rigidbody.worldCenterOfMass.y, pitchInput,
                acceleration + Physics.gravity, directFlightWeight, Time.fixedDeltaTime);
            _queuedEnergyLoss = 0f;
            _rigidbody.AddForce(_collisionConstraint.ConstrainAcceleration(acceleration), ForceMode.Acceleration);
        }

        private float CalculateAvailableAcceleration()
        {
            float progress = Mathf.InverseLerp(AccelerationFadeStartSpeed, AccelerationFadeEndSpeed,
                EffectiveForwardSpeed);
            float multiplier = Mathf.SmoothStep(1f, HighSpeedAccelerationMultiplier, progress);
            return _acceleration * multiplier * _afterburner.AccelerationMultiplier;
        }

        private float CalculateThrust(float throttle, bool boosting, bool isGrounded)
        {
            if (isGrounded ? throttle < 0f : throttle <= 0f && !boosting) return 0f;

            float speedLimit = _maxSpeed * _afterburner.MaxSpeedMultiplier;
            float accelerationLimit = Mathf.Max(0f, CalculateAvailableAcceleration());
            float demand = boosting ? 1f : Mathf.Clamp01(throttle);
            float speedError = Mathf.Min(_targetSpeed, speedLimit) - EffectiveForwardSpeed;
            float thrust = Mathf.Clamp(accelerationLimit * demand + speedError * SpeedFeedback, 0f, accelerationLimit);
            return Mathf.Min(thrust, Mathf.Max(0f, speedLimit - EffectiveForwardSpeed) / Time.fixedDeltaTime);
        }

        private Vector3 CalculateBraking(Vector3 velocity, Vector3 groundNormal, bool isGrounded, float throttle)
        {
            float brakeInput = Mathf.Clamp01(-throttle);
            Vector3 brakeVelocity = isGrounded ? Vector3.ProjectOnPlane(velocity, groundNormal) : velocity;
            float brakeSpeed = brakeVelocity.magnitude;
            if (brakeSpeed < 0.01f) return Vector3.zero;

            float brake = Mathf.Max(0f, _deceleration) * brakeInput;
            if (!isGrounded)
                brake *= Mathf.Clamp01(EffectiveForwardSpeed / _aerodynamics.LevelFlightSpeed);

            float requestedLoss = brake + _queuedEnergyLoss / Time.fixedDeltaTime;
            float loss = Mathf.Min(brakeSpeed / Time.fixedDeltaTime, requestedLoss);
            return -brakeVelocity * (loss / brakeSpeed);
        }

        private void UpdateSpeedReadouts(Vector3 velocity, Vector3 forward)
        {
            CurrentSpeed = velocity.magnitude;
            EffectiveForwardSpeed = Mathf.Max(0f, Vector3.Dot(velocity, forward));
        }
    }
}
