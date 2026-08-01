using UnityEngine;
using System.Collections.Generic;
using Battlefield.Fighter.Controller;

namespace Battlefield.Fighter.Movement
{
    [RequireComponent(typeof(Rigidbody))]
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

        private Rigidbody _rigidbody;
        private KeyboardJetInput _input;
        private readonly Dictionary<Collider, Vector3> _contactNormals = new();
        private bool _wasUsingGravity;
        private bool _isRecoveringFromFall;

        public float CurrentSpeed { get; private set; }
        public float EffectiveForwardSpeed { get; private set; }
        public float MaxSpeed => _maxSpeed;

        public void StopImmediately()
        {
            CurrentSpeed = 0f;
            EffectiveForwardSpeed = 0f;
            _rigidbody.linearVelocity = Vector3.zero;
        }

        public void ApplyCollisionImpact(
            Collider target,
            Vector3 contactNormal)
        {
            TrySetContactNormal(target, contactNormal);
        }

        public void MaintainCollisionContact(
            Collider target,
            Vector3 contactNormal)
        {
            TrySetContactNormal(target, contactNormal);
        }

        public void EndCollisionContact(Collider target)
        {
            if (target != null)
            {
                _contactNormals.Remove(target);
            }
        }

        public void ClearCollisionContacts()
        {
            _contactNormals.Clear();
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _input = GetComponent<KeyboardJetInput>();
            if (_input == null) Debug.Log($"{nameof(IJetInput)} is missing", this);
        }

        private void OnDisable()
        {
            ClearCollisionContacts();
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

            bool isAirborneFalling =
                _rigidbody.useGravity &&
                _rigidbody.linearVelocity.y < FallingVelocityThreshold;

            if (isAirborneFalling)
            {
                forwardSpeed = Mathf.Max(
                    forwardSpeed,
                    _minimumFallingForwardSpeed);
            }

            EffectiveForwardSpeed = forwardSpeed;

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

            foreach (Vector3 contactNormal in _contactNormals.Values)
            {
                float inwardSpeed = Vector3.Dot(
                    velocity,
                    contactNormal);
                if (inwardSpeed < 0f)
                {
                    velocity -= contactNormal * inwardSpeed;
                }
            }

            _rigidbody.linearVelocity = velocity;
        }

        private bool TrySetContactNormal(
            Collider target,
            Vector3 contactNormal)
        {
            if (target == null || contactNormal.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            _contactNormals[target] = contactNormal.normalized;
            return true;
        }
    }
}
