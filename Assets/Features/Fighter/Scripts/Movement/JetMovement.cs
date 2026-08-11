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
        [SerializeField] private float _fallRecoveryAcceleration = 20f;
        [SerializeField, Min(0f)] private float _velocityAlignmentSpeed = 90f;

        private Rigidbody _rigidbody;
        private IJetInput _input;
        private FighterCollisionConstraint _collisionConstraint;
        private Afterburner _afterburner;
        private ThrustVectoring _thrustVectoring;
        private float _targetSpeed;
        private float _queuedEnergyLoss;
        private bool _wasUsingGravity;
        private bool _isRecoveringFromFall;

        public float CurrentSpeed { get; private set; }
        public float EffectiveForwardSpeed { get; private set; }
        public float MaxSpeed => _maxSpeed;
        public bool IsUnpowered =>
            _targetSpeed <= _minSpeed + Mathf.Epsilon;

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
            CurrentSpeed = _targetSpeed;
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void Update()
        {
            if (_input == null) return;

            _afterburner.Tick(_input.Afterburner, Time.deltaTime);

            float acceleration =
                _acceleration * _afterburner.AccelerationMultiplier;

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

            bool shouldStop =
                !_afterburner.IsActive &&
                _input.Throttle <= 0f &&
                _targetSpeed <= Mathf.Max(0f, _stopSpeedThreshold);
            if (shouldStop)
            {
                _targetSpeed = 0f;
            }
        }

        private void FixedUpdate()
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            float forwardSpeed = _targetSpeed;

            bool isUnpoweredFall =
                _rigidbody.useGravity &&
                forwardSpeed <= _minSpeed + Mathf.Epsilon;
            if (isUnpoweredFall)
            {
                _queuedEnergyLoss = 0f;
                _wasUsingGravity = true;
                _isRecoveringFromFall = false;

                Vector3 fallingVelocity =
                    _collisionConstraint.ConstrainVelocity(currentVelocity);
                _rigidbody.linearVelocity = fallingVelocity;
                CurrentSpeed = fallingVelocity.magnitude;
                EffectiveForwardSpeed = Mathf.Max(
                    0f,
                    Vector3.Dot(fallingVelocity, transform.forward));
                return;
            }

            Vector3 currentDirection = currentVelocity.sqrMagnitude > Mathf.Epsilon
                ? currentVelocity.normalized
                : transform.forward;
            float velocityAlignmentMultiplier = _thrustVectoring != null
                ? _thrustVectoring.VelocityAlignmentMultiplier
                : 1f;
            float maximumDirectionChange =
                _velocityAlignmentSpeed *
                velocityAlignmentMultiplier *
                Mathf.Deg2Rad *
                Time.fixedDeltaTime;
            Vector3 velocityDirection = Vector3.RotateTowards(
                currentDirection,
                transform.forward,
                maximumDirectionChange,
                0f).normalized;

            float currentMagnitude = Mathf.Max(
                _minSpeed,
                currentVelocity.magnitude - _queuedEnergyLoss);
            _queuedEnergyLoss = 0f;
            float forwardAcceleration =
                _acceleration * _afterburner.AccelerationMultiplier;
            float speedChangeRate = forwardSpeed >= currentMagnitude
                ? forwardAcceleration
                : _deceleration;
            bool preserveStallMomentum =
                _rigidbody.useGravity &&
                forwardSpeed < currentMagnitude;
            float velocityMagnitude = preserveStallMomentum
                ? currentMagnitude
                : Mathf.MoveTowards(
                    currentMagnitude,
                    forwardSpeed,
                    speedChangeRate * Time.fixedDeltaTime);
            Vector3 velocity = velocityDirection * velocityMagnitude;

            if (_rigidbody.useGravity)
            {
                velocity.y = _rigidbody.linearVelocity.y;
                _wasUsingGravity = true;
                _isRecoveringFromFall = false;
            }
            else if (_wasUsingGravity || _isRecoveringFromFall)
            {
                _wasUsingGravity = false;
                _isRecoveringFromFall = true;

                float targetVerticalSpeed = velocity.y;
                velocity.y = Mathf.MoveTowards(
                    _rigidbody.linearVelocity.y,
                    targetVerticalSpeed,
                    _fallRecoveryAcceleration * Time.fixedDeltaTime);

                if (Mathf.Abs(velocity.y - targetVerticalSpeed) < 0.01f)
                {
                    _isRecoveringFromFall = false;
                }
            }

            Vector3 constrainedVelocity =
                _collisionConstraint.ConstrainVelocity(velocity);
            _rigidbody.linearVelocity = constrainedVelocity;
            CurrentSpeed = constrainedVelocity.magnitude;
            EffectiveForwardSpeed = Mathf.Max(
                0f,
                Vector3.Dot(constrainedVelocity, transform.forward));
        }

    }
}
