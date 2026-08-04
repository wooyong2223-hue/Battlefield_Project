using UnityEngine;

namespace Battlefield.Features.Fighter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(JetMovement))]
    public class JetPhysics : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField, Min(0.01f)] private float _gravityThresholdSpeed = 30f;

        [Header("Flight Mode")]
        [SerializeField] private float _flightDrag = 0f;
        [SerializeField] private float _flightMass = 1000f;

        [Header("Falling Mode")]
        [SerializeField] private float _fallingMass = 3000f;
        [SerializeField] private float _fallingDrag = 0f;
        [SerializeField, Min(0f)] private float _fallingGravityMultiplier = 0.75f;

        private Rigidbody _rigidbody;
        private JetMovement _movement;

        private bool _isFalling;
        private bool _isInitialized;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _movement = GetComponent<JetMovement>();
        }

        private void FixedUpdate()
        {
            bool shouldFall = _movement.CurrentSpeed < _gravityThresholdSpeed;

            if (!_isInitialized || shouldFall != _isFalling)
            {
                _isInitialized = true;
                _isFalling = shouldFall;

                if (_isFalling)
                {
                    ApplyFallingMode();
                }
                else
                {
                    ApplyFlightMode();
                }
            }

            if (_isFalling)
            {
                UpdateFallingGravity();
            }
        }

        private void ApplyFallingMode()
        {
            _rigidbody.useGravity = true;
            _rigidbody.mass = _fallingMass;
            _rigidbody.linearDamping = _fallingDrag;
        }

        private void ApplyFlightMode()
        {
            _rigidbody.useGravity = false;
            _rigidbody.mass = _flightMass;
            _rigidbody.linearDamping = _flightDrag;
        }

        private void UpdateFallingGravity()
        {
            float fallRatio = 1f - Mathf.Clamp01(
                _movement.CurrentSpeed / _gravityThresholdSpeed);
            float gravityScale =
                fallRatio * _fallingGravityMultiplier;

            Vector3 gravityCompensation =
                UnityEngine.Physics.gravity * (gravityScale - 1f);

            _rigidbody.AddForce(
                gravityCompensation,
                ForceMode.Acceleration);
        }
    }
}
