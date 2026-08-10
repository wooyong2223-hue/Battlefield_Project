using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(FighterCollisionConstraint))]
    [RequireComponent(typeof(Afterburner))]
    public class JetMovement : MonoBehaviour
    {
        private const float FallingVelocityThreshold = -0.5f;

        [Header("Movement")]
        [SerializeField] private float _acceleration = 30f;
        [SerializeField] private float _deceleration = 40f;
        [SerializeField] private float _maxSpeed = 300f;
        [SerializeField] private float _minSpeed = 0f;
        [SerializeField] private float _minimumFallingForwardSpeed = 30f;
        [SerializeField] private float _fallRecoveryAcceleration = 20f;
        [SerializeField, Min(0f)] private float _velocityAlignmentSpeed = 90f;

        private Rigidbody _rigidbody;
        private IJetInput _input;
        private FighterCollisionConstraint _collisionConstraint;
        private Afterburner _afterburner;
        private float _targetSpeed;
        private bool _wasUsingGravity;
        private bool _isRecoveringFromFall;

        public float CurrentSpeed { get; private set; }
        public float EffectiveForwardSpeed { get; private set; }
        public float MaxSpeed => _maxSpeed;

        public void ApplyTurnEnergyLoss(float speedLoss)
        {
            if (speedLoss <= 0f) return;

            _targetSpeed = Mathf.Max(_minSpeed, _targetSpeed - speedLoss);
        }

        public void StopImmediately()
        {
            _targetSpeed = 0f;
            _afterburner.Stop();
            CurrentSpeed = 0f;
            EffectiveForwardSpeed = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<IJetInput>();
            _collisionConstraint = GetComponent<FighterCollisionConstraint>();
            _afterburner = GetComponent<Afterburner>();
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

            if (_input.Throttle > 0f)
            {
                _targetSpeed +=
                    _input.Throttle * acceleration * Time.deltaTime;
            }
            else if (_input.Throttle < 0f)
            {
                _targetSpeed +=
                    _input.Throttle * _deceleration * Time.deltaTime;
            }

            float maximumSpeed =
                _maxSpeed * _afterburner.MaxSpeedMultiplier;
            _targetSpeed = Mathf.Clamp(_targetSpeed, _minSpeed, maximumSpeed);
        }

        private void FixedUpdate()
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            float forwardSpeed = _targetSpeed;

            bool isAirborneFalling =
                _rigidbody.useGravity &&
                _rigidbody.linearVelocity.y < FallingVelocityThreshold;

            if (isAirborneFalling)
            {
                forwardSpeed = Mathf.Max(
                    forwardSpeed,
                    _minimumFallingForwardSpeed);
            }

            Vector3 currentDirection = currentVelocity.sqrMagnitude > Mathf.Epsilon
                ? currentVelocity.normalized
                : transform.forward;
            float maximumDirectionChange =
                _velocityAlignmentSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime;
            Vector3 velocityDirection = Vector3.RotateTowards(
                currentDirection,
                transform.forward,
                maximumDirectionChange,
                0f).normalized;

            float currentMagnitude = currentVelocity.magnitude;
            float forwardAcceleration =
                _acceleration * _afterburner.AccelerationMultiplier;
            float speedChangeRate = forwardSpeed >= currentMagnitude
                ? forwardAcceleration
                : _deceleration;
            float velocityMagnitude = Mathf.MoveTowards(
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
            CurrentSpeed = Mathf.Max(
                0f,
                Vector3.Dot(constrainedVelocity, transform.forward));
            EffectiveForwardSpeed = CurrentSpeed;
        }
    }
}
