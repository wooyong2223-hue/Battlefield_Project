using UnityEngine;
using Battlefield.Fighter.Controller;

namespace Battlefield.Fighter.Movement
{
    [RequireComponent(typeof(Rigidbody))]
    public class JetMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _acceleration = 30f;
        [SerializeField] private float _deceleration = 40f;
        [SerializeField] private float _maxSpeed = 300f;
        [SerializeField] private float _minSpeed = 0f;
        [SerializeField] private float _minimumFallingForwardSpeed = 30f;
        [SerializeField] private float _fallRecoveryAcceleration = 20f;

        private Rigidbody _rigidbody;
        private KeyboardJetInput _input;
        private bool _wasUsingGravity;
        private bool _isRecoveringFromFall;

        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<KeyboardJetInput>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void Update()
        {
            if (_input == null) return;

            if (Mathf.Approximately(_input.Throttle, 0f))
            {
                CurrentSpeed = Mathf.MoveTowards(
                    CurrentSpeed,
                    0f,
                    _deceleration * Time.deltaTime);
            }
            else
            {
                CurrentSpeed +=
                    _input.Throttle * _acceleration * Time.deltaTime;
            }

            CurrentSpeed = Mathf.Clamp(CurrentSpeed, _minSpeed, _maxSpeed);
        }

        private void FixedUpdate()
        {
            float forwardSpeed = CurrentSpeed;

            if (_rigidbody.useGravity)
            {
                forwardSpeed = Mathf.Max(
                    forwardSpeed,
                    _minimumFallingForwardSpeed);
            }

            Vector3 velocity = transform.forward * forwardSpeed;

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

            _rigidbody.linearVelocity = velocity;
        }
    }
}
